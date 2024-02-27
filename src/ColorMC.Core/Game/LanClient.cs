using System.Net;
using System.Net.Sockets;
using System.Text;

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
        _socketV4 = new(new IPEndPoint(IPAddress.Any, 4445));
        _socketV4.JoinMulticastGroup(IPAddress.Parse("224.0.2.60"));
        _socketV6 = new(new IPEndPoint(IPAddress.IPv6Any, 4445));
        _socketV6.JoinMulticastGroup(IPAddress.Parse("FF75:230::60"));

        _isRun = true;

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

                    string motd = GetMotd(temp);
                    string? ad = GetAd(temp);

                    if (ad != null)
                    {
                        FindLan?.Invoke(motd, point.ToString(), ad);
                    }
                }
                catch
                {

                }
            }
        }).Start();

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

                    string motd = GetMotd(temp);
                    string? ad = GetAd(temp);

                    if (ad != null)
                    {
                        FindLan?.Invoke(motd, point.ToString(), ad);
                    }
                }
                catch
                {

                }
            }
        }).Start();
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

    /// <summary>
    /// 获取Motd信息
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <returns>Motd信息</returns>
    public static string GetMotd(string input)
    {
        int i = input.IndexOf("[MOTD]");

        if (i < 0)
        {
            return "missing no";
        }
        else
        {
            int j = input.IndexOf("[/MOTD]", i + "[MOTD]".Length);
            return j < i ? "missing no" : input[(i + "[MOTD]".Length)..j];
        }
    }

    /// <summary>
    /// 获取地址信息
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <returns>地址信息</returns>
    public static string? GetAd(string input)
    {
        int i = input.IndexOf("[/MOTD]");

        if (i < 0)
        {
            return null;
        }
        else
        {
            int j = input.IndexOf("[/MOTD]", i + "[/MOTD]".Length);

            if (j >= 0)
            {
                return null;
            }
            else
            {
                int k = input.IndexOf("[AD]", i + "[/MOTD]".Length);

                if (k < 0)
                {
                    return null;
                }
                else
                {
                    int l = input.IndexOf("[/AD]", k + "[AD]".Length);
                    return l < k ? null : input[(k + "[AD]".Length)..l];
                }
            }
        }
    }
}
