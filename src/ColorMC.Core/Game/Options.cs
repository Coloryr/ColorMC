namespace ColorMC.Core.Game;

/// <summary>
/// 配置文件相关操作
/// </summary>
public static class Options
{
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
            if (options.ContainsKey(temp[0]))
            {
                options.Remove(temp[0]);
            }
            if (temp.Length == 1)
            {
                options.Add(temp[0], "");
            }
            else
            {
                options.Add(temp[0], temp[1]);
            }
        }

        return options;
    }
}
