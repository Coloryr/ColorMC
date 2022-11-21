using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;

namespace ColorMC.Core.Game;

public static class Options
{
    public static Dictionary<string, string> ReadOptions(this GameSettingObj game)
    {
        var options = new Dictionary<string, string>();
        string file = game.GetOptionsFile();
        if (File.Exists(file))
            return options;

        var lines = File.ReadAllLines(file);
        foreach (var item in lines)
        {
            var temp = item.Split(":");
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

    public static void SaveOptions(this GameSettingObj game, Dictionary<string, string> dir)
    {
        string data = "";
        foreach (var item in dir)
        {
            data += $"{item.Key}:{item.Value}{Environment.NewLine}";
        }

        File.WriteAllText(game.GetOptionsFile(), data);
    }
}
