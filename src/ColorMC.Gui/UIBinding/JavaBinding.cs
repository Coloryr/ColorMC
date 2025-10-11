using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UIBinding;

public static class JavaBinding
{
    /// <summary>
    /// 导入Java压缩包
    /// </summary>
    /// <param name="file">压缩包位置</param>
    /// <param name="name">名字</param>
    /// <param name="zip">UI相关</param>
    /// <returns></returns>
    public static async Task<StringRes> AddJavaZipAsync(string file, string name, ColorMCCore.ZipUpdate zip)
    {
        return await JvmPath.UnzipJavaAsync(new UnzipArg
        {
            File = file,
            Name = name,
            Zip = zip
        });
    }

    /// <summary>
    /// 获取Java名字
    /// </summary>
    /// <returns></returns>
    public static List<string> GetJavaName()
    {
        var list = new List<string>();
        foreach (var item in JvmPath.Jvms)
        {
            list.Add(item.Key);
        }

        return list;
    }

    /// <summary>
    /// 测试并添加Java
    /// </summary>
    /// <param name="name">名字</param>
    /// <param name="local">路径</param>
    /// <returns></returns>
    public static StringRes AddJava(string name, string local)
    {
        var res = JvmPath.AddItem(name, local);
        if (res.State == false)
        {
            return res;
        }
        else
        {
            var info = JvmPath.GetInfo(res.Data);
            if (info == null)
            {
                return new() { Data = App.Lang("JavaBinding.Error1") };
            }
            return new() { State = true };
        }
    }

    /// <summary>
    /// 获取Java信息
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public static JavaInfo? GetJavaInfo(string path)
    {
        return JavaHelper.GetJavaInfo(path);
    }

    /// <summary>
    /// 删除Java
    /// </summary>
    /// <param name="name">名字</param>
    public static void RemoveJava(string name)
    {
        JvmPath.Remove(name);
    }

    /// <summary>
    /// 获取Java列表
    /// </summary>
    /// <returns></returns>
    public static List<JavaDisplayModel> GetJavas()
    {
        var res = new List<JavaDisplayModel>();
        foreach (var item in JvmPath.Jvms)
        {
            res.Add(new()
            {
                Name = item.Key,
                Path = item.Value.Path,
                MajorVersion = item.Value.MajorVersion.ToString(),
                Version = item.Value.Version,
                Type = item.Value.Type,
                Arch = item.Value.Arch.GetName()
            });
        }

        return res;
    }

    /// <summary>
    /// 下载Java
    /// </summary>
    /// <param name="obj">Java下载项目</param>
    /// <param name="zip">UI相关</param>
    /// <param name="unzip">UI相关</param>
    /// <returns></returns>
    public static async Task<StringRes> DownloadJavaAsync(JavaDownloadModel obj,
        ColorMCCore.ZipUpdate zip, ColorMCCore.JavaUnzip unzip)
    {
        var res = await JvmPath.InstallAsync(new InstallJvmArg
        {
            File = obj.File,
            Name = obj.Name,
            Sha256 = obj.Sha256,
            Url = obj.Url,
            Zip = zip,
            Unzip = unzip
        });
        if (!res.State)
        {
            return new() { Data = res.Data };
        }

        return new() { State = true };
    }

    /// <summary>
    /// 获取推荐路径
    /// </summary>
    /// <returns></returns>
    public static DirectoryInfo? GetSuggestedStartLocation()
    {
        switch (SystemInfo.Os)
        {
            case OsType.Windows:
                if (Directory.Exists("C:\\Program Files\\java"))
                    return new DirectoryInfo("C:\\Program Files\\java");
                else if (Directory.Exists("C:\\Program Files\\Java"))
                    return new DirectoryInfo("C:\\Program Files\\Java");
                break;
            case OsType.MacOS:
                if (Directory.Exists("/Library/Java/JavaVirtualMachines/"))
                    return new DirectoryInfo("/Library/Java/JavaVirtualMachines/");
                break;
        }

        return new DirectoryInfo(JvmPath.JavaDir);
    }

    /// <summary>
    /// 删除所有Java
    /// </summary>
    public static void RemoveAllJava()
    {
        JvmPath.RemoveAll();
    }

    /// <summary>
    /// 搜索Java
    /// </summary>
    /// <returns></returns>
    public static Task<List<JavaInfo>?> FindJavaAsync()
    {
        return Task.Run(JavaHelper.FindJava);
    }

    /// <summary>
    /// 搜索Java
    /// </summary>
    /// <param name="local">搜索路径</param>
    /// <returns></returns>
    public static Task<List<JavaInfo>?> FindJavaAsync(string local)
    {
        return Task.Run(() => JavaHelper.FindJava(local));
    }
}