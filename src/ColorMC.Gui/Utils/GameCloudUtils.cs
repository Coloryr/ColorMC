using ColorMC.Core;
using ColorMC.Core.Net;
using ColorMC.Core.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Utils;

public static class GameCloudUtils
{
    private static string s_server;
    private static string s_serverkey;
    private static string s_clientkey;

    public static string Info { get; private set; }

    public static bool Connect { get; private set; }

    public static async Task Init()
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
}
