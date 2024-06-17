using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Helpers;

public static class LanGameHelper
{
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

    public static string MakeMotd(string motd, string ip)
    {
        return $"[MOTD]{motd}[/MOTD][AD]{ip}[/AD]";
    }
}
