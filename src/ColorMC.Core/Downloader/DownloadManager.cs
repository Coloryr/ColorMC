using System.Collections.Concurrent;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Downloader;

/// <summary>
/// 下载器
/// </summary>
public static class DownloadManager
{
    /// <summary>
    /// 下载状态
    /// </summary>
    public static bool State { get; private set; }
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
    private static readonly ConcurrentQueue<DownloadTask> s_tasks = [];
    /// <summary>
    /// 当前任务
    /// </summary>
    private static DownloadTask? s_nowTask;
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
        s_nowTask?.Cancel();
        s_tasks.Clear();
        s_threads.ForEach(a => a.DownloadStop());
        State = false;
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
    public static async Task<bool> StartAsync(ICollection<DownloadItemObj> list)
    {
        if (s_stop)
        {
            return false;
        }
        var arg = ColorMCCore.OnDownload != null ? ColorMCCore.OnDownload() : new();

        return await StartAsync(list, arg);
    }

    /// <summary>
    /// 下载错误
    /// </summary>
    /// <param name="item">下载项目</param>
    /// <param name="e">错误内容</param>
    internal static void Error(DownloadItemObj item, Exception e)
    {
        Logs.Error(string.Format(LanguageHelper.Get("Core.Http.Error1"), item.Name), e);
    }

    /// <summary>
    /// 下载任务完成
    /// </summary>
    /// <param name="arg">下载参数</param>
    internal static void TaskDone(DownloadArg arg)
    {
        s_nowTask = null;
        Task.Run(() =>
        {
            if (s_tasks.TryDequeue(out s_nowTask))
            {
                arg.Update?.Invoke(s_threads.Count, State, s_tasks.Count + 1);
                Start(s_nowTask);
            }
            else
            {
                State = false;
                arg.Update?.Invoke(s_threads.Count, State, 0);
            }
        });
    }

    /// <summary>
    /// 计算buffer大小
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns>大小</returns>
    internal static int GetCopyBufferSize(Stream stream)
    {
        int DefaultCopyBufferSize = 81920;
        int bufferSize = DefaultCopyBufferSize;

        if (stream.CanSeek)
        {
            long length = stream.Length;
            long position = stream.Position;
            if (length <= position)
            {
                bufferSize = 1;
            }
            else
            {
                long remaining = length - position;
                if (remaining > 0)
                {
                    bufferSize = (int)Math.Min(bufferSize, remaining);
                }
            }
        }

        return bufferSize;
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="list">下载列表</param>
    /// <param name="arg">下载参数</param>
    /// <returns>是否完成</returns>
    private static Task<bool> StartAsync(ICollection<DownloadItemObj> list, DownloadArg arg)
    {
        var task = new DownloadTask(list, arg);
        s_tasks.Enqueue(task);

        if (State == false)
        {
            State = true;
            arg.Update?.Invoke(s_threads.Count, State, s_tasks.Count);
            TaskDone(arg);
        }
        else
        {
            arg.Update?.Invoke(s_threads.Count, State,
                s_tasks.Count + (s_nowTask != null ? 1 : 0));
        }

        return task.WaitDone();
    }

    /// <summary>
    /// 初始化下载线程
    /// </summary>
    private static void InitThread()
    {
        Stop();
        Logs.Info(string.Format(LanguageHelper.Get("Core.Http.Info1"),
            ConfigUtils.Config.Http.DownloadThread));
        for (int a = 0; a < ConfigUtils.Config.Http.DownloadThread; a++)
        {
            s_threads.Add(new(a));
        }
    }

    /// <summary>
    /// 停止下载器
    /// </summary>
    private static void Close()
    {
        s_stop = true;
        s_nowTask?.Cancel();
        s_tasks.Clear();
        s_threads.ForEach(a => a.Close());
        State = false;
    }

    /// <summary>
    /// 开始下载任务
    /// </summary>
    /// <param name="task">下载任务</param>
    private static void Start(DownloadTask task)
    {
        task.Update();
        foreach (var item in s_threads)
        {
            item.Start(task);
        }
    }
}
