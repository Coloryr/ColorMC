﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.NetFrp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ColorMC.Gui.Net.Apis;

/// <summary>
/// ColorMC相关API
/// </summary>
public static class ColorMCCloudAPI
{
    private static readonly HttpClient s_client = new()
    {
        Timeout = Timeout.InfiniteTimeSpan
    };

    public static string Server { get; set; }
    public static string Serverkey { get; set; }
    public static string Clientkey { get; set; }

    public static string Info { get; private set; } = App.Lang("GameCloudUtils.Error2");
    public static bool Connect { get; private set; }

    /// <summary>
    /// 更新检查网址
    /// </summary>
    public const string CheckUrl = $"{ColorMCAPI.BaseUrl}update/{ColorMCCore.TopVersion}/";

    /// <summary>
    /// 获取更新日志
    /// </summary>
    /// <returns>日志内容</returns>
    public static async Task<string?> GetNewLog()
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, ColorMCAPI.BaseUrl + "update/log");
            req.Headers.Add("ColorMC", ColorMCCore.Version);
            var data = await CoreHttpClient.DownloadClient.SendAsync(req);
            return await data.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.ColorMC.Error2"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取主版本号
    /// </summary>
    /// <returns></returns>
    public static async Task<JObject> GetUpdateIndex()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, ColorMCAPI.BaseUrl + "update/index.json");
        req.Headers.Add("ColorMC", ColorMCCore.Version);
        var data = await CoreHttpClient.DownloadClient.SendAsync(req);
        return JObject.Parse(await data.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// 获取文件Sha1
    /// </summary>
    /// <returns></returns>
    public static async Task<JObject> GetUpdateSha1()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, CheckUrl + "sha1.json");
        req.Headers.Add("ColorMC", ColorMCCore.Version);
        var data = await CoreHttpClient.DownloadClient.SendAsync(req);
        string text = await data.Content.ReadAsStringAsync();
        return JObject.Parse(text);
    }

    /// <summary>
    /// 获取在线服务器
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns></returns>
    public static async Task<JObject?> GetCloudServer(string version)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, ColorMCAPI.BaseUrl + "frplist?version=" + version);
            req.Headers.Add("ColorMC", ColorMCCore.Version);
            var data = await CoreHttpClient.DownloadClient.SendAsync(req);
            return JObject.Parse(await data.Content.ReadAsStringAsync());
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.ColorMC.Error4"), e);
            return null;
        }
    }

    /// <summary>
    /// 上传在线服务器
    /// </summary>
    /// <param name="token">Minecraft token</param>
    /// <param name="ip">IP地址</param>
    /// <param name="model">显示内容</param>
    /// <returns>是否上传成功</returns>
    public static async Task<bool> PutCloudServer(string token, string ip, FrpShareModel model)
    {
        var httpRequest = new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(ColorMCAPI.BaseUrl + "frp"),
            Content = new StringContent(JsonConvert.SerializeObject(new
            {
                token,
                ip,
                custom = new
                {
                    model.Version,
                    model.Loader,
                    model.IsLoader,
                    model.Text
                }
            }))
        };
        httpRequest.Headers.Add("ColorMC", ColorMCCore.Version);

        try
        {
            var data = await CoreHttpClient.DownloadClient.SendAsync(httpRequest);
            var data1 = await data.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(data1))
            {
                return false;
            }
            var obj = JObject.Parse(data1);
            if (obj.TryGetValue("res", out var res) && ((int)res) != 100)
            {
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.ColorMC.Error3"), e);
            return false;
        }
    }

    /// <summary>
    /// 检查云服务器是否在线
    /// </summary>
    public static async Task Check()
    {
        var requ = new HttpRequestMessage(HttpMethod.Post,
            new Uri(Server + "/check"));
        requ.Headers.Add("ColorMC", ColorMCCore.Version);
        requ.Headers.Add("serverkey", Serverkey);
        requ.Headers.Add("clientkey", Clientkey);

        var res = await s_client.SendAsync(requ);
        if (res.IsSuccessStatusCode)
        {
            var data = await res.Content.ReadAsStringAsync();
            var obj = JObject.Parse(data);
            if (!obj.TryGetValue("res", out var res1))
            {
                var value = (int)res1!;
                if (value == 300)
                {
                    Info = App.Lang("GameCloudUtils.Error4");
                    return;
                }
                else if (value != 100)
                {
                    Info = App.Lang("GameCloudUtils.Error5");
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
    public static async Task<CloudRes> HaveCloud(GameSettingObj obj)
    {
        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(Server + "/haveuuid"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);
            requ.Headers.Add("uuid", obj.UUID);

            var res = await s_client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj1 = JObject.Parse(data);
                if (obj1.TryGetValue("res", out var res1) && (int)res1 == 100)
                {
                    return new CloudRes()
                    {
                        State = true,
                        Data1 = (bool)obj1["have"]!,
                        Data2 = (string?)obj1["time"]
                    };
                }
            }
        }
        catch (Exception e)
        {
            string temp = App.Lang("GameCloudWindow.Error3");
            Logs.Error(temp, e);
        }

        return new()
        {
            Message = App.Lang("GameCloudWindow.Error3")
        };
    }

    /// <summary>
    /// 实例开启云同步
    /// </summary>
    public static async Task<int> StartCloud(GameSettingObj obj)
    {
        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(Server + "/startuuid"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);
            requ.Headers.Add("uuid", obj.UUID);
            requ.Headers.Add("name", obj.Name);

            var res = await s_client.SendAsync(requ);
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
            string temp = App.Lang("GameCloudWindow.Error3");
            Logs.Error(temp, e);
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
        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(Server + "/stopuuid"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);
            requ.Headers.Add("uuid", obj.UUID);

            var res = await s_client.SendAsync(requ);
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
            string temp = App.Lang("GameCloudWindow.Error3");
            Logs.Error(temp, e);
        }

        return -1;
    }

    /// <summary>
    /// 上传游戏实例的配置文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="path">配置文件路径</param>
    /// <returns></returns>
    public static async Task<CloudUploadRes> UploadConfig(GameSettingObj obj, string path)
    {
        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(Server + "/putconfig"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);
            requ.Headers.Add("uuid", obj.UUID);
            using var stream = PathHelper.OpenRead(path)!;
            requ.Content = new StreamContent(stream);

            var res = await s_client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj1 = JObject.Parse(data);

                if (obj1.TryGetValue("res", out var res1))
                {
                    return new CloudUploadRes()
                    {
                        State = true,
                        Data1 = (int)res1,
                        Data2 = obj1["time"]?.ToString()
                    };
                }
            }
        }
        catch (Exception e)
        {
            string temp = App.Lang("GameCloudWindow.Error3");
            Logs.Error(temp, e);
        }

        return new();
    }

    /// <summary>
    /// 下载配置压缩包
    /// </summary>
    /// <param name="uuid">游戏实例UUID</param>
    /// <param name="local">压缩包位置</param>
    /// <returns></returns>
    public static async Task<int> DownloadConfig(string uuid, string local)
    {
        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(Server + "/pullconfig"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);
            requ.Headers.Add("uuid", uuid);

            var res = await s_client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                using var data = res.Content.ReadAsStream();
                await PathHelper.WriteBytesAsync(local, data);
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
            string temp = App.Lang("GameCloudWindow.Error3");
            Logs.Error(temp, e);
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
                 new Uri(Server + "/getstate"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);

            var res = await s_client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(data);
                if (!obj.TryGetValue("res", out var res1) || (int)res1 != 100)
                {
                    Info = App.Lang("GameCloudUtils.Error5");
                    return;
                }
                Info = string.Format(App.Lang("GameCloudUtils.Info1"), obj["use"], obj["size"]);
            }
        }
        catch (Exception e)
        {
            string temp = App.Lang("GameCloudWindow.Error3");
            Logs.Error(temp, e);
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
                new Uri(Server + "/getlist"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);

            var res = await s_client.SendAsync(requ);
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
            string temp = App.Lang("GameCloudWindow.Error3");
            Logs.Error(temp, e);
        }
        return null;
    }

    /// <summary>
    /// 获取世界列表
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns></returns>
    public static async Task<CloudWorldRes> GetWorldList(GameSettingObj game)
    {
        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(Server + "/getworldlist"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);
            requ.Headers.Add("uuid", game.UUID);

            var res = await s_client.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(data);
                if (obj.TryGetValue("res", out var res1) && (int)res1 == 100)
                {
                    return new()
                    {
                        State = true,
                        Data = obj["list"]?.ToObject<List<CloudWorldObj>>()
                    };
                }
            }
        }
        catch (Exception e)
        {
            string temp = App.Lang("GameCloudWindow.Error3");
            Logs.Error(temp, e);
        }

        return new()
        {
            Message = App.Lang("GameCloudWindow.Error3")
        };
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
        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(Server + "/putworld"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);
            requ.Headers.Add("uuid", game.UUID);
            requ.Headers.Add("name", UrlEncoder.Default.Encode(world.LevelName));
            using var stream = PathHelper.OpenRead(local)!;
            requ.Content = new StreamContent(stream);

            var res = await s_client.SendAsync(requ);
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
            string temp = App.Lang("GameCloudWindow.Error3");
            Logs.Error(temp, e);
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
                new Uri(Server + "/getworldfiles"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);
            requ.Headers.Add("uuid", game.UUID);
            requ.Headers.Add("name", UrlEncoder.Default.Encode(world.LevelName));

            var res = await s_client.SendAsync(requ);
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
            string temp = App.Lang("GameCloudWindow.Error3");
            Logs.Error(temp, e);
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
                new Uri(Server + "/pullworld"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);
            requ.Headers.Add("uuid", game.UUID);
            requ.Headers.Add("name", UrlEncoder.Default.Encode(world.Name));
            requ.Content = new StringContent(JsonConvert.SerializeObject(list));

            var res = await s_client.SendAsync(requ);
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
                await PathHelper.WriteBytesAsync(local, data);
                return 100;
            }

            return -1;
        }
        catch (Exception e)
        {
            string temp = App.Lang("GameCloudWindow.Error3");
            Logs.Error(temp, e);
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
        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(Server + "/deleteworld"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);
            requ.Headers.Add("uuid", game.UUID);
            requ.Headers.Add("name", UrlEncoder.Default.Encode(name));

            var res = await s_client.SendAsync(requ);
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
            string temp = App.Lang("GameCloudWindow.Error3");
            Logs.Error(temp, e);
        }

        return -1;
    }
}
