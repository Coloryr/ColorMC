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
    /// ����Javaѹ����
    /// </summary>
    /// <param name="file"></param>
    /// <param name="name"></param>
    /// <param name="zip"></param>
    /// <returns></returns>
    public static async Task<MessageRes> AddJavaZip(string file, string name, ColorMCCore.ZipUpdate zip)
    {
        return await JvmPath.UnzipJavaAsync(new UnzipArg
        {
            File = file,
            Name = name,
            Zip = zip
        });
    }

    /// <summary>
    /// ��ȡJava����
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
    /// ���Բ����Java
    /// </summary>
    /// <param name="name"></param>
    /// <param name="local"></param>
    /// <returns></returns>
    public static MessageRes AddJava(string name, string local)
    {
        var res = JvmPath.AddItem(name, local);
        if (res.State == false)
        {
            return new() { Message = res.Message };
        }
        else
        {
            var info = JvmPath.GetInfo(res.Message);
            if (info == null)
            {
                return new() { Message = App.Lang("JavaBinding.Error1") };
            }
            return new() { State = true };
        }
    }

    /// <summary>
    /// ��ȡJava��Ϣ
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static JavaInfo? GetJavaInfo(string path)
    {
        return JavaHelper.GetJavaInfo(path);
    }

    /// <summary>
    /// ɾ��Java
    /// </summary>
    /// <param name="name"></param>
    public static void RemoveJava(string name)
    {
        JvmPath.Remove(name);
    }

    /// <summary>
    /// ��ȡJava�б�
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
    /// ����Java
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="zip"></param>
    /// <param name="unzip"></param>
    /// <returns></returns>
    public static async Task<MessageRes> DownloadJava(JavaDownloadModel obj,
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
            return new() { Message = res.Message };
        }

        return new() { State = true };
    }

    /// <summary>
    /// ��ȡ�Ƽ�·��
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

        if (Directory.Exists(JvmPath.BaseDir))
        {
            return new DirectoryInfo(JvmPath.BaseDir);
        }

        return null;
    }

    /// <summary>
    /// ɾ������Java
    /// </summary>
    public static void RemoveAllJava()
    {
        JvmPath.RemoveAll();
    }

    /// <summary>
    /// ����Java
    /// </summary>
    /// <returns></returns>
    public static Task<List<JavaInfo>?> FindJava()
    {
        return Task.Run(JavaHelper.FindJava);
    }
}