using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Objs.ColorMC;
using ColorMC.Gui.UIBinding;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace ColorMC.Gui.Utils;

/// <summary>
/// ColorMC进程通信
/// </summary>
public static class LaunchSocketUtils
{
    public static readonly List<ColorMCCloudServerObj> Servers = [];

    public static int Port { get; private set; }

    private const int TypeGameMouseState = 1;
    private const int TypeGameMotd = 2;
    private const int TypeLaunchShow = 3;
    private const int TypeLaunchStart = 4;
    //private const int TypeMouseXY = 5;
    //private const int TypeMouseClick = 6;
    //private const int TypeKeybordClick = 7;
    //private const int TypeMouseScoll = 8;
    private const int TypeGameChannel = 9;
    private const int TypeSetTitle = 10;
    private const int TypeGameWindowSize = 11;

    private static bool s_isRun;

    private static IEventLoopGroup _bossGroup;
    private static IEventLoopGroup _workerGroup;
    private static ServerBootstrap _bootstrap;

    private static IChannel _channel;

    /// <summary>
    /// 客户端信道
    /// </summary>
    private static readonly List<IChannel> _channels = [];

    private static readonly Dictionary<string, IChannel> _gameChannels = [];

    /// <summary>
    /// 获取所有正在使用的端口
    /// </summary>
    /// <returns>端口列表</returns>
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
    /// 获取一个没有使用的端口
    /// </summary>
    /// <returns>端口</returns>
    private static int GetFirstAvailablePort()
    {
        try
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
        catch
        {
            var random = new Random();
            do
            {
                try
                {
                    int port = random.Next(65535);
                    using var socket = new TcpListener(IPAddress.Any, port);
                    socket.Start();
                    socket.Stop();
                    return port;
                }
                catch
                {

                }
            } while (true);
        }
    }

    /// <summary>
    /// 启动游戏端口服务器
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

            int port = GetFirstAvailablePort();
            _channel = await _bootstrap.BindAsync(IPAddress.Any, port);

            ColorMCCore.GameExit += ColorMCCore_GameExit;

            return port;
        }
        catch (Exception e)
        {
            PathBinding.OpenFileWithExplorer(Logs.Crash("netty error", e));
            return 0;
        }
    }

    private static void ColorMCCore_GameExit(GameSettingObj arg1, LoginObj arg2, int arg3)
    {
        _gameChannels.Remove(arg1.UUID);
    }

    /// <summary>
    /// 停止游戏端口服务器
    /// </summary>
    private static async void Stop()
    {
        ColorMCCore.GameExit -= ColorMCCore_GameExit;
        await _channel.CloseAsync();
        await Task.WhenAll(
                _bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
    }

    /// <summary>
    /// 发送数据到所有客户端
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

    /// <summary>
    /// 发送消息到指定客户端
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="byteBuffer"></param>
    private static void SendMessage(GameSettingObj obj, IByteBuffer byteBuffer)
    {
        if (_gameChannels.TryGetValue(obj.UUID, out var channel))
        {
            try
            {
                if (channel.IsWritable)
                {
                    channel.WriteAndFlushAsync(byteBuffer.RetainedDuplicate());
                }
            }
            catch
            {

            }
        }
    }

    /// <summary>
    /// 给现有的ColorMC发送信息
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    public static async Task SendMessage(int port, string[] data)
    {
        if (port <= 0)
        {
            return;
        }
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

            if (data != null && data.Length != 0)
            {
                var buf = Unpooled.Buffer();
                buf.WriteInt(TypeLaunchStart)
                    .WriteStringList(data);
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

    /// <summary>
    /// 初始化服务器
    /// </summary>
    /// <returns></returns>
    public static async Task Init()
    {
        Port = await RunServerAsync();
        App.OnClose += App_OnClose;
        s_isRun = true;
        new Thread(Run).Start();
    }

    /// <summary>
    /// 联机用显示房间
    /// </summary>
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

    public static void AddServerInfo(ColorMCCloudServerObj obj)
    {
        Servers.Add(obj);
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

                    //游戏内鼠标
                    if (type == TypeGameMouseState)
                    {
                        string uuid = buffer.ReadString();
                        var value = buffer.ReadBoolean();
                        GameJoystick.SetMouse(uuid, value);
                    }
                    //前台启动器
                    else if (type == TypeLaunchShow)
                    {
                        App.Show();
                    }
                    //启动游戏实例
                    else if (type == TypeLaunchStart)
                    {
                        var data = buffer.ReadStringList();
                        Dispatcher.UIThread.Post(() =>
                        {
                            ColorMCGui.StartArg(data);
                        });
                    }
                    //游戏实例绑定
                    else if (type == TypeGameChannel)
                    {
                        string uuid = buffer.ReadString();
                        _gameChannels[uuid] = context.Channel;

                        GameBinding.GameConnect(uuid);
                    }
                    else if (type == TypeGameWindowSize)
                    {
                        string uuid = buffer.ReadString();
                        int width = buffer.ReadInt();
                        int height = buffer.ReadInt();
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

    private static void SendServerInfo(ColorMCCloudServerObj model)
    {
        var buf = Unpooled.Buffer();

        var temp = model.IP.Split(':');

        buf.WriteInt(TypeGameMotd);
        buf.WriteString(temp[0]);
        buf.WriteString(temp[1]);
        buf.WriteString(LanGameHelper.MakeMotd("[ColorMC]" + model.Name, temp[1]));
        SendMessage(buf);
    }

    //public static void SendMousePos(GameSettingObj obj, double x, double y)
    //{
    //    var buf = Unpooled.Buffer();
    //    buf.WriteInt(TypeMouseXY)
    //        .WriteDouble(x)
    //        .WriteDouble(y);
    //    SendMessage(obj, buf);
    //}

    //private static void SendMouseClick(GameSettingObj obj, MouseButton button, bool down)
    //{
    //    var buf = Unpooled.Buffer();
    //    buf.WriteInt(TypeMouseClick)
    //        .WriteInt((int)button)
    //        .WriteBoolean(down);
    //    SendMessage(obj, buf);
    //}

    //private static void SendKeybordClick(GameSettingObj obj, KeyModifiers modifiers, Key key, bool down)
    //{
    //    var buf = Unpooled.Buffer();
    //    buf.WriteInt(TypeKeybordClick)
    //        .WriteInt((int)modifiers)
    //        .WriteInt((int)key)
    //        .WriteBoolean(down);
    //    SendMessage(obj, buf);
    //}

    //public static void SendMouseScoll(GameSettingObj obj, bool up)
    //{
    //    var buf = Unpooled.Buffer();
    //    buf.WriteInt(TypeMouseScoll)
    //        .WriteBoolean(up);
    //    SendMessage(obj, buf);
    //}


    //public static void SendKey(GameSettingObj obj, InputKeyObj key, bool down)
    //{
    //    if (key.MouseButton != MouseButton.None)
    //    {
    //        SendMouseClick(obj, key.MouseButton, down);
    //    }
    //    else
    //    {
    //        SendKeybordClick(obj, key.KeyModifiers, key.Key, down);
    //    }
    //}

    public static void SetTitle(GameSettingObj obj, string title)
    {
        var buf = Unpooled.Buffer();
        buf.WriteInt(TypeSetTitle)
            .WriteString(title);
        SendMessage(obj, buf);
    }
}
