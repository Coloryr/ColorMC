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
public class NettyServer
{
    private IEventLoopGroup _bossGroup;
    private IEventLoopGroup _workerGroup;
    private ServerBootstrap _bootstrap;
    private IChannel _channel;

    /// <summary>
    /// 客户端信道
    /// </summary>
    private readonly List<IChannel> _channels = [];

    public event Action<IChannel, IByteBuffer>? NettyPack;

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
    public async Task<int> RunServerAsync()
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
                    channel.Pipeline.AddLast("colormc", new GameServerHandler(this));
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
            _channel = await _bootstrap.BindAsync(port);

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
    public async void Stop()
    {
        await _channel.CloseAsync();
        await Task.WhenAll(
                _bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
    }

    /// <summary>
    /// 发送数据到所有客户端
    /// </summary>
    /// <param name="byteBuffer"></param>
    public void SendMessage(IByteBuffer byteBuffer)
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

    internal void OnNettyPack(IChannel channel, IByteBuffer buffer)
    {
        NettyPack?.Invoke(channel, buffer);
    }

    private class GameServerHandler(NettyServer server) : ChannelHandlerAdapter
    {
        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            server._channels.Add(ctx.Channel);
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            server._channels.Remove(ctx.Channel);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer buffer)
            {
                server.OnNettyPack(context.Channel, buffer);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.CloseAsync();
        }
    }
}

