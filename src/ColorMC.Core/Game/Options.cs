using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;

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
    //public static Dictionary<string, string> ReadOptions(this GameSettingObj game)
    //{
    //    var file = game.GetOptionsFile();
    //    if (File.Exists(file))
    //        return new();
    //    return ReadOptions(File.ReadAllText(file));
    //}

    //public static void SaveOptions(this GameSettingObj game, Dictionary<string, string> dir)
    //{
    //    string data = "";
    //    foreach (var item in dir)
    //    {
    //        data += $"{item.Key}:{item.Value}{Environment.NewLine}";
    //    }

    //    File.WriteAllText(game.GetOptionsFile(), data);
    //}
}
