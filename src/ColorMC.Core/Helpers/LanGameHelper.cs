namespace ColorMC.Core.Helpers;

/// <summary>
/// 局域网游戏处理
/// </summary>
public static class LanGameHelper
{
    public const string IPv4 = "224.0.2.60";
    public const string IPv6 = "FF75:230::60";
    public const int Port = 4445;

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

    /// <summary>
    /// 创建联机用广播包
    /// </summary>
    /// <param name="motd"></param>
    /// <param name="ip"></param>
    /// <returns></returns>
    public static string MakeMotd(string motd, string ip)
    {
        return $"[MOTD]{motd}[/MOTD][AD]{ip}[/AD]";
    }
}
