using ColorMC.Core.Http;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ColorMC.Core.Game.Auth;

namespace ColorMC.Gui;

public record LastUser
{
    public string UUID { get; set; }
    public AuthType Type { get; set; }
}


public record GuiConfigObj
{
    public string Version { get; set; }
    public LastUser LastUser { get; set; }
    public string BackImage { get; set; }
}

public static class GuiConfigUtils
{
    public static GuiConfigObj Config { get; set; }

    public static string Dir;

    private static string Name;

    public static void Init(string dir)
    {
        Dir = dir;
        Name = dir + "gui.json";

        Load(Name);
    }

    public static bool Load(string name, bool quit = false)
    {
        Logs.Info($"正在读取配置文件");
        if (File.Exists(name))
        {
            try
            {
                Config = JsonConvert.DeserializeObject<GuiConfigObj>(File.ReadAllText(name))!;
            }
            catch (Exception e)
            {
                CoreMain.OnError?.Invoke("配置文件读取错误", e, true);
                Logs.Error("配置文件读取错误", e);
            }
        }

        if (Config == null)
        {
            if (quit)
            {
                return false;
            }
            Logs.Warn("配置为空，旧版配置文件会被覆盖");

            Config = MakeDefaultConfig();
            Save();
        }
        else
        {
            
        }

        Save();

        return true;
    }

    public static void Save()
    {
        Logs.Info($"正在保存配置文件");
        File.WriteAllText(Name, JsonConvert.SerializeObject(Config, Formatting.Indented));
    }

    private static GuiConfigObj MakeDefaultConfig()
    {
        return new()
        {
            Version = CoreMain.Version,
        };
    }
}
