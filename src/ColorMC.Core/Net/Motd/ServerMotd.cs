using ColorMC.Core.Objs.Minecraft;
using Heijden.Dns.Portable;
using Heijden.DNS;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace ColorMC.Core.Net.Motd;

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

    /// <summary>
    /// 获取服务器信息
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    public static async Task<ServerMotdObj> GetServerInfo(string ip, ushort port)
    {
        if (port == 0)
        {
            port = 25565;
        }
        var info = new ServerMotdObj(ip, port);
        try
        {
            TcpClient tcp;
            try
            {
                tcp = new TcpClient(ip, port);
            }
            catch (SocketException ex)
            {
                var data = await new Resolver().Query("_minecraft._tcp." + ip, QType.SRV);
                var result = data.Answers?.FirstOrDefault()?.RECORD as RecordSRV;
                if (result != null)
                {
                    tcp = new TcpClient(result.TARGET, result.PORT);
                    ip = result.TARGET;
                    port = result.PORT;
                }
                else
                {
                    info.State = StateType.BAD_CONNECT;
                    info.Message = ex.Message;
                    return info;
                }
            }

            tcp.ReceiveBufferSize = 1024 * 1024;

            try
            {
                #region Handshake
                /*
                *  packid  filed name          filed type      notes
                *  
                *          Protocol Version    VarInt          See protocol version numbers. The version that the client plans on using to connect to the server (which is not important for the ping). If the client is pinging to determine what version to use, by convention -1 should be set.
                *  
                *          Server Address      String      	Hostname or IP, e.g. localhost or 127.0.0.1, that was used to connect. The Notchian server does not use this information. Note that SRV records are a complete redirect, e.g. if _minecraft._tcp.example.com points to mc.example.org, users connecting to example.com will provide mc.example.org as server address in addition to connecting to it.
                *  0x00      
                *          Server Port         Unsigned Short  Default is 25565. The Notchian server does not use this information.
                *  
                *          Next state          VarInt          Should be 1 for status, but could also be 2 for login.
                *  
                */

                // Handshake:
                // Packet ID: 0x00
                byte[] packet_id = ProtocolHandler.GetVarInt(0);

                byte[] protocol_version = ProtocolHandler.GetVarInt(754);
                byte[] server_adress_val = Encoding.UTF8.GetBytes(ip);
                byte[] server_adress_len = ProtocolHandler.GetVarInt(server_adress_val.Length);
                byte[] server_port = BitConverter.GetBytes(port);
                Array.Reverse(server_port);
                byte[] next_state = ProtocolHandler.GetVarInt(1);

                byte[] concat_pack = ProtocolHandler.ConcatBytes(packet_id, protocol_version, server_adress_len, server_adress_val, server_port, next_state);
                byte[] tosend = ProtocolHandler.ConcatBytes(ProtocolHandler.GetVarInt(concat_pack.Length), concat_pack);

                tcp.Client.Send(tosend, SocketFlags.None);
                #endregion

                // Request
                byte[] status_request = ProtocolHandler.GetVarInt(0);
                byte[] request_packet = ProtocolHandler.ConcatBytes(ProtocolHandler.GetVarInt(status_request.Length), status_request);

                tcp.Client.Send(request_packet, SocketFlags.None);

                // Response
                ProtocolHandler handler = new(tcp);
                int packetLength = handler.ReadNextVarIntRAW();
                if (packetLength > 0)
                {
                    List<byte> packetData = new(handler.ReadDataRAW(packetLength));
                    if (ProtocolHandler.ReadNextVarInt(packetData) == 0x00) //Read Packet ID
                    {
                        string result = ProtocolHandler.ReadNextString(packetData); //Get the Json data
                        JsonConvert.PopulateObject(result, info);
                    }
                }

                byte[] ping_id = ProtocolHandler.GetVarInt(1);
                byte[] ping_content = BitConverter.GetBytes((long)233);
                byte[] ping_packet = ProtocolHandler.ConcatBytes(ping_id, ping_content);
                byte[] ping_tosend = ProtocolHandler.ConcatBytes(ProtocolHandler.GetVarInt(ping_packet.Length), ping_packet);

                try
                {
                    tcp.ReceiveTimeout = 1000;

                    Stopwatch pingWatcher = new();

                    pingWatcher.Start();
                    tcp.Client.Send(ping_tosend, SocketFlags.None);

                    int pingLenghth = handler.ReadNextVarIntRAW();
                    pingWatcher.Stop();
                    if (pingLenghth > 0)
                    {
                        List<byte> packetData = new(handler.ReadDataRAW(pingLenghth));
                        if (ProtocolHandler.ReadNextVarInt(packetData) == 0x01) //Read Packet ID
                        {
                            long content = ProtocolHandler.ReadNextByte(packetData); //Get the Json data
                            if (content == 233)
                            {
                                info.Ping = pingWatcher.ElapsedMilliseconds;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    info.Ping = 0;
                }
            }
            catch (SocketException)
            {
                info.State = StateType.NO_RESPONSE;
            }
            tcp.Close();
        }
        catch (SocketException ex)
        {
            info.State = StateType.BAD_CONNECT;
            info.Message = ex.Message;
        }
        catch (Exception ex)
        {
            info.State = StateType.EXCEPTION;
            info.Message = ex.Message;
        }

        return info;
    }
}
