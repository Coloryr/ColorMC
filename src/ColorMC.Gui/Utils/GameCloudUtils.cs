using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Config;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.Objs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 游戏云同步
/// </summary>
public static class GameCloudUtils
{
    public const string Name1 = "cloud.json";

    private static Dictionary<string, CloudDataObj> s_datas;
    private static string s_file;

    /// <summary>
    /// 初始化云同步
    /// </summary>
    public static void Init()
    {
        s_file = Path.Combine(ColorMCGui.RunDir, Name1);

        if (File.Exists(s_file))
        {
            try
            {
                s_datas = JsonConvert.DeserializeObject<Dictionary<string, CloudDataObj>>
                    (File.ReadAllText(s_file))!;
                var games = InstancesPath.Games;
                bool save = false;
                foreach (var item in new List<string>(s_datas.Keys))
                {
                    if (games.Any(a => a.UUID == item))
                    {
                        continue;
                    }

                    s_datas.Remove(item);
                    save = true;
                }
                if (save)
                {
                    Save();
                }
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("GameCloudUtils.Error1"), e);
            }
        }

        if (s_datas == null)
        {
            s_datas = [];
            Save();
        }
    }

    /// <summary>
    /// 保存云同步储存
    /// </summary>
    public static void Save()
    {
        ConfigSave.AddItem(new()
        {
            Name = Name1,
            File = s_file,
            Obj = s_datas
        });
    }

    /// <summary>
    /// 获取云同步数据
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>数据</returns>
    public static CloudDataObj GetCloudData(GameSettingObj obj)
    {
        if (s_datas.TryGetValue(obj.UUID, out var temp))
        {
            return temp;
        }

        var obj1 = new CloudDataObj();
        s_datas.Add(obj.UUID, obj1);
        Save();
        return obj1;
    }

    /// <summary>
    /// 设置云同步数据
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="obj1">云同步数据</param>
    public static void SetCloudData(GameSettingObj obj, CloudDataObj obj1)
    {
        s_datas.Remove(obj.UUID);
        s_datas.Add(obj.UUID, obj1);
        Save();
    }

    /// <summary>
    /// 开始链接云服务器
    /// </summary>
    public static async Task StartConnect()
    {
        var config = GuiConfigUtils.Config.ServerKey;
        if (string.IsNullOrWhiteSpace(config))
        {
            return;
        }

        try
        {
            var data = Convert.FromBase64String(config);
            var obj = JObject.Parse(Encoding.UTF8.GetString(data));
            if (obj == null)
            {
                return;
            }
            if (obj.TryGetValue("server", out var server) && server != null)
            {
                ColorMCCloudAPI.Server = server.ToString();
            }
            if (obj.TryGetValue("serverkey", out var serverkey) && serverkey != null)
            {
                ColorMCCloudAPI.Serverkey = serverkey.ToString();
            }
            if (obj.TryGetValue("clientkey", out var clientkey) && clientkey != null)
            {
                ColorMCCloudAPI.Clientkey = clientkey.ToString();
            }

            if (string.IsNullOrWhiteSpace(ColorMCCloudAPI.Server)
                || string.IsNullOrWhiteSpace(ColorMCCloudAPI.Serverkey)
                || string.IsNullOrWhiteSpace(ColorMCCloudAPI.Clientkey))
            {
                return;
            }

            await ColorMCCloudAPI.Check();
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("GameCloudUtils.Error3"), e);
        }
    }
}
