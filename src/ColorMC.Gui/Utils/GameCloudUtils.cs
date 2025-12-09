using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.Objs.ColorMC;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 游戏云同步
/// </summary>
public static class GameCloudUtils
{
    /// <summary>
    /// 云同步储存
    /// </summary>
    private static Dictionary<Guid, CloudDataObj> s_datas;
    /// <summary>
    /// 云同步配置文件
    /// </summary>
    private static string s_file;

    /// <summary>
    /// 初始化云同步
    /// </summary>
    public static void Init()
    {
        s_file = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameGameCloudFile);

        if (File.Exists(s_file))
        {
            try
            {
                bool save = false;
                using var stream = PathHelper.OpenRead(s_file);
                var obj = JsonUtils.ToObj(stream, JsonGuiType.DictionaryStringCloudDataObj);
                if (obj != null)
                {
                    s_datas = [];
                    foreach (var item in obj)
                    {
                        s_datas.Add(Guid.Parse(item.Key), item.Value);
                    }
                    var games = InstancesPath.Games;
                    foreach (var item in new List<Guid>(s_datas.Keys))
                    {
                        if (games.Any(a => a.UUID == item))
                        {
                            continue;
                        }

                        s_datas.Remove(item);
                        save = true;
                    }
                }
                else
                {
                    save = true;
                }

                if (save)
                {
                    Save();
                }
            }
            catch (Exception e)
            {
                Logs.Error(LanguageUtils.Get("App.Text95"), e);
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
        var list = new Dictionary<string, CloudDataObj>();
        foreach (var item in s_datas)
        {
            list.Add(item.Key.ToString(), item.Value);
        }
        ConfigSave.AddItem(ConfigSaveObj.Build(GuiNames.NameGameCloudFile, 
            s_file, list, JsonGuiType.DictionaryStringCloudDataObj));
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
            var obj = JsonDocument.Parse(Encoding.UTF8.GetString(data));
            if (obj == null)
            {
                return;
            }
            var json = obj.RootElement;
            if (json.TryGetProperty(GuiNames.NameServerKey, out var server) && server.ValueKind is JsonValueKind.String)
            {
                ColorMCCloudAPI.Server = server.GetString()!;
            }
            if (json.TryGetProperty(GuiNames.NameServerKey1, out var serverkey) && serverkey.ValueKind is JsonValueKind.String)
            {
                ColorMCCloudAPI.Serverkey = serverkey.GetString()!;
            }
            if (json.TryGetProperty(GuiNames.NameServerKey2, out var clientkey) && clientkey.ValueKind is JsonValueKind.String)
            {
                ColorMCCloudAPI.Clientkey = clientkey.GetString()!;
            }

            if (string.IsNullOrWhiteSpace(ColorMCCloudAPI.Server)
                || string.IsNullOrWhiteSpace(ColorMCCloudAPI.Serverkey)
                || string.IsNullOrWhiteSpace(ColorMCCloudAPI.Clientkey))
            {
                return;
            }

            await ColorMCCloudAPI.CheckAsync();
        }
        catch (Exception e)
        {
            Logs.Error(LanguageUtils.Get("App.Text97"), e);
        }
    }
}
