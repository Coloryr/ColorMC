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
    public static DownloadState State { get; private set; } = DownloadState.End;
    /// <summary>
    /// 缓存路径
    /// </summary>
    public static string DownloadDir { get; private set; }

    /// <summary>
    /// 下载线程
    /// </summary>
    private static readonly List<DownloadThread> s_threads = [];

    private static readonly ConcurrentQueue<DownloadTask> s_tasks = [];

    private static DownloadTask? s_nowTask;

    private static bool s_stop;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        ColorMCCore.Stop += Stop;

        DownloadDir = dir + "download";
        Directory.CreateDirectory(DownloadDir);

        LoadThread();
    }

    private static void LoadThread()
    {
        DownloadStop();
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
    private static void Stop()
    {
        s_stop = true;
        s_nowTask?.Cancel();
        s_tasks.Clear();
        s_threads.ForEach(a => a.Close());
        State = DownloadState.End;
    }

    /// <summary>
    /// 停止下载
    /// </summary>
    public static void DownloadStop()
    {
        if (s_stop)
        {
            return;
        }
        s_nowTask?.Cancel();
        s_tasks.Clear();
        s_threads.ForEach(a => a.DownloadStop());
        State = DownloadState.End;
    }

    /// <summary>
    /// 暂停下载
    /// </summary>
    public static void DownloadPause()
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
    public static void DownloadResume()
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
    /// 调用GUI的方式下载
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static async Task<bool> StartAsync(ICollection<DownloadItemObj> list)
    {
        if (s_stop)
        {
            return false;
        }
        DownloadArg arg;
        if (ColorMCCore.OnDownload == null)
        {
            arg = new();
            return await StartAsync(list, arg);
        }
        else
        {
            arg = ColorMCCore.OnDownload();
            return await StartAsync(list, arg);
        }
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="list">下载列表</param>
    /// <returns>结果</returns>
    private static async Task<bool> StartAsync(ICollection<DownloadItemObj> list, DownloadArg arg)
    {
        var task = new DownloadTask(list, arg);
        s_tasks.Enqueue(task);

        if (State == DownloadState.End)
        {
            State = DownloadState.Start;
            arg.Update?.Invoke(s_threads.Count, State, s_tasks.Count);
            TaskDone(arg);
        }
        else
        {
            arg.Update?.Invoke(s_threads.Count, State, 
                s_tasks.Count + (s_nowTask != null ? 1 : 0));
        }

        return await task.WaitDone();
    }

    internal static void TaskDone(DownloadArg arg)
    {
        s_nowTask = null;
        Task.Run(() =>
        {
            if (s_tasks.TryDequeue(out s_nowTask))
            {
                arg.Update?.Invoke(s_threads.Count, State, s_tasks.Count + 1);
                Start(s_nowTask, s_nowTask.Token);
            }
            else
            {
                State = DownloadState.End;
                arg.Update?.Invoke(s_threads.Count, State, 0);
            }
        });
    }

    internal static void Start(DownloadTask task, CancellationToken token)
    {
        task.Update();
        foreach (var item in s_threads)
        {
            item.Start(task, token);
        }
    }

    /// <summary>
    /// 下载错误
    /// </summary>
    /// <param name="index">下载器号</param>
    /// <param name="item">下载项目</param>
    /// <param name="e">错误内容</param>
    internal static void Error(DownloadItemObj item, Exception e)
    {
        Logs.Error(string.Format(LanguageHelper.Get("Core.Http.Error1"), item.Name), e);
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
}
