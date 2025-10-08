using System.Text;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

public class GameLaunchPhone
{
#if Phone
    /// <summary>
    /// 获取Forge安装参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="v2">是否为1.13以上版本</param>
    /// <returns>安装参数</returns>
    private static List<string> MakeInstallForgeArg(this GameSettingObj obj, bool v2)
    {
        var jvm = new List<string>
        {
            $"-Dforgewrapper.librariesDir={LibrariesPath.BaseDir}",
            $"-Dforgewrapper.installer={(obj.Loader == Loaders.NeoForge ?
            GameDownloadHelper.BuildNeoForgeInstaller(obj.Version, obj.LoaderVersion!).Local :
            GameDownloadHelper.BuildForgeInstaller(obj.Version, obj.LoaderVersion!).Local)}",
            $"-Dforgewrapper.minecraft={LibrariesPath.GetGameFile(obj.Version)}",
            "-Dforgewrapper.justInstall=true"
        };

        var list = new Dictionary<LibVersionObj, string>();

        var forge = obj.Loader == Loaders.NeoForge
                ? VersionPath.GetNeoForgeInstallObj(obj.Version, obj.LoaderVersion!)!
                : VersionPath.GetForgeInstallObj(obj.Version, obj.LoaderVersion!)!;
        var list2 = GameDownloadHelper.BuildForgeLibs(forge, obj.Version, obj.LoaderVersion!,
            obj.Loader == Loaders.NeoForge, v2);
        foreach (var item in list2)
        {
            list.AddOrUpdate(FuntionUtils.MakeVersionObj(item.Name), item.Local);
        }

        GameHelper.ReadyForgeWrapper();
        list.AddOrUpdate(new(), GameHelper.ForgeWrapper);

        var libraries = new List<string>(list.Values);
        var classpath = new StringBuilder();
        var sep = SystemInfo.Os == OsType.Windows ? ';' : ':';
        ColorMCCore.OnGameLog(obj, LanguageHelper.Get("Core.Launch.Info2"));

        //添加lib路径到classpath
        foreach (var item in libraries)
        {
            if (File.Exists(item))
            {
                classpath.Append($"{item}{sep}");
            }
        }

        var cp = classpath.ToString()[..^1].Trim();
        jvm.Add("-cp");
        jvm.Add(cp);

        //var arg = MakeV2GameArg(obj);

        jvm.Add("io.github.zekerzhayard.forgewrapper.installer.Main");
        var forge1 = obj.Loader == Loaders.NeoForge
                ? obj.GetNeoForgeObj()!
                : obj.GetForgeObj()!;

        jvm.AddRange(forge1.Arguments.Game);

        return jvm;
    }

    private static int PhoneCmdRun(this GameSettingObj obj, string start, JavaInfo jvm1, Dictionary<string, string> env)
    {
        var args = start.Split('\n');
        var arglist = new List<string>();
        for (int a = 1; a < args.Length; a++)
        {
            arglist.Add(args[a].Trim());
        }

        return PhoneCmdRun(obj, arglist, jvm1, env);
    }

    private static int PhoneCmdRun(this GameSettingObj obj, List<string> arglist, JavaInfo jvm1, Dictionary<string, string> env)
    {
        var res2 = ColorMCCore.PhoneJvmRun(obj, jvm1, obj.GetGamePath(), arglist, env);
        res2.StartInfo.RedirectStandardError = true;
        res2.StartInfo.RedirectStandardInput = true;
        res2.StartInfo.RedirectStandardOutput = true;
        res2.OutputDataReceived += (a, b) =>
        {
            ColorMCCore.OnGameLog(obj, b.Data);
        };
        res2.ErrorDataReceived += (a, b) =>
        {
            ColorMCCore.OnGameLog(obj, b.Data);
        };
        res2.Start();
        res2.BeginOutputReadLine();
        res2.BeginErrorReadLine();

        res2.WaitForExit();

        return res2.ExitCode;
    }
#endif
}