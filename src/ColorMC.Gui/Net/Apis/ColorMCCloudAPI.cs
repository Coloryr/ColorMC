using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.ColorMC;
using ColorMC.Gui.Objs.Frp;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.Utils;

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
    public static async Task<Dictionary<string, FrpDownloadObj>?> GetFrpListAsync()
    {
        try
        {
            string url = ColorMCAPI.BaseWebUrl + "frp/version.json";
            using var data = await ColorMCAPI.GetStreamAsync(url);
            return JsonUtils.ToObj(data, JsonGuiType.DictionaryStringFrpDownloadObj);
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
    public static async Task<Dictionary<string, HdiffDownloadObj>?> GetHdiffListAsync()
    {
        try
        {
            string url = ColorMCAPI.BaseWebUrl + "hdiff/version.json";
            using var data = await ColorMCAPI.GetStreamAsync(url);
            return JsonUtils.ToObj(data, JsonGuiType.DictionaryStringHdiffDownloadObj);
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
    public static async Task<string?> GetNewLogAsync()
    {
        try
        {
            string url = ColorMCAPI.BaseWebUrl + "colormc/update/log";
            return await ColorMCAPI.GetStringAsync(url);
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
    public static async Task<JsonDocument?> GetMainIndexAsync()
    {
        string url = ColorMCAPI.BaseUrl + "colormc/update/index.json";
        using var stream = await ColorMCAPI.GetStreamAsync(url);
        if (stream == null)
        {
            return null;
        }
        return await JsonDocument.ParseAsync(stream);
    }

    /// <summary>
    /// 获取文件修补
    /// </summary>
    /// <returns></returns>
    public static async Task<JsonDocument?> GetUpdateIndexAsync()
    {
        string url = UpdateUrl + "index.json";
        using var stream = await ColorMCAPI.GetStreamAsync(url);
        if (stream == null)
        {
            return null;
        }
        return await JsonDocument.ParseAsync(stream);
    }

    /// <summary>
    /// 获取在线服务器
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns></returns>
    public static async Task<JsonDocument?> GetCloudServerAsync(string version)
    {
        try
        {
            string url = ColorMCAPI.BaseUrl + "frplist?version=" + version;
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("ColorMC", ColorMCCore.Version);
            using var data = await ColorMCAPI.SendAsync(req);
            using var stream = await data.Content.ReadAsStreamAsync();
            return await JsonDocument.ParseAsync(stream);
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
    public static async Task<bool> PutCloudServerAsync(string token, string ip, FrpShareModel model)
    {
        var obj = new PutCloudServerObj()
        {
            Token = token,
            IP = ip,
            Custom = new()
            {
                Version = model.Version,
                Loader = model.Loader,
                IsLoader = model.IsLoader,
                Text = model.Text
            }
        };
        var httpRequest = new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(ColorMCAPI.BaseUrl + "frp"),
            Content = new StringContent(JsonUtils.ToString(obj, JsonGuiType.PutCloudServerObj))
        };
        httpRequest.Headers.Add("ColorMC", ColorMCCore.Version);

        try
        {
            using var data = await ColorMCAPI.SendAsync(httpRequest);
            using var data1 = await data.Content.ReadAsStreamAsync();
            var obj1 = await JsonDocument.ParseAsync(data1);
            var json = obj1.RootElement;
            if (json.TryGetProperty("res", out var res) && res.ValueKind is JsonValueKind.Number)
            {
                return res.GetInt32() == 100;
            }
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.ColorMC.Error3"), e);
        }
        return false;
    }

    /// <summary>
    /// 检查云服务器是否在线
    /// </summary>
    public static async Task CheckAsync()
    {
        var requ = new HttpRequestMessage(HttpMethod.Post,
            new Uri(Server + "/check"));
        requ.Headers.Add("ColorMC", ColorMCCore.Version);
        requ.Headers.Add("serverkey", Serverkey);
        requ.Headers.Add("clientkey", Clientkey);

        using var res = await ColorMCAPI.SendAsync(requ);
        if (res.IsSuccessStatusCode)
        {
            using var data = await res.Content.ReadAsStreamAsync();
            var obj = await JsonDocument.ParseAsync(data);
            var json = obj.RootElement;
            if (json.TryGetProperty("res", out var res1) && res1.ValueKind is JsonValueKind.Number)
            {
                var value = res1.GetInt32();
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
            await GetStateAsync();
        }
    }

    /// <summary>
    /// 检查游戏实例是否启用了云同步
    /// </summary>
    public static async Task<CloudRes> HaveCloudAsync(GameSettingObj obj)
    {
        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(Server + "/haveuuid"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);
            requ.Headers.Add("uuid", obj.UUID);

            using var res = await ColorMCAPI.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                using var data = await res.Content.ReadAsStreamAsync();
                var obj1 = await JsonDocument.ParseAsync(data);
                var json = obj1.RootElement;
                if (json.TryGetProperty("res", out var res1) && res1.ValueKind is JsonValueKind.Number)
                {
                    var value = res1.GetInt32();
                    if (value == 100)
                    {
                        return new CloudRes()
                        {
                            State = true,
                            Data1 = json.GetProperty("have").GetBoolean()!,
                            Data2 = json.GetProperty("time").GetString()
                        };
                    }
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
            Data = App.Lang("GameCloudWindow.Error3")
        };
    }

    /// <summary>
    /// 实例开启云同步
    /// </summary>
    public static async Task<int> StartCloudAsync(GameSettingObj obj)
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

            using var res = await ColorMCAPI.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                using var data = await res.Content.ReadAsStreamAsync();
                var obj1 = await JsonDocument.ParseAsync(data);
                var json = obj1.RootElement;
                if (json.TryGetProperty("res", out var res1) && res1.ValueKind is JsonValueKind.Number)
                {
                    return res1.GetInt32();
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
    public static async Task<int> StopCloudAsync(GameSettingObj obj)
    {
        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(Server + "/stopuuid"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);
            requ.Headers.Add("uuid", obj.UUID);

            using var res = await ColorMCAPI.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                using var data = await res.Content.ReadAsStreamAsync();
                var obj1 = await JsonDocument.ParseAsync(data);
                var json = obj1.RootElement;
                if (json.TryGetProperty("res", out var res1) && res1.ValueKind is JsonValueKind.Number)
                {
                    return res1.GetInt32();
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
    public static async Task<CloudUploadRes> UploadConfigAsync(GameSettingObj obj, string path)
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

            using var res = await ColorMCAPI.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                using var data = await res.Content.ReadAsStreamAsync();
                var obj1 = await JsonDocument.ParseAsync(data);
                var json = obj1.RootElement;
                if (json.TryGetProperty("res", out var res1) && res1.ValueKind is JsonValueKind.Number)
                {
                    var value = res1.GetInt32();
                    if (value == 100)
                    {
                        return new CloudUploadRes()
                        {
                            State = true,
                            Data1 = value,
                            Data2 = json.GetProperty("time").GetString()
                        };
                    }
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
    public static async Task<int> DownloadConfigAsync(string uuid, string local)
    {
        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(Server + "/pullconfig"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);
            requ.Headers.Add("uuid", uuid);

            using var res = await ColorMCAPI.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                using var data = res.Content.ReadAsStream();
                await PathHelper.WriteBytesAsync(local, data);
                return 100;
            }
            else if (res.StatusCode == HttpStatusCode.BadRequest)
            {
                using var data = await res.Content.ReadAsStreamAsync();
                var obj1 = await JsonDocument.ParseAsync(data);
                var json = obj1.RootElement;
                if (json.TryGetProperty("res", out var res1) && res1.ValueKind is JsonValueKind.Number)
                {
                    return res1.GetInt32();
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
    public static async Task GetStateAsync()
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

            using var res = await ColorMCAPI.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                using var data = await res.Content.ReadAsStreamAsync();
                var obj1 = await JsonDocument.ParseAsync(data);
                var json = obj1.RootElement;
                if (json.TryGetProperty("res", out var res1) && res1.ValueKind is JsonValueKind.Number)
                {
                    var value = res1.GetInt32();
                    if (value == 100)
                    {
                        Info = string.Format(App.Lang("GameCloudUtils.Info1"),
                            json.GetProperty("use").GetInt64(),
                            json.GetProperty("size").GetInt64());
                    }
                    else
                    {
                        Info = App.Lang("GameCloudUtils.Error5");
                    }
                }

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
    public static async Task<List<CloundListObj>?> GetListAsync()
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

            using var res = await ColorMCAPI.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                using var data = await res.Content.ReadAsStreamAsync();
                var obj1 = await JsonDocument.ParseAsync(data);
                var json = obj1.RootElement;
                if (json.TryGetProperty("res", out var res1) && res1.ValueKind is JsonValueKind.Number)
                {
                    var value = res1.GetInt32();
                    if (value == 100)
                    {
                        json.GetProperty("list").Deserialize(JsonGuiType.ListCloundListObj);
                    }
                }
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
    public static async Task<CloudWorldRes> GetWorldListAsync(GameSettingObj game)
    {
        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
                new Uri(Server + "/getworldlist"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", Serverkey);
            requ.Headers.Add("clientkey", Clientkey);
            requ.Headers.Add("uuid", game.UUID);

            using var res = await ColorMCAPI.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                using var data = await res.Content.ReadAsStreamAsync();
                var obj1 = await JsonDocument.ParseAsync(data);
                var json = obj1.RootElement;
                if (json.TryGetProperty("res", out var res1) && res1.ValueKind is JsonValueKind.Number)
                {
                    var value = res1.GetInt32();
                    if (value == 100)
                    {
                        return new CloudWorldRes
                        {
                            State = true,
                            Worlds = json.GetProperty("list").Deserialize(JsonGuiType.ListCloudWorldObj)
                        };
                    }
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
            Data = App.Lang("GameCloudWindow.Error3")
        };
    }

    /// <summary>
    /// 上传游戏实例
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="world">世界储存</param>
    /// <param name="local">压缩包路径</param>
    /// <returns></returns>
    public static async Task<int> UploadWorldAsync(GameSettingObj game, SaveObj world, string local)
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

            using var res = await ColorMCAPI.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                using var data = await res.Content.ReadAsStreamAsync();
                var obj1 = await JsonDocument.ParseAsync(data);
                var json = obj1.RootElement;
                if (json.TryGetProperty("res", out var res1) && res1.ValueKind is JsonValueKind.Number)
                {
                    return res1.GetInt32();
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
    public static async Task<Dictionary<string, string>?> GetWorldFilesAsync(GameSettingObj game, SaveObj world)
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

            using var res = await ColorMCAPI.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                using var data = await res.Content.ReadAsStreamAsync();
                var obj1 = await JsonDocument.ParseAsync(data);
                var json = obj1.RootElement;
                if (json.TryGetProperty("res", out var res1) && res1.ValueKind is JsonValueKind.Number)
                {
                    var value = res1.GetInt32();
                    if (value == 100)
                    {
                        return json.GetProperty("list").Deserialize(JsonGuiType.DictionaryStringString);
                    }
                }
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
    public static async Task<int> DownloadWorldAsync(GameSettingObj game, CloudWorldObj world,
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
            requ.Content = new StringContent(JsonUtils.ToString(list, JsonGuiType.DictionaryStringString));

            using var res = await ColorMCAPI.SendAsync(requ);
            if (res.StatusCode == HttpStatusCode.BadRequest)
            {
                using var data = await res.Content.ReadAsStreamAsync();
                var obj1 = await JsonDocument.ParseAsync(data);
                var json = obj1.RootElement;
                if (json.TryGetProperty("res", out var res1) && res1.ValueKind is JsonValueKind.Number)
                {
                    return res1.GetInt32();
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
    public static async Task<int> DeleteWorldAsync(GameSettingObj game, string name)
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

            using var res = await ColorMCAPI.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                using var data = await res.Content.ReadAsStreamAsync();
                var obj1 = await JsonDocument.ParseAsync(data);
                var json = obj1.RootElement;
                if (json.TryGetProperty("res", out var res1) && res1.ValueKind is JsonValueKind.Number)
                {
                    return res1.GetInt32();
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

    ///// <summary>
    ///// 获取Hdiff
    ///// </summary>
    ///// <param name="key"></param>
    ///// <param name="value"></param>
    ///// <returns></returns>
    //public static (string?, FileItemObj?) BuildHdiffItem(string key, HdiffDownloadObj value)
    //{
    //    string data1;
    //    string sha1;
    //    if (SystemInfo.Os == OsType.Windows)
    //    {
    //        if (SystemInfo.IsArm)
    //        {
    //            data1 = $"hdiffpatch_v{key}_bin_windows_arm64.zip";
    //            sha1 = value.WindowsArm64;
    //        }
    //        else
    //        {
    //            data1 = $"hdiffpatch_v{key}_bin_windows64.zip";
    //            sha1 = value.WindowsAmd64;
    //        }
    //    }
    //    else if (SystemInfo.Os == OsType.Linux)
    //    {
    //        if (SystemInfo.IsArm)
    //        {
    //            data1 = $"hdiffpatch_v{key}_bin_linux_arm64.zip";
    //            sha1 = value.LinuxArm64;
    //        }
    //        else
    //        {
    //            data1 = $"hdiffpatch_v{key}_bin_linux64.zip";
    //            sha1 = value.LinuxAmd64;
    //        }
    //    }
    //    else if (SystemInfo.Os == OsType.MacOS)
    //    {
    //        data1 = $"hdiffpatch_v{key}_bin_macos.zip";
    //        sha1 = value.Macos;
    //    }
    //    else
    //    {
    //        return (null, null);
    //    }

    //    return (Path.Combine(ToolUtils.GetHdiffLocal(key), ToolUtils.GetHdiffName()), new()
    //    {
    //        Name = $"Hdiff {data1}",
    //        Local = ToolUtils.GetHdiffLocal(key, data1),
    //        Url = $"{ColorMCAPI.BaseWebUrl}hdiff/{key}/{data1}",
    //        Sha1 = sha1,
    //        Later = (stream) =>
    //        {
    //            ToolUtils.Unzip(stream, ToolUtils.GetHdiffLocal(key), data1);
    //        }
    //    });
    //}

    /// <summary>
    /// 获取上游Frp
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static FileItemRes BuildFrpItem(string key, FrpDownloadObj value)
    {
        string data1;
        string sha1;
        if (SystemInfo.Os == OsType.Windows)
        {
            if (SystemInfo.IsArm)
            {
                data1 = $"frp_{key}_windows_arm64.zip";
                sha1 = value.WindowsArm64;
            }
            else
            {
                data1 = $"frp_{key}_windows_amd64.zip";
                sha1 = value.WindowsAmd64;
            }
        }
        else if (SystemInfo.Os == OsType.Linux)
        {
            if (SystemInfo.IsArm)
            {
                data1 = $"frp_{key}_linux_arm64.tar.gz";
                sha1 = value.LinuxArm64;
            }
            else
            {
                data1 = $"frp_{key}_linux_amd64.tar.gz";
                sha1 = value.LinuxAmd64;
            }
        }
        else if (SystemInfo.Os == OsType.MacOS)
        {
            if (SystemInfo.IsArm)
            {
                data1 = $"frp_{key}_darwin_arm64.tar.gz";
                sha1 = value.DarwinArm64;
            }
            else
            {
                data1 = $"frp_{key}_darwin_amd64.tar.gz";
                sha1 = value.DarwinAmd64;
            }
        }
        else
        {
            return new FileItemRes();
        }

        return new FileItemRes
        {
            Path = Path.Combine(FrpLaunchUtils.GetFrpLocal(key, FrpLaunchUtils.GetFrpcName())),
            File = new()
            {
                Name = $"Frp {data1}",
                Local = FrpLaunchUtils.GetFrpLocal(key, data1),
                Url = $"{ColorMCAPI.BaseWebUrl}frp/{key}/{data1}",
                Sha1 = sha1,
                Later = (stream) =>
                {
                    ToolUtils.Unzip(stream, FrpLaunchUtils.GetFrpLocal(key), data1);
                }
            }
        };
    }
}
