using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Diagnostics;

namespace ColorMC.Core.LaunchPath;

public static class JvmPath
{
    public static Dictionary<string, JavaInfo> Jvms { get; } = new();

    public static (bool Res, string Msg) AddItem(string name, string local)
    {
        if (Jvms.ContainsKey(name))
        {
            return (false, LanguageHelper.GetName("Core.Path.Jvm.Error1"));
        }
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

        });

        if (objs.Count != Jvms.Count)
        {
            objs.Clear();
            foreach (var item in Jvms)
            {
                objs.Add(new JvmConfigObj()
                {
                    Name = item.Key,
                    Local = item.Value.Path
                });
            }
        }

    }

    public static JavaInfo? GetInfo(string name)
    {
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
}
