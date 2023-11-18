using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using System.Text;

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
            return ReadOptions(PathHelper.ReadText(file)!);
        }

        return new();
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
    /// <param name="file">文件</param>
    /// <param name="sp">分隔符</param>
    /// <returns>配置文件组</returns>
    public static Dictionary<string, string> ReadOptions(string file, string sp = ":")
    {
        var options = new Dictionary<string, string>();

        var lines = file.Split("\n");
        foreach (var item in lines)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                continue;
            }
            var temp = item.Trim().Split(sp);
            options.Remove(temp[0]);
            if (temp.Length == 1)
            {
                options.Add(temp[0], "");
            }
            else if (temp.Length > 2)
            {
                options.Add(temp[0], MakeString(temp, 1, sp));
            }
            else
            {
                options.Add(temp[0], temp[1]);
            }
        }

        return options;
    }

    /// <summary>
    /// 组合字符串
    /// </summary>
    /// <param name="input"></param>
    /// <param name="index"></param>
    /// <param name="sp"></param>
    /// <returns></returns>
    private static string MakeString(string[] input, int index, string sp)
    {
        string temp = "";
        for (int i = index; i < input.Length; i++)
        {
            temp += input[i] + sp;
        }

        return temp[0..^(sp.Length)];
    }
}
