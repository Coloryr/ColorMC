using System.Diagnostics;
using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 游戏启动类
/// </summary>
public static class Launch
{
    /// <summary>
    /// 实例启动前登录账户
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="larg">启动参数</param>
    /// <exception cref="LaunchException"></exception>
    private static async Task AuthLoginAsync(GameSettingObj obj, GameLaunchArg larg)
    {
        larg.Update2?.Invoke(obj, LaunchState.Login);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        //刷新账户token
        var res1 = await larg.Auth.RefreshTokenAsync();
        if (res1.LoginState != LoginState.Done)
        {
            if (larg.Auth.AuthType == AuthType.OAuth
                && !string.IsNullOrWhiteSpace(larg.Auth.UUID)
                && larg.Loginfail != null
                && await larg.Loginfail(larg.Auth))
            {
                larg.Auth = new LoginObj
                {
                    UserName = larg.Auth.UserName,
                    UUID = larg.Auth.UUID,
                    AuthType = AuthType.Offline
                };
            }
            else
            {
                larg.Update2?.Invoke(obj, LaunchState.LoginFail);
                if (res1.Ex != null)
                {
                    throw new LaunchException(LaunchState.LoginFail, res1.Message!, res1.Ex);
                }

                throw new LaunchException(LaunchState.LoginFail, res1.Message!);
            }
        }
        else
        {
            larg.Auth = res1.Auth!;
            larg.Auth.Save();
        }

        stopwatch.Stop();
        string temp = string.Format(LanguageHelper.Get("Core.Launch.Info4"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.OnGameLog(obj, temp);
        Logs.Info(temp);
    }

    /// <summary>
    /// 处理服务器实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="larg">启动参数</param>
    /// <param name="token"></param>
    /// <exception cref="LaunchException"></exception>
    private static async Task PackUpdateAsync(GameSettingObj obj, GameLaunchArg larg, CancellationToken token)
    {
        if (obj.ModPackType != SourceType.ColorMC || string.IsNullOrWhiteSpace(obj.ServerUrl))
        {
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var pack = await obj.ServerPackCheckAsync(new ServerPackCheckArg
        {
            State = larg.State,
            Select = larg.Select
        }, token);

        stopwatch.Stop();
        var temp = string.Format(LanguageHelper.Get("Core.Launch.Info14"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.OnGameLog(obj, temp);
        Logs.Info(temp);

        if (token.IsCancellationRequested || pack)
        {
            return;
        }

        //检查失败询问是否取消启动
        if (larg.Request == null)
        {
            throw new LaunchException(LaunchState.VersionError,
                string.Format(LanguageHelper.Get("Core.Launch.Info16"), obj.Name));
        }

        var res2 = await larg.Request(string.Format(LanguageHelper.Get("Core.Launch.Info15"), obj.Name));
        if (!res2)
        {
            throw new LaunchException(LaunchState.Cancel, LanguageHelper.Get("Core.Launch.Error8"));
        }
    }

    /// <summary>
    /// 检查游戏实例文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="larg">启动参数</param>
    /// <param name="token"></param>
    /// <returns>游戏实例启动配置</returns>
    /// <exception cref="LaunchException"></exception>
    private static async Task<GameLaunchObj> CheckGameFileAsync(GameSettingObj obj, GameLaunchArg larg,
        CancellationToken token)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var res3 = await larg.Auth.CheckLoginCoreAsync(token);
        var arg1 = await obj.MakeArgAsync(larg, token);
        var res = await obj.CheckGameFileAsync(arg1, token);
        stopwatch.Stop();
        var temp = string.Format(LanguageHelper.Get("Core.Launch.Info5"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.OnGameLog(obj, temp);
        Logs.Info(temp);

        if (res3 != null)
        {
            res.Add(res3);
        }

        //下载缺失的文件
        if (res.IsEmpty)
        {
            return arg1;
        }

        bool download = true;
        if (ConfigUtils.Config.Http.AutoDownload == false && larg.Request != null)
        {
            download = await larg.Request(LanguageHelper.Get("Core.Launch.Info12"));
        }

        if (download)
        {
            larg.Update2?.Invoke(obj, LaunchState.Download);

            stopwatch.Reset();
            stopwatch.Start();

            var ok = await DownloadManager.StartAsync([.. res]);
            if (!ok)
            {
                throw new LaunchException(LaunchState.LostFile,
                    LanguageHelper.Get("Core.Launch.Error5"));
            }

            stopwatch.Stop();
            temp = string.Format(LanguageHelper.Get("Core.Launch.Info7"),
                obj.Name, stopwatch.Elapsed.ToString());
            ColorMCCore.OnGameLog(obj, temp);
            Logs.Info(temp);
        }

        return arg1;
    }

    /// <summary>
    /// 创建启动运行环境
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>运行环境</returns>
    private static Dictionary<string, string> MakeEnv(GameSettingObj obj)
    {
        var env = new Dictionary<string, string>();
        string envstr;
        if (obj.JvmArg?.JvmEnv is { } str)
        {
            envstr = str;
        }
        else if (ConfigUtils.Config.DefaultJvmArg.JvmEnv is { } str1)
        {
            envstr = str1;
        }
        else
        {
            envstr = "";
        }

        if (!string.IsNullOrWhiteSpace(envstr))
        {
            var list1 = envstr.Split('\n');
            foreach (var item in list1)
            {
                var item1 = item.Trim();
                var index = item1.IndexOf('=');
                string key, value;
                if (index == -1)
                {
                    key = item1;
                    value = "";
                }
                else if (index + 1 == item1.Length)
                {
                    key = item1[..^1];
                    value = "";
                }
                else
                {
                    key = item1[..index];
                    value = item1[(index + 1)..];
                }

                if (!env.TryAdd(key, value))
                {
                    env[key] = value;
                }
            }
        }

        return env;
    }

    /// <summary>
    /// 查找合适的Java
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="larg">启动参数</param>
    /// <param name="arg1"></param>
    /// <returns>Java路径</returns>
    /// <exception cref="LaunchException"></exception>
    private static async Task<string> FindJavaAsync(GameSettingObj obj, GameLaunchArg larg, GameLaunchObj arg1)
    {
        if (!string.IsNullOrWhiteSpace(obj.JvmLocal))
        {
            return obj.JvmLocal;
        }

        if (JvmPath.Jvms.Count == 0)
        {
            var list1 = await JavaHelper.FindJava();
            list1?.ForEach(item => JvmPath.AddItem(item.Type + "_" + item.Version, item.Path));
        }

        var jvm = JvmPath.GetInfo(obj.JvmName);
        if (jvm == null)
        {
            foreach (var item in arg1.JavaVersions.OrderDescending())
            {
                jvm = JvmPath.FindJava(item);
                if (jvm != null)
                {
                    break;
                }
            }
        }

        if (jvm == null)
        {
            var jv = arg1.JavaVersions.First();
            larg.Update2?.Invoke(obj, LaunchState.JavaError);
            larg.Nojava?.Invoke(jv);
            throw new LaunchException(LaunchState.JavaError,
                string.Format(LanguageHelper.Get("Core.Launch.Error6"), jv));
        }

        return jvm.GetPath();
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="cmd">命令</param>
    /// <param name="env">运行环境</param>
    /// <param name="runsame">是否不等待结束</param>
    /// <param name="admin">管理员启动</param>
    private static void CmdRun(this GameSettingObj obj, string cmd, Dictionary<string, string> env, bool runsame,
        bool admin)
    {
        var args = cmd.Split('\n');
        var name = args[0].Trim();

        //查找文件
        if (!File.Exists(name))
        {
            name = Path.Combine(obj.GetBasePath(), name);
        }

        if (!File.Exists(name))
        {
            name = Path.Combine(obj.GetGamePath(), name);
        }

        if (!File.Exists(name))
        {
            ColorMCCore.OnGameLog(obj, string.Format(LanguageHelper.Get("Core.Launch.Error14"), name));
            return;
        }

        //准备启动
        var info = new ProcessStartInfo(name)
        {
            WorkingDirectory = obj.GetGamePath(),
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        foreach (var item in env)
        {
            info.Environment.Add(item.Key, item.Value);
        }

        for (int a = 1; a < args.Length; a++)
        {
            info.ArgumentList.Add(args[a].Trim());
        }

        var p = new Process
        {
            EnableRaisingEvents = true,
            StartInfo = info
        };
        p.OutputDataReceived += (_, b) => { ColorMCCore.OnGameLog(obj, b.Data); };
        p.ErrorDataReceived += (_, b) => { ColorMCCore.OnGameLog(obj, b.Data); };
        p.Exited += (_, _) => { p.Dispose(); };
        ProcessUtils.Launch(p, admin);
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        //是否与游戏同时启动
        if (runsame)
        {
            return;
        }

        p.WaitForExit();
    }

    /// <summary>
    /// 同时启动多个实例
    /// </summary>
    /// <param name="objs">启动的游戏列表</param>
    /// <param name="larg">启动参数</param>
    /// <param name="cancels">取消Token</param>
    /// <returns>启动结果</returns>
    public static async Task<Dictionary<GameSettingObj, GameLaunchRes>>
        StartGameAsync(this ICollection<GameSettingObj> objs, GameLaunchArg larg, Dictionary<string, CancellationToken> cancels)
    {
        //清理存在的实例日志
        foreach (var item in objs)
        {
            ColorMCCore.GameLogClear(item);
        }

        var list = new Dictionary<GameSettingObj, GameLaunchRes>();
        var list2 = new List<GameRunObj>();
        //登录账户
        var temp1 = objs.First();
        try
        {
            await AuthLoginAsync(temp1, larg);
        }
        catch (Exception e)
        {
            list.Add(temp1, new GameLaunchRes { Ex = e });
            return list;
        }

        if (cancels.Values.Any(item => item.IsCancellationRequested))
        {
            return list;
        }

        //逐个实例启动
        foreach (var obj in objs)
        {
            try
            {
                //版本号检测
                if (string.IsNullOrWhiteSpace(obj.Version)
                    || (obj.Loader != Loaders.Normal && string.IsNullOrWhiteSpace(obj.LoaderVersion))
                    || (obj.Loader == Loaders.Custom && !File.Exists(obj.GetGameLoaderFile())))
                {
                    throw new LaunchException(LaunchState.VersionError, LanguageHelper.Get("Core.Launch.Error7"));
                }

                var cancel = cancels[obj.UUID];

                //服务器实例更新
                await PackUpdateAsync(obj, larg, cancel);
                if (cancel.IsCancellationRequested)
                {
                    return list;
                }

                larg.Update2?.Invoke(obj, LaunchState.Check);

                //检查游戏文件
                var arg1 = await CheckGameFileAsync(obj, larg, cancel);

                if (cancel.IsCancellationRequested)
                {
                    return list;
                }

                //查找合适的JAVA
                var path = await FindJavaAsync(obj, larg, arg1);
                if (cancel.IsCancellationRequested)
                {
                    return list;
                }

                //准备Jvm参数
                larg.Update2?.Invoke(obj, LaunchState.JvmPrepare);
                var arg = obj.MakeRunArg(larg, arg1, true);

                if (cancel.IsCancellationRequested)
                {
                    return list;
                }

                //打印Jvm参数
                ColorMCCore.OnGameLog(obj, LanguageHelper.Get("Core.Launch.Info1"));
                bool hidenext = false;
                foreach (var item in arg)
                {
                    if (hidenext)
                    {
                        hidenext = false;
                        ColorMCCore.OnGameLog(obj, "******");
                    }
                    else
                    {
                        ColorMCCore.OnGameLog(obj, item);
                    }

                    var low = item.ToLower();
                    if (low.StartsWith("--uuid") || low.StartsWith("--accesstoken"))
                    {
                        hidenext = true;
                    }
                }

                ColorMCCore.OnGameLog(obj, LanguageHelper.Get("Core.Launch.Info3"));
                ColorMCCore.OnGameLog(obj, path);

                if (cancel.IsCancellationRequested)
                {
                    return list;
                }

                //自定义启动参数
                var env = MakeEnv(obj);

                if (cancel.IsCancellationRequested)
                {
                    return list;
                }

                list2.Add(new GameRunObj()
                {
                    Obj = obj,
                    Path = path,
                    Arg = arg,
                    Env = env,
                    Admin = larg.Admin,
                    Auth = larg.Auth
                });
            }
            catch (Exception e)
            {
                list.Add(obj, new GameLaunchRes { Ex = e });
            }
        }

        foreach (var item1 in list2)
        {
            //启动进程
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var handel = new GameHandel(item1);
            stopwatch.Stop();
            var temp = string.Format(LanguageHelper.Get("Core.Launch.Info6"),
                item1.Obj.Name, stopwatch.Elapsed.ToString());
            ColorMCCore.OnGameLog(item1.Obj, temp);
            Logs.Info(temp);

            ColorMCCore.AddGameHandel(item1.Obj.UUID, handel);
            list.Add(item1.Obj, new GameLaunchRes { Handel = handel });
        }

        return list;
    }

    /// <summary>
    /// 生成游戏启动参数，不用于启动，用于生成启动脚本
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="larg">请求参数</param>
    /// <returns>启动参数</returns>
    public static async Task<CreateCmdRes> CreateGameCmdAsync(this GameSettingObj obj, GameLaunchArg larg)
    {
        //版本号检测
        if (string.IsNullOrWhiteSpace(obj.Version)
            || (obj.Loader is not (Loaders.Normal or Loaders.Custom) && string.IsNullOrWhiteSpace(obj.LoaderVersion))
            || (obj.Loader is Loaders.Custom && !File.Exists(obj.GetGameLoaderFile())))
        {
            return new CreateCmdRes { Message = LanguageHelper.Get("Core.Launch.Error7") };
        }

        //登录账户
        await AuthLoginAsync(obj, larg);

        //检查游戏文件
        var arg1 = await obj.MakeArgAsync(larg, CancellationToken.None);

        //查找Java
        var path = await FindJavaAsync(obj, larg, arg1);

        if (!File.Exists(path))
        {
            return new CreateCmdRes { Message = LanguageHelper.Get("Core.Launch.Error13") };
        }

        //准备Jvm参数
        larg.Update2?.Invoke(obj, LaunchState.JvmPrepare);
        var arg = obj.MakeRunArg(larg, arg1, false);

        //自定义启动参数
        var env = new Dictionary<string, string>();
        string envstr;
        if (obj.JvmArg?.JvmEnv is { } str)
        {
            envstr = str;
        }
        else if (ConfigUtils.Config.DefaultJvmArg.JvmEnv is { } str1)
        {
            envstr = str1;
        }
        else
        {
            envstr = "";
        }

        //需要处理环境变量
        if (!string.IsNullOrWhiteSpace(envstr))
        {
            var list1 = envstr.Split('\n');
            foreach (var item in list1)
            {
                var item1 = item.Trim();
                var index = item1.IndexOf('=');
                string key, value;
                if (index == -1)
                {
                    key = item1;
                    value = "";
                }
                else if (index + 1 == item1.Length)
                {
                    key = item1[..^1];
                    value = "";
                }
                else
                {
                    key = item1[..index];
                    value = item1[(index + 1)..];
                }

                if (!env.TryAdd(key, value))
                {
                    env[key] = value;
                }
            }
        }

        larg.Update2?.Invoke(obj, LaunchState.End);

        return new CreateCmdRes
        {
            Res = true,
            Args = arg,
            Dir = obj.GetGamePath(),
            Java = path,
            Envs = env
        };
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="larg">启动参数</param>
    /// <param name="token">取消Token</param>
    /// <exception cref="LaunchException">启动错误</exception>
    /// <returns>游戏句柄</returns>
    public static async Task<GameHandel?> StartGameAsync(this GameSettingObj obj, GameLaunchArg larg,
        CancellationToken token)
    {
        ColorMCCore.GameLogClear(obj);

        if (obj.CustomLoader?.CustomJson != true)
        {
            //版本号检测
            if (string.IsNullOrWhiteSpace(obj.Version)
                || (obj.Loader is not (Loaders.Normal or Loaders.Custom) &&
                    string.IsNullOrWhiteSpace(obj.LoaderVersion))
                || (obj.Loader is Loaders.Custom && !File.Exists(obj.GetGameLoaderFile())))
            {
                throw new LaunchException(LaunchState.VersionError, LanguageHelper.Get("Core.Launch.Error7"));
            }
        }

        //登录账户
        await AuthLoginAsync(obj, larg);

        if (token.IsCancellationRequested)
        {
            return null;
        }

        //处理服务器实例
        await PackUpdateAsync(obj, larg, token);

        if (token.IsCancellationRequested)
        {
            return null;
        }

        larg.Update2?.Invoke(obj, LaunchState.Check);

        //检查游戏文件
        var arg1 = await CheckGameFileAsync(obj, larg, token);

        if (token.IsCancellationRequested)
        {
            return null;
        }

        //查找合适的JAVA
        var path = await FindJavaAsync(obj, larg, arg1);

        if (token.IsCancellationRequested)
        {
            return null;
        }

        if (!File.Exists(path))
        {
            throw new LaunchException(LaunchState.JavaError,
                LanguageHelper.Get("Core.Launch.Error13"));
        }

        if (token.IsCancellationRequested)
        {
            return null;
        }

        //准备Jvm参数
        larg.Update2?.Invoke(obj, LaunchState.JvmPrepare);

        var arg = obj.MakeRunArg(larg, arg1, true);

        ColorMCCore.OnGameLog(obj, LanguageHelper.Get("Core.Launch.Info1"));
        bool hidenext = false;
        //打印参数
        foreach (var item in arg)
        {
            if (hidenext)
            {
                hidenext = false;
                ColorMCCore.OnGameLog(obj, "******");
            }
            else
            {
                ColorMCCore.OnGameLog(obj, item);
            }

            var low = item.ToLower();
            if (low.StartsWith("--uuid") || low.StartsWith("--accesstoken"))
            {
                hidenext = true;
            }
        }

        ColorMCCore.OnGameLog(obj, LanguageHelper.Get("Core.Launch.Info3"));
        ColorMCCore.OnGameLog(obj, path);

        if (token.IsCancellationRequested)
        {
            return null;
        }

        //自定义启动参数
        var env = MakeEnv(obj);

        if (token.IsCancellationRequested)
        {
            return null;
        }

        var stopwatch = new Stopwatch();

        //启动前运行
        if (obj.JvmArg?.LaunchPre == true || ConfigUtils.Config.DefaultJvmArg.LaunchPre)
        {
            var cmd1 = obj.JvmArg?.LaunchPreData;
            var cmd2 = ConfigUtils.Config.DefaultJvmArg.LaunchPreData;
            var start = string.IsNullOrWhiteSpace(cmd1) ? cmd2 : cmd1;
            var prerun = obj.JvmArg?.PreRunSame ?? ConfigUtils.Config.DefaultJvmArg.PreRunSame;
            if (!string.IsNullOrWhiteSpace(start) &&
                (larg.Pre == null || await larg.Pre(true)))
            {
                stopwatch.Reset();
                stopwatch.Start();
                larg.Update2?.Invoke(obj, LaunchState.LaunchPre);
                start = obj.ReplaceArg(path, arg, start);
                obj.CmdRun(start, env, prerun, larg.Admin);
                stopwatch.Stop();
                string temp1 = string.Format(LanguageHelper.Get("Core.Launch.Info8"),
                    obj.Name, stopwatch.Elapsed.ToString());
                ColorMCCore.OnGameLog(obj, temp1);
                Logs.Info(temp1);
            }
            else
            {
                string temp2 = string.Format(LanguageHelper.Get("Core.Launch.Info10"),
                    obj.Name);
                ColorMCCore.OnGameLog(obj, temp2);
                Logs.Info(temp2);
            }
        }

        if (token.IsCancellationRequested)
        {
            return null;
        }
        if (token.IsCancellationRequested)
        {
            return null;
        }

        //启动进程
        stopwatch.Reset();
        stopwatch.Start();
        var handel = new GameHandel(new GameRunObj
        {
            Obj = obj,
            Arg = arg,
            Env = env,
            Path = path,
            Auth = larg.Auth,
            Admin = larg.Admin
        });
        stopwatch.Stop();
        var temp = string.Format(LanguageHelper.Get("Core.Launch.Info6"),
            obj.Name, stopwatch.Elapsed.ToString());
        ColorMCCore.OnGameLog(obj, temp);
        Logs.Info(temp);

        ColorMCCore.AddGameHandel(obj.UUID, handel);

        //启动后执行
        if (obj.JvmArg?.LaunchPost != true && !ConfigUtils.Config.DefaultJvmArg.LaunchPost)
        {
            return handel;
        }

        var start1 = obj.JvmArg?.LaunchPostData;
        if (string.IsNullOrWhiteSpace(start1))
        {
            start1 = ConfigUtils.Config.DefaultJvmArg?.LaunchPostData;
        }

        if (!string.IsNullOrWhiteSpace(start1) && (larg.Pre == null || await larg.Pre(false)))
        {
            stopwatch.Start();
            larg.Update2?.Invoke(obj, LaunchState.LaunchPost);
            start1 = obj.ReplaceArg(path, arg, start1);
            obj.CmdRun(start1, env, false, larg.Admin);
            stopwatch.Stop();
            ColorMCCore.OnGameLog(obj, string.Format(LanguageHelper.Get("Core.Launch.Info9"),
                obj.Name, stopwatch.Elapsed.ToString()));
        }
        else
        {
            ColorMCCore.OnGameLog(obj, string.Format(LanguageHelper.Get("Core.Launch.Info11"),
                obj.Name));
        }

        return handel;
    }
}
