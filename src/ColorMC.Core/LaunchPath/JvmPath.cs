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

    /// <summary>
    /// 安装Java
    /// </summary>
    /// <param name="file">文件名</param>
    /// <param name="name">Java名</param>
    /// <param name="sha256">验证</param>
    /// <param name="url">地址</param>
    /// <returns>结果</returns>
    public static async Task<(CoreRunState, string?)> Install(string file, string name, string sha256, string url)
    {
        Jvms.Remove(name);
        var res = await Download(file, sha256, url);
        if (!res.Item1)
        {
            return (CoreRunState.Error, LanguageHelper.GetName("Core.Java.Error1"));
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
            return (false, LanguageHelper.GetName("Core.Java.Error2"));

        return AddItem(name, Path.GetRelativePath(AppContext.BaseDirectory, java));
    }

    /// <summary>
    /// 添加Java
    /// </summary>
    /// <param name="name">名字</param>
    /// <param name="local">路径</param>
    /// <returns>结果</returns>
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

    /// <summary>
    /// 编辑Java项目
    /// </summary>
    /// <param name="old">旧名字</param>
    /// <param name="name">新名字</param>
    /// <param name="local">路径</param>
    /// <returns>结果</returns>
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

    /// <summary>
    /// 删除Java
    /// </summary>
    /// <param name="name">名字</param>
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

    /// <summary>
    /// 添加到列表
    /// </summary>
    /// <param name="list">列表</param>
    public static void AddList(List<JvmConfigObj> list)
    {
        Logs.Info(LanguageHelper.GetName("Core.Path.Jvm.Load"));
        Jvms.Clear();
        list.ForEach(a =>
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

    /// <summary>
    /// 获取Java信息
    /// </summary>
    /// <param name="name">名字</param>
    /// <returns>信息</returns>
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

    /// <summary>
    /// 获取Java信息
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns>信息</returns>
    public static JavaInfo? GetJavaInfo(string path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                Process p = new();
                p.StartInfo.FileName = path;
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
                    Path = path,
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

    /// <summary>
    /// 删除所有Java
    /// </summary>
    public static void RemoveAll()
    {
        Jvms.Clear();
        ConfigUtils.Config.JavaList.Clear();
        ConfigUtils.Save();
    }
}
