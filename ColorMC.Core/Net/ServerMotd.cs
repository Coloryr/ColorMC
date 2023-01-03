using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using Heijden.Dns.Portable;
using Heijden.DNS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Net;

public class ServerDescriptionJsonConverter : JsonConverter<Chat>
{
    public override Chat? ReadJson(JsonReader reader, Type objectType, Chat? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            return new Chat() { Text = reader.Value.ToString() };
        }
        else
        {
            JObject obj = JObject.Load(reader);
            Chat chat = new();
            serializer.Populate(obj.CreateReader(), chat);
            return chat;
        }
    }

    public override void WriteJson(JsonWriter writer, Chat? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}

public static class ServerMotd
{
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
    public static Dictionary<char, string> MinecraftColors { get; private set; } = new Dictionary<char, string>()
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

    public static async Task StartGetServerInfo(this ServerInfo info)
    {
        try
        {
            TcpClient tcp = null;

            try
            {
                tcp = new TcpClient(info.ServerAddress, info.ServerPort);
            }
            catch (SocketException ex)
            {
                var data = await new Resolver().Query("_minecraft._tcp." + info.ServerAddress, QType.SRV);
                RecordSRV? result = data.Answers?.FirstOrDefault()?.RECORD as RecordSRV;
                if (result != null)
                {
                    tcp = new TcpClient(result.TARGET, result.PORT);
                    info.ServerAddress = result.TARGET;
                    info.ServerPort = result.PORT;
                }
                else
                {
                    info.State = StateType.BAD_CONNECT;
                    info.Message = ex.Message;
                    return;
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
                byte[] packet_id = ProtocolHandler.getVarInt(0);

                byte[] protocol_version = ProtocolHandler.getVarInt(-1);
                byte[] server_adress_val = Encoding.UTF8.GetBytes(info.ServerAddress);
                byte[] server_adress_len = ProtocolHandler.getVarInt(server_adress_val.Length);
                byte[] server_port = BitConverter.GetBytes(info.ServerPort);
                Array.Reverse(server_port);
                byte[] next_state = ProtocolHandler.getVarInt(1);

                byte[] concat_pack = ProtocolHandler.concatBytes(packet_id, protocol_version, server_adress_len, server_adress_val, server_port, next_state);
                byte[] tosend = ProtocolHandler.concatBytes(ProtocolHandler.getVarInt(concat_pack.Length), concat_pack);

                tcp.Client.Send(tosend, SocketFlags.None);
                #endregion

                // Request
                byte[] status_request = ProtocolHandler.getVarInt(0);
                byte[] request_packet = ProtocolHandler.concatBytes(ProtocolHandler.getVarInt(status_request.Length), status_request);

                tcp.Client.Send(request_packet, SocketFlags.None);

                // Response
                ProtocolHandler handler = new ProtocolHandler(tcp);
                int packetLength = handler.readNextVarIntRAW();
                if (packetLength > 0)
                {
                    List<byte> packetData = new List<byte>(handler.readDataRAW(packetLength));
                    if (ProtocolHandler.readNextVarInt(packetData) == 0x00) //Read Packet ID
                    {
                        string result = ProtocolHandler.readNextString(packetData); //Get the Json data
                        JsonConvert.PopulateObject(result, info);
                    }
                }

                #region Ping
                byte[] ping_id = ProtocolHandler.getVarInt(1);
                byte[] ping_content = BitConverter.GetBytes((long)233);
                byte[] ping_packet = ProtocolHandler.concatBytes(ping_id, ping_content);
                byte[] ping_tosend = ProtocolHandler.concatBytes(ProtocolHandler.getVarInt(ping_packet.Length), ping_packet);

                try
                {
                    tcp.ReceiveTimeout = 1000;

                    Stopwatch pingWatcher = new Stopwatch();

                    pingWatcher.Start();
                    tcp.Client.Send(ping_tosend, SocketFlags.None);

                    int pingLenghth = handler.readNextVarIntRAW();
                    pingWatcher.Stop();
                    if (pingLenghth > 0)
                    {
                        List<byte> packetData = new List<byte>(handler.readDataRAW(pingLenghth));
                        if (ProtocolHandler.readNextVarInt(packetData) == 0x01) //Read Packet ID
                        {
                            long content = ProtocolHandler.readNextByte(packetData); //Get the Json data
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
                #endregion
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
    }

    public static string ClearColor(string str)
    {
        str = str.Replace(@"\n", "\n");
        while (str.Contains('§'))
        {
            str = str.Remove(str.IndexOf('§'), 2);
        }
        return str.Trim();
    }
}
