using Avalonia.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Launcher;

public record VersionObj
{ 
    public string Version { get; set; }
}

public class Updater
{
    private const string url = "http://127.0.0.1:80/colormc/";

    private HttpClient Client;
    private VersionObj version;
    private VersionObj New;
    public Updater()
    {
        Client = new();

        try
        {
            if (File.Exists("version.json"))
            {
                version = JsonConvert.DeserializeObject<VersionObj>(
                    File.ReadAllText("version.json"))!;
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

            File.WriteAllText("version.json", JsonConvert.SerializeObject(version));
        }
    }

    public void Check()
    {
        new Thread(async () =>
        {
            try
            {
                var data = await Client.GetStringAsync(url + "version.json");
                New = JsonConvert.DeserializeObject<VersionObj>(data)!;

                Program.semaphore.WaitOne();

                Dispatcher.UIThread.Post(async () =>
                {
                    if (New == null)
                    {
                        Program.CheckFailCall();
                        return;
                    }

                    if (New.Version != version.Version)
                    {
                        var res = await Program.HaveUpdate();
                        if (!res)
                            return;

                        File.Delete("ColorMC.Gui.dll");

                        Process process = Process.Start("ColorMC.Launcher.exe");

                        Program.Quit();
                    }
                });
            }
            catch
            {
                Program.CheckFailCall();
                return;
            }
        }).Start();
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
        New = JsonConvert.DeserializeObject<VersionObj>(data)!;

        File.WriteAllText("version.json", JsonConvert.SerializeObject(New));
    }

    private async Task Download(string name)
    {
        var res = await Client.GetAsync(url + name,
            HttpCompletionOption.ResponseHeadersRead);
        if (res.IsSuccessStatusCode)
        {
            using var stream = res.Content.ReadAsStream();
            using var stream1 = File.Create(name);
            await stream.CopyToAsync(stream1);
        }
    }
}
