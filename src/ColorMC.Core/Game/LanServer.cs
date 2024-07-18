using System.Net;
using System.Net.Sockets;
using System.Text;
using ColorMC.Core.Helpers;

namespace ColorMC.Core.Game;

public class LanServer
{
    private readonly string _motd;
    private readonly string _port;

    private readonly byte[] _data;

    private bool _isRun;

    private readonly Socket _socket;
    private readonly IPEndPoint _send;
    public LanServer(string port, string? motd)
    {
        _motd = motd ?? "ColorMC";
        _port = port;

        _data = Encoding.UTF8.GetBytes(LanGameHelper.MakeMotd(_motd, _port));

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _send = new IPEndPoint(IPAddress.Parse("224.0.2.60"), 4445);
        _isRun = true;
        new Thread(Run)
        {
            Name = "ColorMC NetFrp"
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
            _socket.SendTo(_data, _send);
            Thread.Sleep(1500);
        }
    }
}
