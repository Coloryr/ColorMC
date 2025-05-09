using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Ae.Dns.Client;
using Ae.Dns.Protocol;
using Ae.Dns.Protocol.Enums;
using Ae.Dns.Protocol.Records;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Core.Utils;

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
    public static string ToPlainTextString(this ChatObj chat)
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
                IPAddress? selectDns = null;
                //获取系统Dns服务器
                var list = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var adapter in list.Where(item => item.OperationalStatus is OperationalStatus.Up && item.NetworkInterfaceType is NetworkInterfaceType.Wireless80211 or NetworkInterfaceType.Ethernet && item.Speed > 0))
                {
                    if (selectDns != null)
                    {
                        break;
                    }
                    IPInterfaceProperties properties = adapter.GetIPProperties();

                    if (properties.GatewayAddresses.Count > 0 &&
                        !properties.GatewayAddresses[0].Address.ToString().StartsWith("127."))
                    {
                        foreach (IPAddress dnsAddress in properties.DnsAddresses)
                        {
                            selectDns = dnsAddress;
                            break;
                        }
                    }
                }
                if (selectDns == null)
                {
                    info.State = StateType.Error;

                    return info;
                }
                //进行SRV请求
                using var s_dnsClient = new DnsUdpClient(selectDns);
                var data = await s_dnsClient.Query(DnsQueryFactory.CreateQuery("_minecraft._tcp." + ip, DnsQueryType.SRV), CancellationToken.None);
                if (data.Answers?.FirstOrDefault() is { } result)
                {
                    var result1 = result.Resource as DnsUnknownResource;
                    tcp = new TcpClient()
                    {
                        ReceiveTimeout = 5000,
                        SendTimeout = 5000
                    };
                    var rawData = result1!.Raw.ToArray();
                    int offset = 4;
                    port = (ushort)((rawData[offset] << 8) | rawData[offset + 1]);
                    offset += 2;

                    var targetBuilder = new StringBuilder();
                    while (offset < rawData.Length)
                    {
                        byte length = rawData[offset];
                        offset++;

                        if (length == 0)
                        {
                            break;
                        }

                        targetBuilder.Append(Encoding.ASCII.GetString(rawData, offset, length))
                            .Append('.');
                        offset += length;
                    }

                    ip = targetBuilder.ToString();
                    ip = ip[..^1];

                    tcp.Connect(ip, port);
                }
                else
                {
                    info.State = StateType.ConnectFail;
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
                List<byte> packetData = [.. handler.ReadDataRAW(packetLength)];

                if (ProtocolHandler.ReadNextVarInt(packetData) == 0x00) //Read Packet ID
                {
                    string result = ProtocolHandler.ReadNextString(packetData); //Get the Json data

                    var doc = JsonDocument.Parse(result);
                    foreach (var item in doc.RootElement.EnumerateObject())
                    {
                        if (item.Name == "version")
                        {
                            info.Version = item.Value.Deserialize(JsonType.ServerVersionInfoObj);
                        }
                        else if (item.Name == "players")
                        {
                            info.Players = item.Value.Deserialize(JsonType.ServerPlayerInfoObj);
                        }
                        else if (item.Name == "modinfo")
                        {
                            info.ModInfo = item.Value.Deserialize(JsonType.ServerMotdModInfoObj);
                        }
                        else if (item.Name == "favicon")
                        {
                            info.Favicon = item.Value.GetString();
                        }
                        else if (item.Name == "description")
                        {
                            if (item.Value.ValueKind == JsonValueKind.String)
                            {
                                info.Description = ChatConverter.StringToChar(item.Value.GetString());
                            }
                            else
                            {
                                info.Description = item.Value.Deserialize(JsonType.ChatObj)!;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(info.Description.Text)
                        && info.Description.Extra == null && info.Description.Text.Contains('§'))
                    {
                        info.Description = ChatConverter.StringToChar(info.Description.Text);
                    }
                }
            }
            tcp.Close();
            tcp.Dispose();
        }
        catch (Exception ex)
        {
            info.State = StateType.Error;
            info.Message = ex.Message;
        }

        return info;
    }
}
