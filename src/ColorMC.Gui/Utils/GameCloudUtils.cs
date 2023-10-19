using ColorMC.Core;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 游戏云同步
/// </summary>
public static class GameCloudUtils
{
    public const string Name = "cloud.json";

    private static string s_server;
    private static string s_serverkey;
    private static string s_clientkey;

    private static Dictionary<string, CloudDataObj> s_datas;
    private static string s_file;

    public static string Info { get; private set; }

    public static bool Connect { get; private set; }

    private static HttpClient _client;

    /// <summary>
    /// 初始化云同步
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        HttpClientHandler handler = new HttpClientHandler();
        _client = new HttpClient(handler);
        _client.Timeout = Timeout.InfiniteTimeSpan;

        s_file = Path.GetFullPath(dir + "/" + Name);

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
                Logs.Error(App.GetLanguage("Gui.Error36"), e);
            }
        }

        if (s_datas == null)
        {
            s_datas = new();
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
            Name = Name,
            Local = s_file,
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
        Connect = false;
        Info = App.GetLanguage("GameCloud.Error2");
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
                s_server = server.ToString();
            }
            if (obj.TryGetValue("serverkey", out var serverkey) && serverkey != null)
            {
                s_serverkey = serverkey.ToString();
            }
            if (obj.TryGetValue("clientkey", out var clientkey) && clientkey != null)
            {
                s_clientkey = clientkey.ToString();
            }

            if (string.IsNullOrWhiteSpace(s_server)
                || string.IsNullOrWhiteSpace(s_serverkey)
                || string.IsNullOrWhiteSpace(s_clientkey))
            {
                return;
            }

            await Check();
        }
        catch (Exception e)
        {
            Logs.Error(App.GetLanguage("Gui.Error37"), e);
        }
    }

    /// <summary>
    /// 检查云服务器是否在线
    /// </summary>
    private static async Task Check()
    {
        var requ = new HttpRequestMessage(HttpMethod.Post,
            new Uri(s_server + "/check"));
        requ.Headers.Add("ColorMC", ColorMCCore.Version);
        requ.Headers.Add("serverkey", s_serverkey);
        requ.Headers.Add("clientkey", s_clientkey);

        var res = await _client.SendAsync(requ);
        if (res.IsSuccessStatusCode)
        {
            var data = await res.Content.ReadAsStringAsync();
            var obj = JObject.Parse(data);
            if (!obj.TryGetValue("res", out var res1))
            {
                var value = (int)res1!;
                if (value == 300)
                {
                    Info = App.GetLanguage("GameCloud.Error3");
                    return;
                }
                else if (value != 100)
                {
                    Info = App.GetLanguage("GameCloud.Error1");
                    return;
                }
            }
            Connect = true;
            await GetState();
        }
    }

    /// <summary>
    /// 检查游戏实例是否启用了云同步
    /// </summary>
    public static async Task<(int, bool, string?)> HaveCloud(GameSettingObj obj)
    {
        try
        {
            if (!Connect)
            {
                return (-1, false, null);
            }

            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(s_server + "/haveuuid"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", s_serverkey);
            requ.Headers.Add("clientkey", s_clientkey);
            requ.Headers.Add("uuid", obj.UUID);

            var res = await _client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj1 = JObject.Parse(data);
                if (obj1.TryGetValue("res", out var res1))
                {
                    if ((int)res1 == 100)
                    {
                        return (100, (bool)obj1["have"]!, (string?)obj1["time"]);
                    }
                    else
                    {
                        return ((int)res1, false, null);
                    }
                }
            }
        }
        catch (Exception e)
        {
            string temp = App.GetLanguage("GameCloudWindow.Error3");
            Logs.Error(temp, e);
            App.ShowError(temp, e);
        }

        return (-1, false, null);
    }

    /// <summary>
    /// 实例开启云同步
    /// </summary>
    public static async Task<int> StartCloud(GameSettingObj obj)
    {
        if (!Connect)
        {
            return -1;
        }

        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(s_server + "/startuuid"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", s_serverkey);
            requ.Headers.Add("clientkey", s_clientkey);
            requ.Headers.Add("uuid", obj.UUID);
            requ.Headers.Add("name", obj.Name);

            var res = await _client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj1 = JObject.Parse(data);
                if (obj1.TryGetValue("res", out var res1))
                {
                    return (int)res1;
                }
            }
        }
        catch (Exception e)
        {
            string temp = App.GetLanguage("GameCloudWindow.Error3");
            Logs.Error(temp, e);
            App.ShowError(temp, e);
        }

        return -1;
    }

    /// <summary>
    /// 关闭游戏实例的云同步
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static async Task<int> StopCloud(GameSettingObj obj)
    {
        if (!Connect)
        {
            return -1;
        }

        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(s_server + "/stopuuid"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", s_serverkey);
            requ.Headers.Add("clientkey", s_clientkey);
            requ.Headers.Add("uuid", obj.UUID);

            var res = await _client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj1 = JObject.Parse(data);
                if (obj1.TryGetValue("res", out var res1))
                {
                    return (int)res1;
                }
            }

        }
        catch (Exception e)
        {
            string temp = App.GetLanguage("GameCloudWindow.Error3");
            Logs.Error(temp, e);
            App.ShowError(temp, e);
        }
        return -1;
    }

    /// <summary>
    /// 上传游戏实例的配置文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="path">配置文件路径</param>
    /// <returns></returns>
    public static async Task<int> UploadConfig(GameSettingObj obj, string path)
    {
        if (!Connect)
        {
            return -1;
        }

        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(s_server + "/putconfig"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", s_serverkey);
            requ.Headers.Add("clientkey", s_clientkey);
            requ.Headers.Add("uuid", obj.UUID);
            using var stream = PathHelper.OpenRead(path)!;
            requ.Content = new StreamContent(stream);

            var res = await _client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj1 = JObject.Parse(data);

                if (obj1.TryGetValue("res", out var res1))
                {
                    return (int)res1;
                }
            }
        }
        catch (Exception e)
        {
            string temp = App.GetLanguage("GameCloudWindow.Error3");
            Logs.Error(temp, e);
            App.ShowError(temp, e);

        }

        return -1;
    }

    /// <summary>
    /// 下载配置压缩包
    /// </summary>
    /// <param name="game">云游戏实例</param>
    /// <param name="local">压缩包位置</param>
    /// <returns></returns>
    public static Task<int> DownloadConfig(CloundListObj game, string local)
    {
        return DownloadConfig(game.UUID, local);
    }

    /// <summary>
    /// 下载配置压缩包
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="local">压缩包位置</param>
    /// <returns></returns>
    public static Task<int> DownloadConfig(GameSettingObj game, string local)
    {
        return DownloadConfig(game.UUID, local);
    }

    /// <summary>
    /// 下载配置压缩包
    /// </summary>
    /// <param name="uuid">游戏实例UUID</param>
    /// <param name="local">压缩包位置</param>
    /// <returns></returns>
    private static async Task<int> DownloadConfig(string uuid, string local)
    {
        if (!Connect)
        {
            return -1;
        }

        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(s_server + "/pullconfig"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", s_serverkey);
            requ.Headers.Add("clientkey", s_clientkey);
            requ.Headers.Add("uuid", uuid);

            var res = await _client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                using var data = res.Content.ReadAsStream();
                using var stream = PathHelper.OpenWrite(local);
                await data.CopyToAsync(stream);
                return 100;
            }
            else if (res.StatusCode == HttpStatusCode.BadRequest)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj1 = JObject.Parse(data);

                if (obj1.TryGetValue("res", out var res1))
                {
                    return (int)res1;
                }
            }
        }
        catch (Exception e)
        {
            string temp = App.GetLanguage("GameCloudWindow.Error3");
            Logs.Error(temp, e);
            App.ShowError(temp, e);
        }

        return -1;
    }

    /// <summary>
    /// 获取云储存状态
    /// </summary>
    /// <returns></returns>
    public static async Task GetState()
    {
        if (!Connect)
        {
            return;
        }

        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                 new Uri(s_server + "/getstate"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", s_serverkey);
            requ.Headers.Add("clientkey", s_clientkey);

            var res = await _client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(data);
                if (!obj.TryGetValue("res", out var res1) || (int)res1 != 100)
                {
                    Info = App.GetLanguage("GameCloud.Error1");
                    return;
                }
                Info = string.Format(App.GetLanguage("GameCloud.Info1"), obj["use"], obj["size"]);
            }
        }
        catch (Exception e)
        {
            string temp = App.GetLanguage("GameCloudWindow.Error3");
            Logs.Error(temp, e);
            App.ShowError(temp, e);
        }
    }

    /// <summary>
    /// 获取云端游戏实例
    /// </summary>
    /// <returns>云游戏实例列表</returns>
    public static async Task<List<CloundListObj>?> GetList()
    {
        if (!Connect)
        {
            return null;
        }

        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(s_server + "/getlist"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", s_serverkey);
            requ.Headers.Add("clientkey", s_clientkey);

            var res = await _client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(data);
                if (!obj.TryGetValue("res", out var res1) || (int)res1 != 100)
                {
                    return null;
                }
                return obj["list"]?.ToObject<List<CloundListObj>>();
            }
        }
        catch (Exception e)
        {
            string temp = App.GetLanguage("GameCloudWindow.Error3");
            Logs.Error(temp, e);
            App.ShowError(temp, e);
        }
        return null;
    }

    /// <summary>
    /// 获取世界列表
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns></returns>
    public static async Task<List<CloudWorldObj>?> GetWorldList(GameSettingObj game)
    {
        if (!Connect)
        {
            return null;
        }
        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(s_server + "/getworldlist"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", s_serverkey);
            requ.Headers.Add("clientkey", s_clientkey);
            requ.Headers.Add("uuid", game.UUID);

            var res = await _client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(data);
                if (!obj.TryGetValue("res", out var res1) || (int)res1 != 100)
                {
                    return null;
                }
                return obj["list"]?.ToObject<List<CloudWorldObj>>();
            }
        }
        catch (Exception e)
        {
            string temp = App.GetLanguage("GameCloudWindow.Error3");
            Logs.Error(temp, e);
            App.ShowError(temp, e);
        }
        return null;
    }

    /// <summary>
    /// 上传游戏实例
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="world">世界储存</param>
    /// <param name="local">压缩包路径</param>
    /// <returns></returns>
    public static async Task<int> UploadWorld(GameSettingObj game, WorldObj world, string local)
    {
        if (!Connect)
        {
            return -1;
        }
        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(s_server + "/putworld"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", s_serverkey);
            requ.Headers.Add("clientkey", s_clientkey);
            requ.Headers.Add("uuid", game.UUID);
            requ.Headers.Add("name", UrlEncoder.Default.Encode(world.LevelName));
            using var stream = PathHelper.OpenRead(local)!;
            requ.Content = new StreamContent(stream);

            var res = await _client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(data);

                if (obj.TryGetValue("res", out var res1))
                {
                    return (int)res1;
                }
            }
        }
        catch (Exception e)
        {
            string temp = App.GetLanguage("GameCloudWindow.Error3");
            Logs.Error(temp, e);
            App.ShowError(temp, e);
        }
        return -1;
    }

    /// <summary>
    /// 获取世界云端文件列表
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="name">世界储存</param>
    /// <returns></returns>
    public static async Task<Dictionary<string, string>?> GetWorldFiles(GameSettingObj game, WorldObj world)
    {
        if (!Connect)
        {
            return null;
        }
        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(s_server + "/getworldfiles"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", s_serverkey);
            requ.Headers.Add("clientkey", s_clientkey);
            requ.Headers.Add("uuid", game.UUID);
            requ.Headers.Add("name", UrlEncoder.Default.Encode(world.LevelName));

            var res = await _client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(data);

                if (obj.TryGetValue("res", out var res1) && (int)res1 != 100)
                {
                    return null;
                }

                return obj["list"]?.ToObject<Dictionary<string, string>>();
            }
        }
        catch (Exception e)
        {
            string temp = App.GetLanguage("GameCloudWindow.Error3");
            Logs.Error(temp, e);
            App.ShowError(temp, e);
        }
        return null;
    }

    /// <summary>
    /// 下载云端世界文件
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="world">云存档</param>
    /// <param name="local">压缩包路径</param>
    /// <param name="list">文件列表</param>
    /// <returns></returns>
    public static async Task<int> DownloadWorld(GameSettingObj game, CloudWorldObj world,
        string local, Dictionary<string, string> list)
    {
        if (!Connect)
        {
            return -1;
        }

        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(s_server + "/pullworld"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", s_serverkey);
            requ.Headers.Add("clientkey", s_clientkey);
            requ.Headers.Add("uuid", game.UUID);
            requ.Headers.Add("name", UrlEncoder.Default.Encode(world.Name));
            requ.Content = new StringContent(JsonConvert.SerializeObject(list));

            var res = await _client.SendAsync(requ);
            if (res.StatusCode == HttpStatusCode.BadRequest)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(data);

                if (obj.TryGetValue("res", out var res1))
                {
                    return (int)res1;
                }
            }
            else if (res.IsSuccessStatusCode)
            {
                using var data = res.Content.ReadAsStream();
                using var stream = PathHelper.OpenWrite(local);
                await data.CopyToAsync(stream);
                return 100;
            }

            return -1;
        }
        catch (Exception e)
        {
            string temp = App.GetLanguage("GameCloudWindow.Error3");
            Logs.Error(temp, e);
            App.ShowError(temp, e);
            return -1;
        }
    }

    /// <summary>
    /// 删除云端的存档
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="name">存档</param>
    /// <returns></returns>
    public static async Task<int> DeleteWorld(GameSettingObj game, string name)
    {
        if (!Connect)
        {
            return -1;
        }

        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(s_server + "/deleteworld"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", s_serverkey);
            requ.Headers.Add("clientkey", s_clientkey);
            requ.Headers.Add("uuid", game.UUID);
            requ.Headers.Add("name", UrlEncoder.Default.Encode(name));

            var res = await _client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(data);

                if (obj.TryGetValue("res", out var res1))
                {
                    return (int)res1;
                }
            }

            return -1;
        }
        catch (Exception e)
        {
            string temp = App.GetLanguage("GameCloudWindow.Error3");
            Logs.Error(temp, e);
            App.ShowError(temp, e);
            return -1;
        }
    }
}
