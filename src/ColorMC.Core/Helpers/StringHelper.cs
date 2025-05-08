using System.Text;
using System.Text.RegularExpressions;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 文本处理
/// </summary>
public static partial class StringHelper
{
    [GeneratedRegex("\\b\\d+(.\\d+)+\\b")]
    private static partial Regex VersionRegex();

    /// <summary>
    /// 截取字符串
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <param name="start">开始于</param>
    /// <param name="end">结束于</param>
    /// <returns>输出</returns>
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
    /// <param name="temp">数据</param>
    /// <returns>HEX文本</returns>
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
    /// <param name="temp">数据</param>
    /// <returns>HEX文本</returns>
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
    /// <param name="temp">数据</param>
    /// <returns>HEX文本</returns>
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
    /// <param name="temp">数据</param>
    /// <returns>HEX文本</returns>
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

    /// <summary>
    /// 版本号排序
    /// </summary>
    /// <param name="list">序号</param>
    public static void VersionSort(List<string> list)
    {
        var regex = VersionRegex();
        var list1 = new List<VersionStrObj>();
        foreach (var item in list)
        {
            var version = regex.Match(item.Replace("+build", ""));
            var version1 = new Version(version.Groups[0].Value);
            list1.Add(new()
            {
                Version = version1,
                VersionStr = item
            });
        }

        list1.Sort(VersionStrObjComparer.Instance);

        list.Clear();
        foreach (var item in list1)
        {
            list.Add(item.VersionStr);
        }
    }

    /// <summary>
    /// 从Steam获取字符串
    /// </summary>
    /// <param name="stream1">流</param>
    /// <returns>字符串</returns>
    public static async Task<string> GetStringAsync(Stream stream1)
    {
        //var head = Encoding.UTF8.GetPreamble();
        using var stream = new MemoryStream();
        await stream1.CopyToAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        //if (head.Length == 0)
        //{
        return Encoding.UTF8.GetString(stream.ToArray());
        //}
        //else
        //{
        //    var temp = new byte[head.Length];
        //    await stream.ReadExactlyAsync(temp);
        //    for (int a = 0; a < temp.Length; a++)
        //    {
        //        if (temp[a] != head[a])
        //        {
        //            stream.Seek(0, SeekOrigin.Begin);
        //            return Encoding.UTF8.GetString(stream.ToArray());
        //        }
        //    }

        //    var temp1 = stream.ToArray();
        //    return Encoding.UTF8.GetString(temp1, head.Length, temp1.Length - head.Length);
        //}
    }

    /// <summary>
    /// 删除符号
    /// </summary>
    /// <param name="input">输入数据</param>
    /// <param name="symbol">符号</param>
    /// <returns>替换数据</returns>
    public static string RemovePartAfterSymbol(string input, char symbol)
    {
        int index = input.IndexOf(symbol);
        if (index >= 0)
        {
            return input[..index];
        }
        return input; // 如果没有找到符号，就返回原始字符串
    }

    /// <summary>
    /// 替换字符串第一个数据
    /// </summary>
    /// <param name="input">输入数据</param>
    /// <param name="oldValue">查找内容</param>
    /// <param name="newValue">替换内容</param>
    /// <returns>输出结果</returns>
    public static string ReplaceFirst(string input, string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(oldValue))
        {
            return input;
        }

        int index = input.IndexOf(oldValue);

        if (index < 0)
        {
            return input;
        }

        return string.Concat(input.AsSpan(0, index), newValue, input.AsSpan(index + oldValue.Length));
    }

    /// <summary>
    /// 生成字符串
    /// </summary>
    /// <param name="strings">列表</param>
    /// <returns>输出结果</returns>
    public static string MakeString(HashSet<string>? strings)
    {
        if (strings == null)
            return "";
        string temp = "";
        foreach (var item in strings)
        {
            temp += item + ",";
        }

        if (temp.Length > 0)
        {
            return temp[..^1];
        }

        return temp;
    }

    /// <summary>
    /// 生成字符串
    /// </summary>
    /// <param name="strings">列表</param>
    /// <returns>输出结果</returns>
    public static string MakeString(HashSet<Loaders>? strings)
    {
        if (strings == null)
            return "";
        string temp = "";
        foreach (var item in strings)
        {
            temp += item + ",";
        }

        if (temp.Length > 0)
        {
            return temp[..^1];
        }

        return temp;
    }
}
