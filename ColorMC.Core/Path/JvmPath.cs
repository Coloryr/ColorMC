using ColorMC.Core.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Path;

public record JavaInfo
{ 
    public string Path { get; set; }
    public string Version { get; set; }
    public int MajorVersion
    {
        get
        {
            string[] vers = Version.Trim().Split('.', '_', '-', '+', 'u', 'U');
            if (vers[0] == "1")
            {
                return int.Parse(vers[1]);
            }
            else
            {
                return int.Parse(vers[0]);
            }
        }
    }
    public string Type { get; set; }
    public ArchEnum Arch { get; set; }
}

public static class JvmPath
{
    public static Dictionary<string, JavaInfo> Jvms { get; } = new();

    public static (bool, string) AddItem(string name, string local) 
    {
        if (Jvms.ContainsKey(name))
        {
            return (false, "Java名字已存在");
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

        return (false, "Java检查失败");
    }

    public static void AddList(List<JvmConfigObj> objs)
    {
        Jvms.Clear();
        objs.ForEach(a =>
        {
            var info = GetJavaInfo(a.Local);
            if (info != null)
                Jvms.Add(a.Name, info);
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
