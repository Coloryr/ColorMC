using ColorMC.Core.Objs;
using ColorMC.Core.LaunchPath;

namespace ColorMC.Core.Game;

public static class Options
{
    public static Dictionary<string, string> ReadOptions(this GameSettingObj obj)
    {
        var options = new Dictionary<string, string>();
        string file = InstancesPath.GetDir(obj) + "/options.txt";
        if (File.Exists(file))
        {
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
        }
        return options;
    }

    public static void SaveOptions(this GameSettingObj obj, Dictionary<string, string> dir)
    {

    }
}
