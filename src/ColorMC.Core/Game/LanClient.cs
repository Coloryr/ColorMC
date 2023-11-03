using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ColorMC.Core.Game;

public class LanClient
{
    public Action<string, string>? FindLan;

    private UdpClient _socket;

    private byte[] _temp = new byte[1024];

    public LanClient()
    {
        var ipep = new IPEndPoint(IPAddress.Any, 4445);
        _socket = new(ipep);
        _socket.JoinMulticastGroup(IPAddress.Parse("224.0.2.60"));

        _socket.BeginReceive(new AsyncCallback(Callback), null);
    }

    private void Callback(IAsyncResult result)
    {
        var point = new IPEndPoint(IPAddress.Any, 0);
        var data = _socket.EndReceive(result, ref point);

        string temp = Encoding.UTF8.GetString(data);

        string motd = GetMotd(temp);
        string? ad = GetAd(temp);

        if (ad != null)
        {
            FindLan?.Invoke(motd, ad);
        }

        _socket.BeginReceive(new AsyncCallback(Callback), null);
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
