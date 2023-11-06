using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ColorMC.Core.Game;

public class LanServer
{
    private string _motd;
    private string _ip;

    private byte[] _data;

    private bool _isRun;

    private Socket _socket;
    private IPEndPoint _port;
    public LanServer(string ip, string? motd)
    {
        _motd = motd ?? "ColorMC NetFrp";
        _ip = ip;

        _data = Encoding.UTF8.GetBytes(MakeMotd());

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _port = new IPEndPoint(IPAddress.Parse("224.0.2.60"), 4445);
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

    private string MakeMotd()
    {
        return $"[MOTD]{_motd}[/MOTD][AD]{_ip}[/AD]";
    }

    private void Run()
    {
        while (_isRun)
        {
            _socket.SendTo(_data, _port);
            Thread.Sleep(1500);
        }
    }
}
