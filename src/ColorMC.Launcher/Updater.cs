using Avalonia.Threading;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Launcher;

public class Updater
{
    private const string url = "https://colormc.coloryr.com/colormc/A16/";

    private readonly HttpClient Client;
    private string sha1;
    private string sha2;
    public Updater()
    {
        Client = new();

        if (File.Exists($"{Program.BaseDir}ColorMC.Core.dll"))
        {
            using var file = File.OpenRead($"{Program.BaseDir}ColorMC.Core.dll");
            sha1 = GenSha1(file);
        }
        else
        {
            sha1 = "";
        }
        if (File.Exists($"{Program.BaseDir}ColorMC.Gui.dll"))
        {
            using var file = File.OpenRead($"{Program.BaseDir}ColorMC.Gui.dll");
            sha2 = GenSha1(file);
        }
        else
        {
            sha2 = "";
        }
    }

    public static string GenSha1(Stream stream)
    {
        SHA1 sha1 = SHA1.Create();
        StringBuilder EnText = new();
        foreach (byte iByte in sha1.ComputeHash(stream))
        {
            EnText.AppendFormat("{0:x2}", iByte);
        }
        return EnText.ToString().ToLower();
    }

    public void Check()
    {
        new Thread(async () =>
        {
            try
            {
                var data = await Client.GetStringAsync(url + "sha1.json");
                var obj = JObject.Parse(data);

                Dispatcher.UIThread.Post(async () =>
                {
                    if (obj == null)
                    {
                        Program.CheckFailCall();
                        //ColorMC.Gui.ColorMCGui.CheckUpdateFail();
                        return;
                    }

                    if ((obj.ContainsKey("core.dll") && obj["core.dll"]?.ToString() != sha1) ||
                    (obj.ContainsKey("gui.dll") && obj["gui.dll"]?.ToString() != sha2))
                    {
                        string text;
                        obj.TryGetValue("text", out var data1);
                        text = data1?.ToString() ?? "没有更新消息";
                        var res = await Program.HaveUpdate(text);
                        //var res = await ColorMC.Gui.ColorMCGui.HaveUpdate(text);
                        if (res)
                            return;

                        StartUpdate();
                    }
                });
            }
            catch
            {

            }
        }).Start();
    }

    public void StartUpdate()
    {
        File.Delete($"{Program.BaseDir}ColorMC.Core.dll");
        File.Delete($"{Program.BaseDir}ColorMC.Core.pdb");
        File.Delete($"{Program.BaseDir}ColorMC.Gui.dll");
        File.Delete($"{Program.BaseDir}ColorMC.Gui.pdb");

        new Mutex(true, "ColorMC-Launcher");

        Program.Launch();

        //Program.Quit();
        ColorMC.Gui.ColorMCGui.Quit();
    }

    public async Task<(bool?, string?)> CheckOne()
    {
        try
        {
            var data = await Client.GetStringAsync(url + "sha1.json");
            var obj = JObject.Parse(data);

            if (obj == null)
            {
                return (null, null);
            }

            string text;
            obj.TryGetValue("text", out var data1);
            text = data1?.ToString() ?? "没有更新消息";

            return ((obj.ContainsKey("core.dll") && obj["core.dll"]?.ToString() != sha1) ||
                    (obj.ContainsKey("gui.dll") && obj["gui.dll"]?.ToString() != sha2), text);
        }
        catch
        {

        }

        return (null, null);
    }

    public async Task Download(Action<int> state)
    {
        state.Invoke(0);
        await Download("ColorMC.Core.dll");
        state.Invoke(1);
        await Download("ColorMC.Core.pdb");
        state.Invoke(2);
        await Download("ColorMC.Gui.dll");
        state.Invoke(3);
        await Download("ColorMC.Gui.pdb");
        state.Invoke(4);
    }

    private async Task Download(string name)
    {
        var res = await Client.GetAsync(url + name,
            HttpCompletionOption.ResponseHeadersRead);
        if (res.IsSuccessStatusCode)
        {
            using var stream = res.Content.ReadAsStream();
            using var stream1 = File.Create($"{Program.BaseDir}{name}.temp");
            await stream.CopyToAsync(stream1);
        }

        if (File.Exists($"{Program.BaseDir}{name}"))
            File.Delete($"{Program.BaseDir}{name}");

        File.Move($"{Program.BaseDir}{name}.temp", $"{Program.BaseDir}{name}");
    }
}
