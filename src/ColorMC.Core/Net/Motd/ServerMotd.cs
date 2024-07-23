using System.Net.Sockets;
using System.Text;
using ColorMC.Core.Objs.Minecraft;
using Heijden.Dns.Portable;
using Heijden.DNS;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Motd;

/// <summary>
/// 获取Motd
/// </summary>
public static class ServerMotd
{
    /// <summary>
    /// 转字符串
    /// </summary>
    /// <param name="chat"></param>
    /// <returns></returns>
    public static string ToPlainTextString(this Chat chat)
    {
        StringBuilder stringBuilder = new(chat.Text);
        if (chat.Extra != null)
        {
            foreach (var item in chat.Extra)
            {
                stringBuilder.Append(item.ToPlainTextString());
            }
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 清除格式
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string CleanFormat(string str)
    {
        str = str.Replace(@"\n", "\n");

        int index;
        do
        {
            index = str.IndexOf('§');
            if (index >= 0)
            {
                str = str.Remove(index, 2);
            }
        } while (index >= 0);

        return str.Trim();
    }

    /// <summary>
    /// 获取与特定格式代码相关联的颜色代码
    /// </summary>
    public static Dictionary<char, string> MinecraftColors { get; private set; } =
        new Dictionary<char, string>()
    {
        { '0', "#000000" },
        { '1', "#0000AA" },
        { '2', "#00AA00" },
        { '3', "#00AAAA" },
        { '4', "#AA0000" },
        { '5', "#AA00AA" },
        { '6', "#FFAA00" },
        { '7', "#AAAAAA" },
        { '8', "#555555" },
        { '9', "#5555FF" },
        { 'a', "#55FF55" },
        { 'b', "#55FFFF" },
        { 'c', "#FF5555" },
        { 'd', "#FF55FF" },
        { 'e', "#FFFF55" },
        { 'f', "#FFFFFF" }
    };

    public static readonly Dictionary<string, string> ColorMap = new()
    {
        { "black", "#000000" },
        { "dark_blue", "#0000aa" },
        { "dark_green", "#00aa00" },
        { "dark_aqua", "#000000" },
        { "dark_red", "#aa0000" },
        { "dark_purple", "#aa00aa" },
        { "gold", "#ffaa00" },
        { "gray", "#aaaaaa" },
        { "dark_gray", "#555555" },
        { "blue", "#5555ff" },
        { "green", "#55ff55" },
        { "aqua", "#55ffff" },
        { "red", "#ff5555" },
        { "light_purple", "#ff55ff" },
        { "yellow", "#ffff55" },
        { "white", "#ffffff" }
    };

    /// <summary>
    /// 获取服务器信息
    /// </summary>
    /// <param name="ip">地址</param>
    /// <param name="port">端口</param>
    /// <returns>服务器信息</returns>
    public static async Task<ServerMotdObj> GetServerInfo(string ip, ushort port)
    {
        var info = new ServerMotdObj(ip, port);
        var tcp = new TcpClient()
        {
            ReceiveTimeout = 5000,
            SendTimeout = 5000
        };
        try
        {
            try
            {
                tcp.Connect(ip, port);
            }
            catch (SocketException ex)
            {
                tcp.Close();
                tcp.Dispose();
                var data = await new Resolver().Query("_minecraft._tcp." + ip, QType.SRV);
                if (data.Answers?.FirstOrDefault()?.RECORD is RecordSRV result)
                {
                    tcp = new TcpClient()
                    {
                        ReceiveTimeout = 5000,
                        SendTimeout = 5000
                    };
                    tcp.Connect(ip = result.TARGET[..^1], port = result.PORT);
                }
                else
                {
                    info.State = StateType.BAD_CONNECT;
                    info.Message = ex.Message;
                    return info;
                }
            }

            tcp.ReceiveBufferSize = 1024 * 1024;

            byte[] packet_id = ProtocolHandler.GetVarInt(0);

            byte[] protocol_version = ProtocolHandler.GetVarInt(754);
            byte[] server_adress_val = Encoding.UTF8.GetBytes(ip);
            byte[] server_adress_len = ProtocolHandler.GetVarInt(server_adress_val.Length);
            byte[] server_port = BitConverter.GetBytes(port);
            Array.Reverse(server_port);
            byte[] next_state = ProtocolHandler.GetVarInt(1);

            byte[] concat_pack = ProtocolHandler.ConcatBytes(packet_id, protocol_version, server_adress_len, server_adress_val, server_port, next_state);
            byte[] tosend = ProtocolHandler.ConcatBytes(ProtocolHandler.GetVarInt(concat_pack.Length), concat_pack);

            // Request
            byte[] status_request = ProtocolHandler.GetVarInt(0);
            byte[] request_packet = ProtocolHandler.ConcatBytes(ProtocolHandler.GetVarInt(status_request.Length), status_request);

            ProtocolHandler handler = new(tcp);
            tcp.Client.Send(tosend, SocketFlags.None);
            tcp.Client.Send(request_packet, SocketFlags.None);

            // Response

            int packetLength = handler.ReadNextVarIntRAW();
            info.Ping = handler.PingWatcher.ElapsedMilliseconds;
            if (packetLength > 0)
            {
                List<byte> packetData = new(handler.ReadDataRAW(packetLength));

                if (ProtocolHandler.ReadNextVarInt(packetData) == 0x00) //Read Packet ID
                {
                    string result = ProtocolHandler.ReadNextString(packetData); //Get the Json data
                    JsonConvert.PopulateObject(result, info);

                    if (!string.IsNullOrEmpty(info.Description.Text)
                        && info.Description.Extra == null && info.Description.Text.Contains('§'))
                    {
                        info.Description = ServerDescriptionJsonConverter.StringToChar(info.Description.Text);
                    }
                }
            }
            tcp.Close();
            tcp.Dispose();
        }
        catch (Exception ex)
        {
            info.State = StateType.EXCEPTION;
            info.Message = ex.Message;
        }

        return info;
    }
}
