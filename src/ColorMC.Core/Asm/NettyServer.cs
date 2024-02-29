using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System.Net;
using System.Net.NetworkInformation;

namespace ColorMC.Core.Asm;

public static class NettyServer
{
    private static IEventLoopGroup s_bossGroup;
    private static IEventLoopGroup s_workerGroup;
    private static ServerBootstrap s_bootstrap;
    private static IChannel s_channel;

    private static readonly List<IChannel> s_channels = [];

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

    private static int GetFirstAvailablePort()
    {
        var portUsed = PortIsUsed();
        if (portUsed.Count > 50000)
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
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast("echo", new EchoServerHandler());
                }));

            int port = GetFirstAvailablePort();
            s_channel = await s_bootstrap.BindAsync(port);
            ColorMCCore.Stop += Stop;

            return port;
        }
        finally
        {

        }
    }

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

    private class EchoServerHandler : ChannelHandlerAdapter
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
                ColorMCCore.NettyPack?.Invoke(context.Channel, buffer);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.CloseAsync();
        }
    }
}

