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
    /// <summary>
    /// Jvm列表
    /// </summary>
    public static Dictionary<string, JavaInfo> Jvms { get; } = [];

    /// <summary>
    /// 下载的Java位置
    /// </summary>
    public static string JavaDir { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        JavaDir = Path.Combine(ColorMCCore.BaseDir, Names.NameJavaDir);

        Directory.CreateDirectory(JavaDir);

        if (ConfigUtils.Config.JavaList is { } list)
        {
            AddList(list);
        }

        if (Jvms.Count == 0)
        {
            Task.Run(() =>
            {
                var list1 = JavaHelper.FindJava().Result;
                list1?.ForEach(item => AddItem(item.Type + "_" + item.Version, item.Path));
            });
        }
    }

    /// <summary>
    /// 获取路径
    /// </summary>
    /// <param name="info">Java信息</param>
    /// <returns>路径</returns>
    public static string GetPath(this JavaInfo info)
    {
        if (info.Path.StartsWith(Names.NameJavaDir))
        {
            return Path.Combine(ColorMCCore.BaseDir, info.Path);
        }

        return info.Path;
    }

    /// <summary>
    /// 安装Java
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>结果</returns>
    public static async Task<StringRes> InstallAsync(InstallJvmArg arg)
    {
        try
        {
            Remove(arg.Name);
            var res = await DownloadAsync(arg.File, arg.Sha256, arg.Url);
            if (!res.State)
            {
                return new StringRes { Data = LanguageHelper.Get("Core.Jvm.Error5") };
            }
            arg.Unzip?.Invoke();
            res = await UnzipJavaAsync(new UnzipArg
            {
                Name = arg.Name,
                File = res.Data!,
                Zip = arg.Zip
            });
            if (!res.State)
            {
                return new StringRes { Data = res.Data };
            }
        }
        catch (Exception e)
        {
            string text = LanguageHelper.Get("Core.Jvm.Error7");
            Logs.Error(text, e);
            return new StringRes { Data = text };
        }

        return new StringRes { State = true };
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="name">名字</param>
    /// <param name="sha256">校验</param>
    /// <param name="url">网址</param>
    /// <returns>结果</returns>
    private static async Task<StringRes> DownloadAsync(string name, string sha256, string url)
    {
        var item = new FileItemObj()
        {
            Name = name,
            Sha256 = sha256,
            Local = DownloadManager.DownloadDir + "/" + name,
            Url = url
        };

        var res = await DownloadManager.StartAsync([item]);

        if (res == false)
        {
            return new();
        }

        return new StringRes { State = true, Data = item.Local };
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
            OsType.Windows => PathHelper.GetFile(path, Names.NameJavawFile),
            OsType.Linux or OsType.MacOS => PathHelper.GetFile(path, Names.NameJavaFile),
            _ => null,
        };
    }

    /// <summary>
    /// 解压
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>结果</returns>
    public static async Task<StringRes> UnzipJavaAsync(UnzipArg arg)
    {
        string path = Path.Combine(JavaDir, arg.Name);
        Directory.CreateDirectory(path);
        var stream = PathHelper.OpenRead(arg.File);
        if (stream == null)
        {
            return new StringRes { Data = string.Format(LanguageHelper.Get("Core.Jvm.Error11"), arg.File) };
        }

        var res = await Task.Run(async () =>
        {
            try
            {
                await new ZipProcess(zipUpdate: arg.Zip).UnzipAsync(path, arg.File, stream);
                return (true, null!);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        });

        stream.Close();
        if (!res.Item1)
        {
            string temp = LanguageHelper.Get("Core.Jvm.Error12");
            Logs.Error(temp, res.e);
            return new StringRes { Data = temp };
        }

        var java = Find(path);
        if (java == null)
        {
            return new StringRes { Data = LanguageHelper.Get("Core.Jvm.Error6") };
        }
        else
        {
            //选择jdk的java
            if (java.Contains("jre"))
            {
                var info = new FileInfo(java);
                var tpath = Path.Combine(info.Directory!.Parent!.Parent!.FullName, "bin", info.Name);
                if (File.Exists(tpath))
                {
                    java = tpath;
                }
            }
            Logs.Info(string.Format(LanguageHelper.Get("Core.Jvm.Info3"), java));
        }

        if (SystemInfo.Os is OsType.Linux or OsType.MacOS)
        {
            PathHelper.PerJavaChmod(java);
        }
        return AddItem(arg.Name, java);
    }

    /// <summary>
    /// 添加Java
    /// </summary>
    /// <param name="name">名字</param>
    /// <param name="local">路径</param>
    /// <returns>结果</returns>
    public static StringRes AddItem(string name, string local)
    {
        var basedir = ColorMCCore.BaseDir;
        if (local.StartsWith(basedir))
        {
            local = local[basedir.Length..];
        }

        Logs.Info(string.Format(LanguageHelper.Get("Core.Jvm.Info5"), local));

        //删除旧的
        Remove(name);
        var path = local;
        if (path.StartsWith(Names.NameJavaDir))
        {
            path = Path.GetFullPath(basedir + path);
        }

        var info = JavaHelper.GetJavaInfo(path);
        if (info != null)
        {
            Jvms.Add(name, info);
            ConfigUtils.Config.JavaList?.Add(new JvmConfigObj
            {
                Name = name,
                Local = local
            });
            ConfigUtils.Save();
            return new StringRes { State = true, Data = name };
        }
        else
        {
            Logs.Info(LanguageHelper.Get("Core.Jvm.Error8"));
        }

        return new StringRes { Data = LanguageHelper.Get("Core.Jvm.Error1") };
    }

    /// <summary>
    /// 删除Java
    /// </summary>
    /// <param name="name">名字</param>
    public static void Remove(string name)
    {
        Jvms.Remove(name);
        var item = ConfigUtils.Config.JavaList?.FirstOrDefault(a => a.Name == name);
        if (item != null && ConfigUtils.Config.JavaList?.Remove(item) == true)
        {
            ConfigUtils.Save();
        }
    }

    /// <summary>
    /// 添加到列表
    /// </summary>
    /// <param name="list">列表</param>
    private static void AddList(List<JvmConfigObj> list)
    {
        var basedir = ColorMCCore.BaseDir;

        Logs.Info(LanguageHelper.Get("Core.Jvm.Info1"));
        Task.Run(() =>
        {
            Jvms.Clear();
            list.ToList().ForEach(a =>
            {
                var path = a.Local;
                var local = path;
                if (path.StartsWith(Names.NameJavaDir))
                {
                    local = Path.GetFullPath(basedir + path);
                }

                //启动java获取信息
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
    /// <returns>Java信息</returns>
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
    /// <param name="jv">主版本</param>
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