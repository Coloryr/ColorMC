using System.Net;
using System.Net.NetworkInformation;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace ColorMC.Core.Asm;

/// <summary>
/// 启动器与游戏通信
/// </summary>
public static class NettyServer
{
    private static IEventLoopGroup s_bossGroup;
    private static IEventLoopGroup s_workerGroup;
    private static ServerBootstrap s_bootstrap;
    private static IChannel s_channel;

    /// <summary>
    /// 客户端信道
    /// </summary>
    private static readonly List<IChannel> s_channels = [];

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
    /// 启动游戏端口服务器
    /// </summary>
    /// <returns></returns>
    public static async Task<int> RunServerAsync()
    {
        if (s_channel != null)
        {
            return (s_channel.LocalAddress as IPEndPoint)!.Port;
        }
        s_bossGroup = new MultithreadEventLoopGroup(1);
        s_workerGroup = new MultithreadEventLoopGroup();
        try
        {
            s_bootstrap = new();
            s_bootstrap.Group(s_bossGroup, s_workerGroup);
            s_bootstrap.Channel<TcpServerSocketChannel>();
            s_bootstrap
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    channel.Pipeline.AddLast("colormc", new GameServerHandler());
                }));

            int port = 0;

            if (SystemInfo.Os == OsType.Android)
            {
                port = ColorMCCore.GetFreePort();
            }
            else
            {
                port = GetFirstAvailablePort();
            }
            s_channel = await s_bootstrap.BindAsync(port);
            ColorMCCore.Stop += Stop;

            return port;
        }
        catch(Exception e)
        {
            Logs.Crash("netty error", e);
            return 0;
        }
    }

    /// <summary>
    /// 停止游戏端口服务器
    /// </summary>
    private static async void Stop()
    {
        await s_channel.CloseAsync();
        await Task.WhenAll(
                s_bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                s_workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
    }

    /// <summary>
    /// 发送数据到所有客户端
    /// </summary>
    /// <param name="byteBuffer"></param>
    public static void SendMessage(IByteBuffer byteBuffer)
    {
        foreach (var item in s_channels.ToArray())
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

    private class GameServerHandler : ChannelHandlerAdapter
    {
        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            s_channels.Add(ctx.Channel);
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            s_channels.Remove(ctx.Channel);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer buffer)
            {
                ColorMCCore.OnNettyPack(context.Channel, buffer);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.CloseAsync();
        }
    }
}

