using System.Text;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 文本处理
/// </summary>
public static class StringHelper
{
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

    /// <summary>
    /// 命令行参数解析
    /// </summary>
    /// <param name="input">命令行</param>
    /// <returns>数组</returns>
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

    /// <summary>
    /// 转为HEX格式文本
    /// </summary>
    /// <param name="temp"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 转为HEX格式文本
    /// </summary>
    /// <param name="temp"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 转为HEX格式文本
    /// </summary>
    /// <param name="temp"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 转为HEX格式文本
    /// </summary>
    /// <param name="temp"></param>
    /// <returns></returns>
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
}
