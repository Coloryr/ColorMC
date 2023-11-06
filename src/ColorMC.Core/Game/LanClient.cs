using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ColorMC.Core.Game;

public class LanClient
{
    public Action<string, string, string>? FindLan;

    private readonly UdpClient _socket;

    private bool _isRun;
    private CancellationTokenSource _cancel = new();

    public LanClient()
    {
        var ipep = new IPEndPoint(IPAddress.Any, 4445);
        _socket = new(ipep);
        _socket.JoinMulticastGroup(IPAddress.Parse("224.0.2.60"));

        _isRun = true;

        new Thread(Run).Start();
    }

    private async void Run()
    {
        while (_isRun)
        {
            try
            {
                var data = await _socket.ReceiveAsync(_cancel.Token);
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
    }

    public void Stop()
    {
        _isRun = false;

        _cancel.Cancel();

        _socket.Close();
        _socket.Dispose();
    }

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
