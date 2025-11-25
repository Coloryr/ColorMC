using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ColorMC.Core.GuiHandle;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;

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
    public static async Task<StringRes> AddJavaZipAsync(string file, string name, IZipGui zip)
    {
        try
        {
            var res = await JvmPath.UnzipJavaAsync(file, name, zip);
            if (!res.State)
            {
                var text = res.Error.GetName();
                return new StringRes { Data = text };
            }
            return new StringRes { State = true };
        }
        catch (Exception e)
        {
            var log = string.Format(LanguageUtils.Get("Core.Error.Log9"), file, name);
            string title = LanguageUtils.Get("AddJavaWindow.Text14");
            Logs.Error(log, e);
            WindowManager.ShowError(title, log, e);

            return new StringRes() { Data = title };
        }
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
                return new StringRes { Data = LanguageUtils.Get("App.Text80") };
            }
            return new StringRes { State = true };
        }
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
            res.Add(new JavaDisplayModel
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
        IZipGui gui)
    {
        var res = await JvmPath.InstallAsync(obj.Name, obj.File, obj.Sha256, obj.Url, gui);
        if (!res.State)
        {
            var text = res.Error.GetName();
            return new StringRes { Data = text };
        }
        return new StringRes { State = true };
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
}