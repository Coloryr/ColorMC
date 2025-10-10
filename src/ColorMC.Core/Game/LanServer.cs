using System.Net;
using System.Net.Sockets;
using System.Text;
using ColorMC.Core.Helpers;

namespace ColorMC.Core.Game;

/// <summary>
/// 广播服务器
/// </summary>
public class LanServer
{
    private readonly byte[] _data;

    private bool _isRun;

    private readonly Socket _socketV4;
    private readonly IPEndPoint _sendV4;

    private readonly Socket _socketV6;
    private readonly IPEndPoint _sendV6;

    public LanServer(string port, string motd)
    {
        _data = Encoding.UTF8.GetBytes(LanGameHelper.MakeMotd(motd, port));

        _socketV4 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socketV4.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _sendV4 = new IPEndPoint(IPAddress.Parse(LanGameHelper.IPv4), LanGameHelper.Port);
        _socketV6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
        _socketV6.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _sendV6 = new IPEndPoint(IPAddress.Parse(LanGameHelper.IPv6), LanGameHelper.Port);
        _isRun = true;
        new Thread(Run)
        {
            Name = "ColorMC Lan Server",
            IsBackground = true
        }.Start();
    }

    public void Stop()
    {
        _isRun = false;
    }

    private void Run()
    {
        while (_isRun)
        {
            _socketV4.SendTo(_data, _sendV4);
            _socketV6.SendTo(_data, _sendV6);
            Thread.Sleep(3000);
        }
    }
}
