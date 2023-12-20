using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using System.Diagnostics;

namespace ColorMC.Core;

public static class ColorMCCore
{
    public const string TopVersion = "A23";
    public const string DateVersion = "20231218";

    public const string Version = $"{TopVersion}.{DateVersion}";

    /// <summary>
    /// 运行路径
    /// </summary>
    public static string BaseDir { get; private set; }
    /// <summary>
    /// 错误显示回调
    /// 标题 错误 关闭程序
    /// </summary>
    public static Action<string?, Exception?, bool>? OnError { internal get; set; }
    /// <summary>
    /// 下载线程相应回调
    /// </summary>
    public static Action<CoreRunState>? DownloaderUpdate { internal get; set; }
    /// <summary>
    /// 下载项目更新回调
    /// </summary>
    public static Action<DownloadItemObj>? DownloadItemUpdate { internal get; set; }
    /// <summary>
    /// 下载项目错误回调
    /// </summary>
    public static Action<DownloadItemObj, Exception>? DownloadItemError { internal get; set; }

    /// <summary>
    /// 游戏实例覆盖回调
    /// </summary>
    public static Func<GameSettingObj, Task<bool>>? GameOverwirte { internal get; set; }
    /// <summary>
    /// 是否请求回调
    /// </summary>
    public static Func<string, Task<bool>>? GameRequest { internal get; set; }
    /// <summary>
    /// 游戏启动回调
    /// </summary>
    public static Action<GameSettingObj, LaunchState>? GameLaunch { get; set; }

    /// <summary>
    /// 压缩包处理回调
    /// </summary>
    public static Action<CoreRunState>? PackState { internal get; set; }
    /// <summary>
    /// 压缩包更新回调
    /// </summary>
    public static Action<int, int>? PackUpdate { internal get; set; }

    /// <summary>
    /// 游戏进程日志回调
    /// </summary>
    public static Action<Process?, string?>? ProcessLog { internal get; set; }
    /// <summary>
    /// 游戏日志回调
    /// </summary>
    public static Action<GameSettingObj, string?>? GameLog { internal get; set; }

    /// <summary>
    /// 登录状态回调
    /// </summary>
    public static Action<AuthState>? AuthStateUpdate { internal get; set; }
    /// <summary>
    /// 登录码回调
    /// </summary>
    public static Action<string, string>? LoginOAuthCode { internal get; set; }

    /// <summary>
    /// 语言更新回调
    /// </summary>
    public static Action<LanguageType>? LanguageReload { internal get; set; }
    /// <summary>
    /// 登录失败是否以离线方式启动
    /// </summary>
    public static Func<LoginObj, Task<bool>>? OfflineLaunch { internal get; set; }
    /// <summary>
    /// 解压Java时
    /// </summary>
    public static Action? JavaUnzip { internal get; set; }
    /// <summary>
    /// 没有Java时
    /// </summary>
    public static Action<int>? NoJava { internal get; set; }
    /// <summary>
    /// 需要更新时
    /// </summary>
    public static Func<string, Task<bool>>? UpdateSelect { internal get; set; }
    /// <summary>
    /// 更新状态
    /// </summary>
    public static Action<string?>? UpdateState { internal get; set; }
    /// <summary>
    /// 执行命令
    /// </summary>
    public static Func<bool, Task<bool>>? LaunchP { internal get; set; }
    /// <summary>
    /// 启动器加载完毕
    /// </summary>
    public static Action? LoadDone { internal get; set; }

    /// <summary>
    /// 手机端启动
    /// </summary>
    public static Action<GameSettingObj, JavaInfo, List<string>, Dictionary<string, string>>? PhoneGameLaunch { internal get; set; }
    /// <summary>
    /// 手机端Jvm安装
    /// </summary>
    public static Action<Stream, string>? PhoneJvmInstall { internal get; set; }
    /// <summary>
    /// 手机端读Java信息
    /// </summary>
    public static Func<string, JavaInfo?>? PhoneReadJvm { internal get; set; }
    /// <summary>
    /// 手机端读文件
    /// </summary>
    public static Func<string, Stream?>? PhoneReadFile { get; set; }
    /// <summary>
    /// 手机端获取运行路径
    /// </summary>
    public static Func<string>? PhoneGetDataDir { internal get; set; }
    /// <summary>
    /// 手机端Jvm运行
    /// </summary>
    public static Func<GameSettingObj, JavaInfo, string, List<string>,
        Dictionary<string, string>, Task<bool>> PhoneJvmRun
    { internal get; set; }
    /// <summary>
    /// 手机端打开网页
    /// </summary>
    public static Action<string?> PhoneOpenUrl { get; set; }

    /// <summary>
    /// 新运行
    /// </summary>
    public static bool NewStart { get; internal set; }

    /// <summary>
    /// 停止事件
    /// </summary>
    internal static event Action? Stop;

    /// <summary>
    /// 初始化阶段1
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(string dir)
    {
        BaseDir = dir;
        Directory.CreateDirectory(dir);
        LanguageHelper.Load(LanguageType.zh_cn);
        Logs.Init(dir);
        ToolPath.Init(dir);
        ConfigUtils.Init(dir);
        BaseClient.Init();

        Logs.Info(LanguageHelper.Get("Core.Info1"));
    }

    /// <summary>
    /// 初始化阶段2
    /// </summary>
    public static void Init1()
    {
        ConfigSave.Init();
        GameCount.Init(BaseDir);
        JvmPath.Init(BaseDir);
        FrpPath.Init(BaseDir);
        LocalMaven.Init(BaseDir);
        DownloadManager.Init(BaseDir);
        AuthDatabase.Init();
        MCPath.Init(BaseDir);
        LoadDone?.Invoke();

        Logs.Info(LanguageHelper.Get("Core.Info3"));
    }

    /// <summary>
    /// 执行关闭操作
    /// </summary>
    public static void Close()
    {
        Stop?.Invoke();
    }
}