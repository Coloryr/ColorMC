using System.Net;
using System.Net.Sockets;
using System.Text;
using ColorMC.Core.Helpers;

namespace ColorMC.Core.Game;

/// <summary>
/// 局域网游戏
/// </summary>
public class LanClient
{
    public required Action<string, string, string> FindLan;

    private readonly UdpClient _socketV4;
    private readonly UdpClient _socketV6;

    private bool _isRun;
    private readonly CancellationTokenSource _cancel = new();

    public LanClient()
    {
        //V4组播地址
        _socketV4 = new();
        _socketV4.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _socketV4.Client.Bind(new IPEndPoint(IPAddress.Any, LanGameHelper.Port));
        _socketV4.JoinMulticastGroup(IPAddress.Parse(LanGameHelper.IPv4));
        //V6组播地址
        _socketV6 = new(AddressFamily.InterNetworkV6);
        _socketV6.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _socketV6.Client.Bind(new IPEndPoint(IPAddress.IPv6Any, LanGameHelper.Port));
        _socketV6.JoinMulticastGroup(IPAddress.Parse(LanGameHelper.IPv6));

        _isRun = true;

        //开始监听
        new Thread(async () =>
        {
            while (_isRun)
            {
                try
                {
                    var data = await _socketV4.ReceiveAsync(_cancel.Token);
                    if (!_isRun)
                    {
                        return;
                    }

                    string temp = Encoding.UTF8.GetString(data.Buffer);
                    var point = data.RemoteEndPoint;

                    string motd = LanGameHelper.GetMotd(temp);
                    string? ad = LanGameHelper.GetAd(temp);

                    if (ad != null)
                    {
                        FindLan?.Invoke(motd, point.ToString(), ad);
                    }
                }
                catch
                {

                }
            }
        })
        {
            Name = "ColorMC Lan Client V4"
        }.Start();

        new Thread(async () =>
        {
            while (_isRun)
            {
                try
                {
                    var data = await _socketV6.ReceiveAsync(_cancel.Token);
                    if (!_isRun)
                    {
                        return;
                    }

                    string temp = Encoding.UTF8.GetString(data.Buffer);
                    var point = data.RemoteEndPoint;

                    string motd = LanGameHelper.GetMotd(temp);
                    string? ad = LanGameHelper.GetAd(temp);

                    if (ad != null)
                    {
                        FindLan?.Invoke(motd, point.ToString(), ad);
                    }
                }
                catch
                {

                }
            }
        })
        {
            Name = "ColorMC Lan Client V6"
        }.Start();
    }

    /// <summary>
    /// 停止获取局域网数据
    /// </summary>
    public void Stop()
    {
        _isRun = false;

        _cancel.Cancel();

        _socketV4.Close();
        _socketV4.Dispose();
    }
}
