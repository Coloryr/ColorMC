using System.Collections.Generic;
using System.Text;
using System.Threading;
using ColorMC.Core;
using ColorMC.Core.Asm;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Gui.UI.Model.Items;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace ColorMC.Gui.Utils;

public static class GameSocket
{
    public static readonly List<NetFrpCloudServerModel> Servers = [];

    public static int Port { get; private set; }

    private static bool s_isRun;

    private static NettyServer s_nettyServer; 

    public static async void Init()
    {
        s_nettyServer = new();
        s_nettyServer.NettyPack += ReadPack;
        Port = await s_nettyServer.RunServerAsync();
        App.OnClose += App_OnClose;
        s_isRun = true;
        new Thread(Run).Start();
    }

    private static void Run()
    {
        while (s_isRun)
        {
            if (Servers.Count != 0)
            {
                foreach (var item in Servers.ToArray())
                {
                    SendServerInfo(item);
                }
            }
            Thread.Sleep(2000);
        }
    }

    private static void App_OnClose()
    {
        s_nettyServer.Stop();
        s_isRun = false;
    }

    private static string ReadString(this IByteBuffer buf)
    {
        int size = buf.ReadInt();
        var datas = new byte[size];
        buf.ReadBytes(datas);
        return Encoding.UTF8.GetString(datas);
    }

    private static void ReadPack(IChannel channel, IByteBuffer buffer)
    {
        try
        {
            int type = buffer.ReadInt();
            string uuid = buffer.ReadString();

            if (type == 1)
            {
                var value = buffer.ReadBoolean();
                GameJoystick.SetMouse(uuid, value);
            }
        }
        catch
        {

        }
    }

    public static void Clear()
    {
        Servers.Clear();
    }

    public static void AddServerInfo(NetFrpCloudServerModel model)
    {
        Servers.Add(model);
    }

    private static void WriteString(this IByteBuffer buf, string data)
    {
        var temp = Encoding.UTF8.GetBytes(data);
        buf.WriteInt(temp.Length);
        buf.WriteBytes(temp);
    }

    private static void SendServerInfo(NetFrpCloudServerModel model)
    {
        var buf = Unpooled.Buffer();

        var temp = model.IP.Split(':');

        buf.WriteInt(2);
        buf.WriteString(temp[0]);
        buf.WriteString(temp[1]);
        buf.WriteString(LanGameHelper.MakeMotd("[ColorMC]" + model.Name, temp[1]));
        s_nettyServer.SendMessage(buf);
    }
}
