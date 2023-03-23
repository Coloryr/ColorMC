using Avalonia.Threading;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Launcher;

public record VersionObj
{
    public string Version { get; set; }
    public string Text { get; set; }
}

public class Updater
{
    private const string url = "https://colormc.coloryr.com/colormc/A16/";

    private readonly HttpClient Client;
    private readonly VersionObj version;
    private readonly string Local;
    public Updater()
    {
        Client = new();

        Local = $"{Program.VersionBaseDir}version.json";
        try
        {
            if (File.Exists(Local))
            {
                version = JsonConvert.DeserializeObject<VersionObj>(
                    File.ReadAllText(Local))!;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        if (version == null)
        {
            version = new()
            {
                Version = "0"
            };

            File.WriteAllText(Local, JsonConvert.SerializeObject(version));
        }
    }

    public void Check()
    {
        new Thread(async () =>
        {
            try
            {
                var data = await Client.GetStringAsync(url + "version.json");
                var obj = JsonConvert.DeserializeObject<VersionObj>(data)!;

                Dispatcher.UIThread.Post(async () =>
                {
                    if (obj == null)
                    {
                        Program.CheckFailCall();
                        return;
                    }

                    if (obj.Version != version.Version)
                    {
                        var res = await Program.HaveUpdate(obj.Text);
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

        Program.Quit();
    }

    public async Task<(bool?, string?)> CheckOne()
    {
        try
        {
            var data = await Client.GetStringAsync(url + "version.json");
            var obj = JsonConvert.DeserializeObject<VersionObj>(data)!;

            if (obj == null)
            {
                return (null, null);
            }

            return (obj.Version != version.Version, obj.Text);
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

        var data = await Client.GetStringAsync(url + "version.json");
        var obj = JsonConvert.DeserializeObject<VersionObj>(data)!;

        File.WriteAllText(Local, JsonConvert.SerializeObject(obj));
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
