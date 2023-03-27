namespace ColorMC.Core.Game;

public static class Options
{
    /// <summary>
    /// 读取配置文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="sp">分隔符</param>
    /// <returns></returns>
    public static Dictionary<string, string> ReadOptions(string file, string sp = ":")
    {
        var options = new Dictionary<string, string>();

        var lines = file.Split("\n");
        foreach (var item in lines)
        {
            var temp = item.Trim().Split(sp);
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
