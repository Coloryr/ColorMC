using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;

namespace ColorMC.Core.Asm;

public static class NettyServer
{
    private static IEventLoopGroup bossGroup;
    private static IEventLoopGroup workerGroup;
    private static ServerBootstrap bootstrap;
    private static IChannel boundChannel;

    private static List<IChannel> channels = [];

    public static Action<IByteBuffer>? GetPack;

    /// <summary>
    /// 获取操作系统已用的端口号
    /// </summary>
    /// <returns></returns>
    public static List<int> PortIsUsed()
    {
        //获取本地计算机的网络连接和通信统计数据的信息
        IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

        //返回本地计算机上的所有Tcp监听程序
        IPEndPoint[] ipsTCP = ipGlobalProperties.GetActiveTcpListeners();

        //返回本地计算机上的所有UDP监听程序
        IPEndPoint[] ipsUDP = ipGlobalProperties.GetActiveUdpListeners();

        //返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。
        TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

        var allPorts = new List<int>();
        foreach (IPEndPoint ep in ipsTCP) allPorts.Add(ep.Port);
        foreach (IPEndPoint ep in ipsUDP) allPorts.Add(ep.Port);
        foreach (TcpConnectionInformation conn in tcpConnInfoArray) allPorts.Add(conn.LocalEndPoint.Port);

        return allPorts;
    }

    public static int GetFirstAvailablePort()
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
        if (boundChannel != null)
        {
            return (boundChannel.LocalAddress as IPEndPoint)!.Port;
        }
        bossGroup = new MultithreadEventLoopGroup(1);
        workerGroup = new MultithreadEventLoopGroup();
        try
        {
            bootstrap = new();
            bootstrap.Group(bossGroup, workerGroup);
            bootstrap.Channel<TcpServerSocketChannel>();
            bootstrap
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast("echo", new EchoServerHandler());
                }));

            int port = GetFirstAvailablePort();
            boundChannel = await bootstrap.BindAsync(port);
            ColorMCCore.Stop += Stop;

            return port;
        }
        finally
        {

        }
    }

    public static async void Stop()
    {
        await boundChannel.CloseAsync();
        await Task.WhenAll(
                bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
    }

    public static void SendMessage(IByteBuffer byteBuffer)
    {
        foreach (var item in channels.ToArray())
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

    public class EchoServerHandler : ChannelHandlerAdapter
    {
        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            channels.Add(ctx.Channel);
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            channels.Remove(ctx.Channel);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer buffer)
            {
                GetPack?.Invoke(buffer);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.CloseAsync();
        }
    }
}

