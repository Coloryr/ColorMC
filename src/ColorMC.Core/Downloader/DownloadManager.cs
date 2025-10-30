using System.Collections.Concurrent;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Downloader;

internal record DownloadItem
{
    public DownloadTask Task;
    public FileItemObj File;
}

/// <summary>
/// 下载器
/// </summary>
public static class DownloadManager
{
    /// <summary>
    /// 下载状态
    /// </summary>
    public static bool State => s_download.Count > 0;
    /// <summary>
    /// 缓存路径
    /// </summary>
    public static string DownloadDir { get; private set; }

    /// <summary>
    /// 当前下载线程数
    /// </summary>
    internal static int ThreadCount => s_threads.Count;

    /// <summary>
    /// 下载线程
    /// </summary>
    private static readonly List<DownloadThread> s_threads = [];
    /// <summary>
    /// 下载任务
    /// </summary>
    private static readonly List<DownloadTask> s_tasks = [];
    /// <summary>
    /// 下载任务
    /// </summary>
    private static readonly ConcurrentQueue<DownloadItem> s_download = [];

    /// <summary>
    /// 是否停止
    /// </summary>
    private static bool s_stop;

    /// <summary>
    /// 初始化
    /// </summary>
    internal static void Init()
    {
        ColorMCCore.Stop += Close;

        DownloadDir = Path.Combine(ColorMCCore.BaseDir, Names.NameDownloadDir);

        Directory.CreateDirectory(DownloadDir);

        InitThread();
    }

    /// <summary>
    /// 停止下载
    /// </summary>
    public static void Stop()
    {
        if (s_stop)
        {
            return;
        }
        s_download.Clear();
        foreach (var item in s_tasks.ToArray())
        {
            item.Cancel();
        }
        s_threads.ForEach(a => a.DownloadStop());
    }

    /// <summary>
    /// 暂停下载
    /// </summary>
    public static void Pause()
    {
        if (s_stop)
        {
            return;
        }
        foreach (var item in s_threads)
        {
            item.Pause();
        }
    }

    /// <summary>
    /// 继续下载
    /// </summary>
    public static void Resume()
    {
        if (s_stop)
        {
            return;
        }
        foreach (var item in s_threads)
        {
            item.Resume();
        }
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="list">下载列表</param>
    /// <returns>是否成功</returns>
    public static async Task<bool> StartAsync(ICollection<FileItemObj> list)
    {
        if (s_stop)
        {
            return false;
        }
        var arg = ColorMCCore.OnDownload != null ? ColorMCCore.OnDownload() : new();

        return await StartAsync(list, arg);
    }

    /// <summary>
    /// 进行下一个任务
    /// </summary>
    /// <param name="arg">下载参数</param>
    internal static void TaskDone(DownloadArg arg, DownloadTask task)
    {
        s_tasks.Remove(task);

        //若没有下一个任务则全部完成
        arg.Update?.Invoke(s_threads.Count, s_tasks.Count > 0, s_tasks.Count);
    }

    internal static DownloadItem? GetDownloadItem()
    {
        if (s_download.TryDequeue(out var item))
        {
            return item;
        }

        return null;
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="list">下载列表</param>
    /// <param name="arg">下载参数</param>
    /// <returns>是否完成</returns>
    private static Task<bool> StartAsync(ICollection<FileItemObj> list, DownloadArg arg)
    {
        var task = new DownloadTask(arg);
        s_tasks.Add(task);

        Logs.Info(LanguageHelper.Get("Core.Info9"));

        var names = new List<string>();
        //装填下载内容
        foreach (var item in list)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Url) || names.Contains(item.Name))
            {
                continue;
            }
            s_download.Enqueue(new DownloadItem
            {
                Task = task,
                File = item
            });
            names.Add(item.Name);
        }

        task.SetSize(names.Count);

        Start();

        arg.Update?.Invoke(s_threads.Count, true, s_tasks.Count);

        return task.WaitDone();
    }

    /// <summary>
    /// 初始化下载线程
    /// </summary>
    private static void InitThread()
    {
        Stop();
        Logs.Info(string.Format(LanguageHelper.Get("Core.Info8"), ConfigUtils.Config.Http!.DownloadThread));
        for (int a = 0; a < ConfigUtils.Config.Http.DownloadThread; a++)
        {
            s_threads.Add(new DownloadThread(a));
        }
    }

    /// <summary>
    /// 停止下载器
    /// </summary>
    private static void Close()
    {
        s_stop = true;
        s_tasks.Clear();
        s_threads.ForEach(a => a.Close());
    }

    /// <summary>
    /// 开始下载任务
    /// </summary>
    /// <param name="task">下载任务</param>
    private static void Start()
    {
        foreach (var item in s_threads)
        {
            item.Start();
        }
    }
}
