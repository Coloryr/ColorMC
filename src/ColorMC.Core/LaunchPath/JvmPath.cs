using ColorMC.Core.Helpers;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Diagnostics;

namespace ColorMC.Core.LaunchPath;

public static class JvmPath
{
    public const string Unknow = "unknow";
    public const string Name1 = "java";
    public static Dictionary<string, JavaInfo> Jvms { get; } = new();

    public static string BaseDir { get; private set; } = "";

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        BaseDir = dir;
        Directory.CreateDirectory(dir + Name1);
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
    public static async Task<(CoreRunState, string?)> Install(string file, string name, string sha256, string url)
    {
        try
        {
            Jvms.Remove(name);
            var res = await Download(file, sha256, url);
            if (!res.Item1)
            {
                return (CoreRunState.Error, LanguageHelper.GetName("Core.Jvm.Error5"));
            }
            ColorMCCore.JavaUnzip?.Invoke();
            res = await UnzipJava(name, res.Item2!);
            if (!res.Item1)
            {
                return (CoreRunState.Error, res.Item2);
            }
        }
        catch (Exception e)
        {
            string text = LanguageHelper.GetName("Core.Jvm.Error7");
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
    private static async Task<(bool, string?)> Download(string name, string sha256, string url)
    {
        var item = new DownloadItemObj()
        {
            Name = name,
            SHA256 = sha256,
            Local = DownloadManager.DownloadDir + "/" + name,
            Url = url
        };

        var res = await DownloadManager.Start(new List<DownloadItemObj>()
        {
            item
        });

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
            OsType.Windows => PathC.GetFile(path, "javaw.exe"),
            OsType.Linux => PathC.GetFile(path, "java"),
            OsType.Android => PathC.GetFile(path, "java"),
            OsType.MacOS => PathC.GetFile(path, "java"),
            _ => null,
        };
    }

    /// <summary>
    /// 解压
    /// </summary>
    /// <param name="name">名字</param>
    /// <param name="file">文件</param>
    /// <returns></returns>
    private static async Task<(bool, string?)> UnzipJava(string name, string file)
    {
        string path = BaseDir + Name1 + "/" + name;
        Directory.CreateDirectory(path);

        await Task.Run(() => ZipUtils.Unzip(path, file));

        var java = Find(path);
        if (java == null)
            return (false, LanguageHelper.GetName("Core.Jvm.Error6"));
        else
        {
            Logs.Info(string.Format(LanguageHelper.GetName("Core.Jvm.Info3"), java));
        }

        if (SystemInfo.Os == OsType.Linux || SystemInfo.Os == OsType.MacOS)
        {
            Per(java);
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

        Logs.Info(string.Format(LanguageHelper.GetName("Core.Jvm.Info5"), local));

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
        else
        {
            Logs.Info(LanguageHelper.GetName("Core.Jvm.Error8"));
        }

        return (false, LanguageHelper.GetName("Core.Jvm.Error1"));
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
                return (false, LanguageHelper.GetName("Core.Jvm.Error2"));
            }
            if (Jvms.ContainsKey(name))
            {
                return (false, LanguageHelper.GetName("Core.Jvm.Error3"));
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

        return (false, LanguageHelper.GetName("Core.Jvm.Error4"));
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
        Logs.Info(LanguageHelper.GetName("Core.Jvm.Info1"));
        Jvms.Clear();
        list.ForEach(a =>
        {
            var path = a.Local;
            var info = GetJavaInfo(path);
            Jvms.Remove(a.Name);
            if (info != null)
            {
                Logs.Info(string.Format(LanguageHelper.GetName("Core.Jvm.Info2"),
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
    /// 提升权限
    /// </summary>
    /// <param name="path">文件</param>
    private static void Per(string path)
    {
        try
        {
            Process p = new();
            p.StartInfo.FileName = "sh";
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            p.StandardInput.WriteLine("chmod -R 777 " + path);
            p.StandardInput.WriteLine("exit");
            p.WaitForExit();

            string temp = p.StandardOutput.ReadToEnd();

            p.Dispose();
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Jvm.Error9"), e);
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
            var local = "";
            if (path.StartsWith(Name1))
            {
                local = Path.GetFullPath(BaseDir + path);
            }
            else
            {
                local = path;
            }
            if (!string.IsNullOrWhiteSpace(local) && File.Exists(local))
            {
                Process p = new();
                p.StartInfo.FileName = local;
                p.StartInfo.Arguments = "-version";
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WorkingDirectory = BaseDir;
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
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Jvm.Error10"), e);
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
