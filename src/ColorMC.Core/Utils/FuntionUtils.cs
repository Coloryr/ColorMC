using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ColorMC.Core.Utils;

public static partial class FuntionUtils
{
    [GeneratedRegex("[^0-9]+")]
    private static partial Regex Regex1();

    /// <summary>
    /// 检查是否为数字
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool CheckNotNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;
        return Regex1().IsMatch(input);
    }

    /// <summary>
    /// Tick转时间
    /// </summary>
    /// <param name="unixTimeStamp"></param>
    /// <returns></returns>
    public static DateTime MillisecondsToDataTime(long unixTimeStamp)
    {
        var start = new DateTime(1970, 1, 1) +
            TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        return start.AddMilliseconds(unixTimeStamp);
    }

    /// <summary>
    /// 时间戳
    /// </summary>
    public static int GetTime()
    {
        var tick = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).Ticks;
        return (int)(tick / 10000000);
    }

    /// <summary>
    /// 获取MD5值
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GenMd5(byte[] data)
    {
        var text = new StringBuilder();
        foreach (byte iByte in MD5.HashData(data))
        {
            text.AppendFormat("{0:x2}", iByte);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 获取SHA1值
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GenSha1(byte[] data)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA1.HashData(data))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 获取SHA1值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GenSha1(string input)
    {
        return GenSha1(Encoding.UTF8.GetBytes(input));
    }
    /// <summary>
    /// 获取SHA256值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GenSha256(string input)
    {
        return GenSha256(Encoding.UTF8.GetBytes(input));
    }
    /// <summary>
    /// 获取SHA1值
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static string GenSha1(Stream stream)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA1.HashData(stream))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 获取SHA1值
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<string> GenSha1Async(Stream stream)
    {
        var text = new StringBuilder();
        foreach (byte item in await SHA1.HashDataAsync(stream))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 获取SHA256值
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GenSha256(byte[] data)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA256.HashData(data))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 获取SHA256值
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static string GenSha256(Stream stream)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA256.HashData(stream))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }

    /// <summary>
    /// 获取SHA512值
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<string> GenSha512Async(Stream stream)
    {
        var text = new StringBuilder();
        foreach (byte item in await SHA512.HashDataAsync(stream))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }

    /// <summary>
    /// 新的UUID
    /// </summary>
    /// <returns></returns>
    public static string NewUUID()
    {
        return Guid.NewGuid().ToString().ToLower();
    }
    /// <summary>
    /// 生成Base64
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GenBase64(string input)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
    }
    /// <summary>
    /// 反解Base64
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string DeBase64(string input)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(input));
    }

    /// <summary>
    /// 执行内存回收
    /// </summary>
    public static void RunGC()
    {
        Task.Run(() =>
        {
            Task.Delay(1000).Wait();
            GC.Collect();
            GC.Collect();
        });
    }

    /// <summary>
    /// 截取字符串
    /// </summary>
    /// <param name="input"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static string GetString(string input, string start, string end)
    {
        var temp = input.IndexOf(start);
        if (temp == -1)
        {
            return input;
        }
        var temp1 = input.IndexOf(end, temp + start.Length + 1);
        if (temp1 == -1)
        {
            return input;
        }

        return input[(temp + start.Length)..temp1];
    }

    public static IEnumerable<string> ArgParse(string input)
    {
        char quoteChar = '"';
        char escapeChar = '\\';
        bool insideQuote = false;
        bool insideEscape = false;

        StringBuilder currentArg = new();

        int currentArgCharCount = 0;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c == quoteChar)
            {
                currentArgCharCount++;

                if (insideEscape)
                {
                    currentArg.Append(c);  
                    insideEscape = false;
                }
                else if (insideQuote)
                {
                    insideQuote = false;
                }
                else
                {
                    insideQuote = true;
                }
            }
            else if (c == escapeChar)
            {
                currentArgCharCount++;

                if (insideEscape)
                    currentArg.Append(escapeChar + escapeChar);

                insideEscape = !insideEscape;
            }
            else if (char.IsWhiteSpace(c))
            {
                if (insideQuote)
                {
                    currentArgCharCount++;
                    currentArg.Append(c);
                }
                else
                {
                    if (currentArgCharCount > 0)
                        yield return currentArg.ToString();

                    currentArgCharCount = 0;
                    currentArg.Clear();
                }
            }
            else
            {
                currentArgCharCount++;
                if (insideEscape)
                {
                    currentArg.Append(escapeChar);
                    currentArgCharCount = 0;
                    insideEscape = false;
                }
                currentArg.Append(c);
            }
        }

        if (currentArgCharCount > 0)
            yield return currentArg.ToString();
    }

    public static string ToHex(byte temp)
    {
        var builder = new StringBuilder();
        for (int a = 0; a < 8; a++)
        {
            if (a > 0 && a % 4 == 0)
            {
                builder.Append(' ');
            }
            builder.Append((temp & 0x80) != 0 ? '1' : '0');
            temp <<= 1;
        }

        return builder.ToString();
    }

    public static string ToHex(short temp)
    {
        var builder = new StringBuilder();
        for (int a = 0; a < 16; a++)
        {
            if (a > 0 && a % 4 == 0)
            {
                builder.Append(' ');
            }
            builder.Append((temp & 0x8000) != 0 ? '1' : '0');
            temp <<= 1;
        }

        return builder.ToString();
    }

    public static string ToHex(int temp)
    {
        var builder = new StringBuilder();
        for (int a = 0; a < 32; a++)
        {
            if (a > 0 && a % 4 == 0)
            {
                builder.Append(' ');
            }
            builder.Append((temp & 0x80000000) != 0 ? '1' : '0');
            temp <<= 1;
        }

        return builder.ToString();
    }

    public static string ToHex(long temp)
    {
        var builder = new StringBuilder();
        for (int a = 0; a < 64; a++)
        {
            if (a > 0 && a % 4 == 0)
            {
                builder.Append(' ');
            }
            builder.Append((temp & -9223372036854775808) != 0 ? '1' : '0');
            temp <<= 1;
        }

        return builder.ToString();
    }

    public static string GetString(this List<string> list)
    {
        var str = new StringBuilder();
        foreach (var item in list)
        {
            str.Append(item).Append(',');
        }

        return str.ToString()[..^1];
    }
}
