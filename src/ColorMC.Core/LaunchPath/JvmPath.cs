using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.LaunchPath;

/// <summary>
/// Java路径
/// </summary>
public static class JvmPath
{
    public const string Name1 = "java";
    public static Dictionary<string, JavaInfo> Jvms { get; } = [];

    public static string BaseDir { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        if (SystemInfo.Os == OsType.Android)
        {
            BaseDir = ColorMCCore.PhoneGetDataDir();
        }
        else
        {
            BaseDir = dir;
        }

        if (!BaseDir.EndsWith('/') && !BaseDir.EndsWith('\\'))
        {
            BaseDir += "/";
        }

        Directory.CreateDirectory(BaseDir + Name1);

        AddList(ConfigUtils.Config.JavaList);
    }

    /// <summary>
    /// 获取路径
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static string GetPath(this JavaInfo info)
    {
        if (info.Path.StartsWith(Name1))
        {
            return Path.GetFullPath(BaseDir + info.Path);
        }

        return info.Path;
    }

    /// <summary>
    /// 安装Java
    /// </summary>
    /// <param name="file">文件名</param>
    /// <param name="name">Java名</param>
    /// <param name="sha256">验证</param>
    /// <param name="url">地址</param>
    /// <returns>结果</returns>
    public static async Task<(CoreRunState Res, string? Message)>
        InstallAsync(string file, string name, string sha256, string url,
        ColorMCCore.ZipUpdate zip, ColorMCCore.JavaUnzip unzip)
    {
        try
        {
            Remove(name);
            var res = await DownloadAsync(file, sha256, url);
            if (!res.Res)
            {
                return (CoreRunState.Error, LanguageHelper.Get("Core.Jvm.Error5"));
            }
            unzip();
            res = await UnzipJavaAsync(name, res.Local!, zip);
            if (!res.Res)
            {
                return (CoreRunState.Error, res.Local);
            }
        }
        catch (Exception e)
        {
            string text = LanguageHelper.Get("Core.Jvm.Error7");
            Logs.Error(text, e);
            return (CoreRunState.Error, text);
        }

        return (CoreRunState.Init, null);
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="name">名字</param>
    /// <param name="sha256">校验</param>
    /// <param name="url">网址</param>
    /// <returns>结果</returns>
    private static async Task<(bool Res, string? Local)> DownloadAsync(string name, string sha256, string url)
    {
        var item = new DownloadItemObj()
        {
            Name = name,
            SHA256 = sha256,
            Local = DownloadManager.DownloadDir + "/" + name,
            Url = url
        };

        var res = await DownloadManager.StartAsync([item]);

        if (res == false)
        {
            return (false, null);
        }

        return (true, item.Local);
    }

    /// <summary>
    /// 查找文件
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns>结果</returns>
    private static string? Find(string path)
    {
        return SystemInfo.Os switch
        {
            OsType.Windows => PathHelper.GetFile(path, "javaw.exe"),
            OsType.Linux => PathHelper.GetFile(path, "java"),
            OsType.Android => PathHelper.GetFile(path, "java"),
            OsType.MacOS => PathHelper.GetFile(path, "java"),
            _ => null,
        };
    }

    /// <summary>
    /// 解压
    /// </summary>
    /// <param name="name">名字</param>
    /// <param name="file">文件</param>
    /// <returns></returns>
    public static async Task<(bool, string?)> UnzipJavaAsync(string name, string file,
        ColorMCCore.ZipUpdate zip)
    {
        string path = BaseDir + Name1 + "/" + name;
        Directory.CreateDirectory(path);
        var stream = PathHelper.OpenRead(file);
        if (stream == null)
        {
            return (false, string.Format(LanguageHelper.Get("Core.Jvm.Error11"), file));
        }

        (bool, Exception?) res;
        if (SystemInfo.Os == OsType.Android)
        {
            res = await Task.Run(() =>
            {
                try
                {
                    ColorMCCore.PhoneJvmInstall(stream, path, zip);
                    return (true, null!);
                }
                catch (Exception e)
                {
                    return (false, e);
                }
            });
        }
        else
        {
            res = await Task.Run(async () =>
            {
                try
                {
                    await new ZipUtils(ZipUpdate: zip).UnzipAsync(path, file, stream);
                    return (true, null!);
                }
                catch (Exception e)
                {
                    return (false, e);
                }
            });

            stream.Close();
        }

        if (!res.Item1)
        {
            string temp = LanguageHelper.Get("Core.Jvm.Error12");
            Logs.Error(temp, res.Item2);
            return (false, temp);
        }

        var java = Find(path);
        if (java == null)
        {
            return (false, LanguageHelper.Get("Core.Jvm.Error6"));
        }
        else
        {
            if (java.Contains("jre"))
            {
                var info = new FileInfo(java);
                var tpath = info.Directory!.Parent!.Parent!.FullName + "/bin/" + info.Name;
                if (File.Exists(tpath))
                {
                    java = tpath;
                }
            }
            Logs.Info(string.Format(LanguageHelper.Get("Core.Jvm.Info3"), java));
        }

        if (SystemInfo.Os == OsType.Linux || SystemInfo.Os == OsType.MacOS
            || SystemInfo.Os == OsType.Android)
        {
            JavaHelper.Per(java);
        }

        return AddItem(name, java);
    }

    /// <summary>
    /// 添加Java
    /// </summary>
    /// <param name="name">名字</param>
    /// <param name="local">路径</param>
    /// <returns>结果</returns>
    public static (bool Res, string Msg) AddItem(string name, string local)
    {
        if (local.StartsWith(BaseDir))
        {
            local = local[BaseDir.Length..];
        }

        Logs.Info(string.Format(LanguageHelper.Get("Core.Jvm.Info5"), local));

        Remove(name);
        var path = local;
        if (path.StartsWith(Name1))
        {
            path = Path.GetFullPath(BaseDir + path);
        }

        var info = JavaHelper.GetJavaInfo(path);
        if (info != null)
        {
            Jvms.Add(name, info);
            ConfigUtils.Config.JavaList.Add(new()
            {
                Name = name,
                Local = local
            });
            ConfigUtils.Save();
            return (true, name);
        }
        else
        {
            Logs.Info(LanguageHelper.Get("Core.Jvm.Error8"));
        }

        return (false, LanguageHelper.Get("Core.Jvm.Error1"));
    }

    /// <summary>
    /// 删除Java
    /// </summary>
    /// <param name="name">名字</param>
    public static void Remove(string name)
    {
        Jvms.Remove(name);
        var list = ConfigUtils.Config.JavaList.Where(a => a.Name == name).ToList();
        foreach (var item in list)
        {
            if (ConfigUtils.Config.JavaList.Remove(item))
            {
                ConfigUtils.Save();
            }
        }
    }

    /// <summary>
    /// 添加到列表
    /// </summary>
    /// <param name="list">列表</param>
    public static void AddList(List<JvmConfigObj> list)
    {
        Logs.Info(LanguageHelper.Get("Core.Jvm.Info1"));
        Task.Run(() =>
        {
            Jvms.Clear();
            list.ForEach(a =>
            {
                var path = a.Local;
                var local = path;
                if (path.StartsWith(Name1))
                {
                    local = Path.GetFullPath(BaseDir + path);
                }

                var info = JavaHelper.GetJavaInfo(local);
                Jvms.Remove(a.Name);
                if (info != null)
                {
                    Logs.Info(string.Format(LanguageHelper.Get("Core.Jvm.Info2"),
                        info.Path, info.Version));
                    info.Name = a.Name;
                    Jvms.Add(a.Name, info);
                }
                else
                {
                    Jvms.Add(a.Name, new()
                    {
                        Name = a.Name,
                        Path = a.Local,
                        Type = LanguageHelper.Get("Core.Jvm.Info7"),
                        Version = LanguageHelper.Get("Core.Jvm.Info6"),
                        MajorVersion = -1,
                        Arch = ArchEnum.unknow
                    });
                }
            });
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
    /// 删除所有Java
    /// </summary>
    public static void RemoveAll()
    {
        Jvms.Clear();
        ConfigUtils.Config.JavaList.Clear();
        ConfigUtils.Save();
    }

    /// <summary>
    /// 找到合适的Java
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>Java信息</returns>
    public static JavaInfo? FindJava(int jv)
    {
        var list = Jvms.Where(a => a.Value.MajorVersion == jv)
            .Select(a => a.Value);

        if (!list.Any())
        {
            if (jv > 8)
            {
                list = Jvms.Where(a => a.Value.MajorVersion >= jv)
                .Select(a => a.Value);
                if (!list.Any())
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        var find = list.Where(a => a.Arch == SystemInfo.SystemArch);
        int max;
        if (find.Any())
        {
            max = find.Max(a => a.MajorVersion);
            return find.Where(x => x.MajorVersion == max).FirstOrDefault();
        }

        max = list.Max(a => a.MajorVersion);
        return list.Where(x => x.MajorVersion == max).FirstOrDefault();
    }
}