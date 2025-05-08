using System.Text;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;

namespace ColorMC.Core.Game;

/// <summary>
/// 配置文件相关操作
/// </summary>
public static class Options
{
    /// <summary>
    /// 获取设置选项
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>选项列表</returns>
    public static Dictionary<string, string> GetOptions(this GameSettingObj obj)
    {
        var file = obj.GetOptionsFile();
        if (File.Exists(file))
        {
            return ReadOptions(PathHelper.OpenRead(file)!);
        }

        return [];
    }

    /// <summary>
    /// 报错设置选项
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="list">选项列表</param>
    /// <param name="sp">分隔符</param>
    public static void SaveOptions(this GameSettingObj obj, Dictionary<string, string> list, string sp = ":")
    {
        var file = obj.GetOptionsFile();
        var builder = new StringBuilder();
        foreach (var item in list)
        {
            builder.Append(item.Key).Append(sp).AppendLine(item.Value);
        }
        PathHelper.WriteText(file, builder.ToString());
    }

    /// <summary>
    /// 读取配置文件
    /// </summary>
    /// <param name="stream">内容</param>
    /// <param name="sp">分隔符</param>
    /// <returns>配置文件组</returns>
    public static Dictionary<string, string> ReadOptions(Stream? stream, string sp = ":")
    {
        if (stream == null)
        {
            return [];
        }
        var options = new Dictionary<string, string>();
        using var reader = new StreamReader(stream);

        while(reader.ReadLine() is { } item)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                continue;
            }
            var item1 = item.Trim();
            var index = item1.IndexOf(sp);
            string key, value;
            if (index == -1)
            {
                key = item1;
                value = "";
            }
            else if (index + 1 == item1.Length)
            {
                key = item1[..^1];
                value = "";
            }
            else
            {
                key = item1[..index];
                value = item1[(index + sp.Length)..];
            }

            if (!options.TryAdd(key, value))
            {
                options[key] = value;
            }
        }

        return options;
    }
}
