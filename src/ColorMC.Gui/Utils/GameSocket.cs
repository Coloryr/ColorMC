using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace ColorMC.Gui.Utils;

public static class GameSocket
{
    public static readonly List<NetFrpCloudServerModel> Servers = [];

    public static int Port { get; private set; }

    private const int TypeGameMouseState = 1;
    private const int TypeGameMotd = 2;
    private const int TypeLaunchShow = 3;
    private const int TypeLaunchStart = 4;

    private static bool s_isRun;

    private static IEventLoopGroup _bossGroup;
    private static IEventLoopGroup _workerGroup;
    private static ServerBootstrap _bootstrap;

    private static IChannel _channel;

    /// <summary>
    /// �ͻ����ŵ�
    /// </summary>
    private static readonly List<IChannel> _channels = [];

    /// <summary>
    /// ��ȡ��������ʹ�õĶ˿�
    /// </summary>
    /// <returns>�˿��б�</returns>
    private static List<int> PortIsUsed()
    {
        var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        var ipsTCP = ipGlobalProperties.GetActiveTcpListeners();
        var ipsUDP = ipGlobalProperties.GetActiveUdpListeners();
        var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

        var allPorts = new List<int>();
        foreach (var ep in ipsTCP) allPorts.Add(ep.Port);
        foreach (var ep in ipsUDP) allPorts.Add(ep.Port);
        foreach (var conn in tcpConnInfoArray) allPorts.Add(conn.LocalEndPoint.Port);

        return allPorts;
    }

    /// <summary>
    /// ��ȡһ��û��ʹ�õĶ˿�
    /// </summary>
    /// <returns>�˿�</returns>
    private static int GetFirstAvailablePort()
    {
        var portUsed = PortIsUsed();
        if (portUsed.Count > 5000)
        {
            return -1;
        }
        var random = new Random();
        do
        {
            int temp = random.Next() % 65535;
            if (!portUsed.Contains(temp))
            {
                return temp;
            }
        }
        while (true);
    }

    /// <summary>
    /// ������Ϸ�˿ڷ�����
    /// </summary>
    /// <returns></returns>
    private static async Task<int> RunServerAsync()
    {
        if (_channel != null)
        {
            return (_channel.LocalAddress as IPEndPoint)!.Port;
        }
        _bossGroup = new MultithreadEventLoopGroup(1);
        _workerGroup = new MultithreadEventLoopGroup();
        try
        {
            _bootstrap = new();
            _bootstrap.Group(_bossGroup, _workerGroup);
            _bootstrap.Channel<TcpServerSocketChannel>();
            _bootstrap
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    channel.Pipeline.AddLast("colormc", new GameServerHandler());
                }));

            int port = 0;

            if (SystemInfo.Os == OsType.Android)
            {
                port = ColorMCGui.PhoneGetFreePort();
            }
            else
            {
                port = GetFirstAvailablePort();
            }
            _channel = await _bootstrap.BindAsync(IPAddress.Any, port);

            return port;
        }
        catch (Exception e)
        {
            Logs.Crash("netty error", e);
            return 0;
        }
    }

    /// <summary>
    /// ֹͣ��Ϸ�˿ڷ�����
    /// </summary>
    private static async void Stop()
    {
        await _channel.CloseAsync();
        await Task.WhenAll(
                _bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
    }

    /// <summary>
    /// �������ݵ����пͻ���
    /// </summary>
    /// <param name="byteBuffer"></param>
    private static void SendMessage(IByteBuffer byteBuffer)
    {
        foreach (var item in _channels.ToArray())
        {
            try
            {
                if (item.IsWritable)
                {
                    item.WriteAndFlushAsync(byteBuffer.RetainedDuplicate());
                }
            }
            catch
            {

            }
        }
    }

    public static async Task SendMessage(int port)
    {
        var group = new MultithreadEventLoopGroup();
        try
        {
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;

                }));

            IChannel clientChannel = await bootstrap.ConnectAsync(IPAddress.Parse("127.0.0.1"), port);

            if (BaseBinding.StartLaunch != null)
            {
                var buf = Unpooled.Buffer();
                buf.WriteInt(TypeLaunchStart)
                    .WriteStringList(BaseBinding.StartLaunch);
                await clientChannel.WriteAndFlushAsync(buf);
            }
            else
            {
                var buf = Unpooled.Buffer();
                buf.WriteInt(TypeLaunchShow);
                await clientChannel.WriteAndFlushAsync(buf);
            }

            await clientChannel.CloseAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        }
    }

    public static async Task Init()
    {
        Port = await RunServerAsync();
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
        Stop();
        s_isRun = false;
    }

    private static string ReadString(this IByteBuffer buf)
    {
        int size = buf.ReadInt();
        var datas = new byte[size];
        buf.ReadBytes(datas);
        return Encoding.UTF8.GetString(datas);
    }

    public static void Clear()
    {
        Servers.Clear();
    }

    public static void AddServerInfo(NetFrpCloudServerModel model)
    {
        Servers.Add(model);
    }

    private class GameServerHandler : ChannelHandlerAdapter
    {
        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            _channels.Add(ctx.Channel);
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            _channels.Remove(ctx.Channel);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer buffer)
            {
                try
                {
                    int type = buffer.ReadInt();

                    //��Ϸ�����
                    if (type == TypeGameMouseState)
                    {
                        string uuid = buffer.ReadString();
                        var value = buffer.ReadBoolean();
                        GameJoystick.SetMouse(uuid, value);
                    }
                    //ǰ̨������
                    else if (type == TypeLaunchShow)
                    {
                        App.Show();
                    }
                    //������Ϸʵ��
                    else if (type == TypeLaunchStart)
                    {
                        BaseBinding.Launch(buffer.ReadStringList());
                    }
                }
                catch
                {

                }
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.CloseAsync();
        }
    }

    private static string[] ReadStringList(this IByteBuffer buf)
    {
        int size = buf.ReadInt();
        string[] temp = new string[size];
        for (int a = 0; a < size; a++)
        {
            temp[a] = buf.ReadString();
        }

        return temp;
    }

    private static IByteBuffer WriteString(this IByteBuffer buf, string data)
    {
        var temp = Encoding.UTF8.GetBytes(data);
        buf.WriteInt(temp.Length);
        buf.WriteBytes(temp);

        return buf;
    }

    private static IByteBuffer WriteStringList(this IByteBuffer buf, string[] data)
    {
        buf.WriteInt(data.Length);
        foreach (var item in data)
        {
            buf.WriteString(item);
        }

        return buf;
    }

    private static void SendServerInfo(NetFrpCloudServerModel model)
    {
        var buf = Unpooled.Buffer();

        var temp = model.IP.Split(':');

        buf.WriteInt(TypeGameMotd);
        buf.WriteString(temp[0]);
        buf.WriteString(temp[1]);
        buf.WriteString(LanGameHelper.MakeMotd("[ColorMC]" + model.Name, temp[1]));
        SendMessage(buf);
    }
}
