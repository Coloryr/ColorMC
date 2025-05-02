using System;
using System.Collections.Generic;
using System.IO;
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
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Frp;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ColorMC.Gui.Net.Apis;

/// <summary>
/// ColorMC云同步相关API
/// </summary>
public static class ColorMCCloudAPI
{
    /// <summary>
    /// 服务器地址
    /// </summary>
    public static string Server { get; set; }
    /// <summary>
    /// 服务器键
    /// </summary>
    public static string Serverkey { get; set; }
    /// <summary>
    /// 客户端ID
    /// </summary>
    public static string Clientkey { get; set; }

    /// <summary>
    /// 云同步信息
    /// </summary>
    public static string Info { get; private set; } = App.Lang("GameCloudUtils.Error2");
    /// <summary>
    /// 是否已连接云同步服务器
    /// </summary>
    public static bool Connect { get; private set; }

    /// <summary>
    /// 更新检查网址
    /// </summary>
    public const string UpdateUrl = $"{ColorMCAPI.BaseWebUrl}colormc/update/{ColorMCCore.TopVersion}/";

    /// <summary>
    /// 获取frp列表
    /// </summary>
    /// <returns></returns>
    public static async Task<Dictionary<string, FrpDownloadObj>?> GetFrpList()
    {
        try
        {
            var data = await ColorMCAPI.Client.GetStringAsync(ColorMCAPI.BaseWebUrl + "frp/version.json");
            return JsonConvert.DeserializeObject<Dictionary<string, FrpDownloadObj>>(data);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.ColorMC.Error2"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取hdiff列表
    /// </summary>
    /// <returns></returns>
    public static async Task<Dictionary<string, HdiffDownloadObj>?> GetHdiffList()
    {
        try
        {
            var data = await ColorMCAPI.Client.GetStringAsync(ColorMCAPI.BaseWebUrl + "hdiff/version.json");
            return JsonConvert.DeserializeObject<Dictionary<string, HdiffDownloadObj>>(data);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.ColorMC.Error2"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取更新日志
    /// </summary>
    /// <returns>日志内容</returns>
    public static async Task<string?> GetNewLog()
    {
        try
        {
            return await ColorMCAPI.Client.GetStringAsync(ColorMCAPI.BaseWebUrl + "colormc/update/log");
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
    public static async Task<JObject> GetMainIndex()
    {
        var data = await CoreHttpClient.DownloadClient.GetStringAsync(ColorMCAPI.BaseUrl + "colormc/update/index.json");
        return JObject.Parse(data);
    }

    /// <summary>
    /// 获取文件修补
    /// </summary>
    /// <returns></returns>
    public static async Task<JObject> GetUpdateIndex()
    {
        var data = await CoreHttpClient.DownloadClient.GetStringAsync(UpdateUrl + "index.json");
        return JObject.Parse(data);
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

        var res = await ColorMCAPI.Client.SendAsync(requ);
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

            var res = await ColorMCAPI.Client.SendAsync(requ);
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

            var res = await ColorMCAPI.Client.SendAsync(requ);
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

            var res = await ColorMCAPI.Client.SendAsync(requ);
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

            var res = await ColorMCAPI.Client.SendAsync(requ);
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

            var res = await ColorMCAPI.Client.SendAsync(requ);
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

            var res = await ColorMCAPI.Client.SendAsync(requ);
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

            var res = await ColorMCAPI.Client.SendAsync(requ);
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

            var res = await ColorMCAPI.Client.SendAsync(requ);
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

            var res = await ColorMCAPI.Client.SendAsync(requ);
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

            var res = await ColorMCAPI.Client.SendAsync(requ);
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

            var res = await ColorMCAPI.Client.SendAsync(requ);
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

            var res = await ColorMCAPI.Client.SendAsync(requ);
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
    /// 获取Hdiff
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static (string?, FileItemObj?) BuildHdiffItem(string key, HdiffDownloadObj value)
    {
        string data1;
        string sha1;
        if (SystemInfo.Os == OsType.Windows)
        {
            if (SystemInfo.IsArm)
            {
                data1 = $"hdiffpatch_v{key}_bin_windows_arm64.zip";
                sha1 = value.windows_arm64;
            }
            else
            {
                data1 = $"hdiffpatch_v{key}_bin_windows64.zip";
                sha1 = value.windows_amd64;
            }
        }
        else if (SystemInfo.Os == OsType.Linux)
        {
            if (SystemInfo.IsArm)
            {
                data1 = $"hdiffpatch_v{key}_bin_linux_arm64.zip";
                sha1 = value.linux_arm64;
            }
            else
            {
                data1 = $"hdiffpatch_v{key}_bin_linux64.zip";
                sha1 = value.linux_amd64;
            }
        }
        else if (SystemInfo.Os == OsType.MacOS)
        {
            data1 = $"hdiffpatch_v{key}_bin_macos.zip";
            sha1 = value.macos;
        }
        else
        {
            return (null, null);
        }

        return (Path.Combine(ToolUtils.GetHdiffLocal(key), ToolUtils.GetHdiffName()), new()
        {
            Name = $"Hdiff {data1}",
            Local = ToolUtils.GetHdiffLocal(key, data1),
            Url = $"{ColorMCAPI.BaseWebUrl}hdiff/{key}/{data1}",
            Sha1 = sha1,
            Later = (stream) =>
            {
                ToolUtils.Unzip(stream, ToolUtils.GetHdiffLocal(key), data1);
            }
        });
    }

    /// <summary>
    /// 获取上游Frp
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static (string?, FileItemObj?) BuildFrpItem(string key, FrpDownloadObj value)
    {
        string data1;
        string sha1;
        if (SystemInfo.Os == OsType.Windows)
        {
            if (SystemInfo.IsArm)
            {
                data1 = $"frp_{key}_windows_arm64.zip";
                sha1 = value.windows_arm64;
            }
            else
            {
                data1 = $"frp_{key}_windows_amd64.zip";
                sha1 = value.windows_amd64;
            }
        }
        else if (SystemInfo.Os == OsType.Linux)
        {
            if (SystemInfo.IsArm)
            {
                data1 = $"frp_{key}_linux_arm64.tar.gz";
                sha1 = value.linux_arm64;
            }
            else
            {
                data1 = $"frp_{key}_linux_amd64.tar.gz";
                sha1 = value.linux_amd64;
            }
        }
        else if (SystemInfo.Os == OsType.MacOS)
        {
            if (SystemInfo.IsArm)
            {
                data1 = $"frp_{key}_darwin_arm64.tar.gz";
                sha1 = value.darwin_arm64;
            }
            else
            {
                data1 = $"frp_{key}_darwin_amd64.tar.gz";
                sha1 = value.darwin_amd64;
            }
        }
        else
        {
            return (null, null);
        }

        return (Path.Combine(FrpLaunchUtils.GetFrpLocal(key), FrpLaunchUtils.GetFrpcName()), new()
        {
            Name = $"Frp {data1}",
            Local = FrpLaunchUtils.GetFrpLocal(key, data1),
            Url = $"{ColorMCAPI.BaseWebUrl}frp/{key}/{data1}",
            Sha1 = sha1,
            Later = (stream) =>
            {
                ToolUtils.Unzip(stream, FrpLaunchUtils.GetFrpLocal(key), data1);
            }
        });
    }
}
