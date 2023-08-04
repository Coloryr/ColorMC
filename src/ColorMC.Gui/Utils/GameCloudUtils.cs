using ColorMC.Core;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Utils;

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

    public static void Init(string dir)
    {
        s_file = Path.GetFullPath(dir + "/" + Name);

        if (File.Exists(s_file))
        {
            try
            {
                s_datas = JsonConvert.DeserializeObject<Dictionary<string, CloudDataObj>>
                    (File.ReadAllText(s_file))!;
            }
            catch (Exception e)
            {
                Logs.Error("load error", e);
            }

            if (s_datas == null)
            {
                s_datas = new();
                Save();
            }
        }
    }

    public static void Save()
    {
        ConfigSave.AddItem(new()
        {
            Name = Name,
            Local = s_file,
            Obj = s_datas
        });
    }

    public static async Task StartConnect()
    {
        Connect = false;
        Info = App.GetLanguage("GameCloud.Error2");
        var config = GuiConfigUtils.Config.ServerKey;
        if (string.IsNullOrWhiteSpace(config))
        {
            return;
        }

        var data = Convert.FromBase64String(config);
        try
        {
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
            Logs.Error("server error", e);
        }
    }

    private static async Task Check()
    {
        var requ = new HttpRequestMessage(HttpMethod.Post, 
            new Uri(s_server + "/check"));
        requ.Headers.Add("ColorMC", ColorMCCore.Version);
        requ.Headers.Add("serverkey", s_serverkey);
        requ.Headers.Add("clientkey", s_clientkey);

        var res = await BaseClient.LoginClient.SendAsync(requ);
        if (res.IsSuccessStatusCode)
        {
            var data = await res.Content.ReadAsStringAsync();
            var obj = JObject.Parse(data);
            if (!obj.TryGetValue("res",out var res1) || (int)res1 != 100)
            {
                Info = App.GetLanguage("GameCloud.Error1");
                return;
            }
            Info = string.Format(App.GetLanguage("GameCloud.Info1"), obj["use"], obj["size"]);
            Connect = true;
        }
    }

    public static async Task<bool?> HaveCloud(string uuid)
    {
        try
        {
            if (!Connect)
            {
                return null;
            }

            var requ = new HttpRequestMessage(HttpMethod.Post,
            new Uri(s_server + "/haveuuid"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", s_serverkey);
            requ.Headers.Add("clientkey", s_clientkey);
            requ.Headers.Add("uuid", uuid);

            var res = await BaseClient.LoginClient.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(data);
                if (obj.TryGetValue("res", out var res1))
                {
                    return (bool?)res1;
                }
            }

            return null;
        }
        catch (Exception e)
        {
            Logs.Error("cloud error", e);
            App.ShowError("cloud error", e);
            return null;
        }
    }

    public static async Task<AddSaveState?> StartCloud(string uuid)
    {
        if (!Connect)
        {
            return null;
        }

        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
            new Uri(s_server + "/startuuid"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", s_serverkey);
            requ.Headers.Add("clientkey", s_clientkey);
            requ.Headers.Add("uuid", uuid);

            var res = await BaseClient.LoginClient.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(data);
                if (obj.TryGetValue("res", out var res1))
                {
                    return (AddSaveState?)(int)res1;
                }
            }

            return null;
        }
        catch (Exception e)
        {
            Logs.Error("cloud error", e);
            App.ShowError("cloud error", e);
            return null;
        }
    }

    public static async Task<bool?> StopCloud(string uuid)
    {
        if (!Connect)
        {
            return null;
        }

        try
        {
            var requ = new HttpRequestMessage(HttpMethod.Post,
            new Uri(s_server + "/stopuuid"));
            requ.Headers.Add("ColorMC", ColorMCCore.Version);
            requ.Headers.Add("serverkey", s_serverkey);
            requ.Headers.Add("clientkey", s_clientkey);
            requ.Headers.Add("uuid", uuid);

            var res = await BaseClient.LoginClient.SendAsync(requ);
            if (res.IsSuccessStatusCode)
            {
                var data = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(data);
                if (obj.TryGetValue("res", out var res1))
                {
                    return (bool?)res1;
                }
            }

            return null;
        }
        catch (Exception e)
        {
            Logs.Error("cloud error", e);
            App.ShowError("cloud error", e);
            return null;
        }
    }

    public static CloudDataObj GetCloudData(GameSettingObj obj)
    {
        
    }
}
