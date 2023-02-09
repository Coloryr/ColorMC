using ColorMC.Core.Game;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Diagnostics;

namespace ColorMC.Core.LaunchPath;

public static class JvmPath
{
    public const string Unknow = "unknow";
    public const string Name1 = "./java";
    public static Dictionary<string, JavaInfo> Jvms { get; } = new();

    public static string BaseDir;

    public static void Init() 
    {
        BaseDir = Name1;
        Directory.CreateDirectory(BaseDir);
    }

    public static async Task<(CoreRunState, string?)> Install(string file, string name, string sha256, string url)
    {
        Jvms.Remove(name);
        var res = await Download(file, sha256, url);
        if (!res.Item1)
        {
            return (CoreRunState.Error, "Java下载失败");
        }
        res = await UnzipJava(name, res.Item2!);
        if (!res.Item1)
        {
            return (CoreRunState.Error, res.Item2);
        }

        return (CoreRunState.Init, null);
    }

    private static async Task<(bool, string?)> Download(string name,  string sha256, string url)
    {
        var item = new DownloadItem()
        {
            Name = name,
            SHA256 = sha256,
            Local = DownloadManager.DownloadDir + "/" + name,
            Url = url
        };
        
        var res = await DownloadManager.Start(new List<DownloadItem>()
        {
            item
        });

        if (res == false)
        {
            return (false, null);
        }

        return (true, item.Local);
    }


    private static string? Find(string path)
    {
        switch (SystemInfo.Os)
        {
            case OsType.Windows:
                return PathC.GetFile(path, "javaw.exe");
            case OsType.Linux:
                return PathC.GetFile(path, "java");
            case OsType.MacOS:
                return PathC.GetFile(path, "java");
        }

        return null;
    }

    private static async Task<(bool, string?)> UnzipJava(string name, string file)
    {
        string path = BaseDir + "/" + name;
        Directory.CreateDirectory(path);

        await Task.Run(() => ZipFloClass.Unzip(path, file));

        var java = Find(path);
        if (java == null)
            return (false, "没有找到Java");

        return AddItem(name, Path.GetRelativePath(AppContext.BaseDirectory, java));
    }

    /// <summary>
    /// 添加java项目
    /// </summary>
    /// <param name="name"></param>
    /// <param name="local"></param>
    /// <returns></returns>
    public static (bool Res, string Msg) AddItem(string name, string local)
    {
        Jvms.Remove(name);
        var info = GetJavaInfo(local);
        if (info != null)
        {
            Jvms.Add(name, info);
            ConfigUtils.Config.JavaList.Add(new JvmConfigObj
            {
                Name = name,
                Local = local
            });
            ConfigUtils.Save();
            return (true, name);
        }

        return (false, LanguageHelper.GetName("Core.Path.Jvm.Error2"));
    }

    public static (bool Res, string Msg) EditItem(string old, string name, string local)
    {
        if (old != name)
        {
            if (!Jvms.ContainsKey(old))
            {
                return (false, LanguageHelper.GetName("Core.Path.Jvm.Error3"));
            }
            if (Jvms.ContainsKey(name))
            {
                return (false, LanguageHelper.GetName("Core.Path.Jvm.Error4"));
            }
        }
        var info = GetJavaInfo(local);
        if (info != null)
        {
            Jvms[name] = info;
            var item = ConfigUtils.Config.JavaList.Where(a => a.Name == old).First();
            item.Name = name;
            item.Local = local;
            ConfigUtils.Save();
            return (true, name);
        }

        return (false, LanguageHelper.GetName("Core.Path.Jvm.Error5"));
    }

    public static void Remove(string name)
    {
        if (!Jvms.ContainsKey(name))
        {
            return;
        }

        Jvms.Remove(name);
        var item = ConfigUtils.Config.JavaList.Where(a => a.Name == name).First();
        ConfigUtils.Config.JavaList.Remove(item);
        ConfigUtils.Save();
    }

    public static void AddList(List<JvmConfigObj> objs)
    {
        Logs.Info(LanguageHelper.GetName("Core.Path.Jvm.Load"));
        Jvms.Clear();
        objs.ForEach(a =>
        {
            var info = GetJavaInfo(a.Local);
            if (info != null)
            {
                Logs.Info(string.Format(LanguageHelper.GetName("Core.Path.Jvm.Info"),
                    info.Path, info.Version));
                Jvms.Add(a.Name, info);
            }
            else
            {
                Jvms.Add(a.Name, new JavaInfo()
                { 
                    Path = a.Local,
                    Type = Unknow,
                    Version = Unknow
                });
            }
        });
    }

    public static JavaInfo? GetInfo(string? name)
    {
        if (name == null)
            return null;

        if (Jvms.TryGetValue(name, out var info))
        {
            return info;
        }
        else
        {
            return null;
        }
    }

    public static JavaInfo? GetJavaInfo(string javaPath)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(javaPath) && File.Exists(javaPath))
            {
                Process p = new();
                p.StartInfo.FileName = javaPath;
                p.StartInfo.Arguments = "-version";
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                string result = p.StandardError.ReadToEnd();
                string[] lines = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                string[] firstL = lines[0].Split(' ');
                string type = firstL[0];
                string version = firstL[2].Trim('\"');
                bool is64 = result.Contains("64-Bit");
                ArchEnum arch = is64 ? ArchEnum.x64 : ArchEnum.x32;
                JavaInfo info = new()
                {
                    Path = javaPath,
                    Version = version,
                    Arch = arch,
                    Type = type
                };
                p.Dispose();
                return info;
            }
            else
            {
                return null;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static void RemoveAll()
    {
        Jvms.Clear();
        ConfigUtils.Config.JavaList.Clear();
        ConfigUtils.Save();
    }
}
