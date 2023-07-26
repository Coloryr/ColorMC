using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Collections.Concurrent;

namespace ColorMC.Core.Downloader;

/// <summary>
/// 下载器
/// </summary>
public static class DownloadManager
{
    /// <summary>
    /// 取消下载
    /// </summary>
    public static CancellationTokenSource Cancel { get; private set; } = new();
    /// <summary>
    /// 下载状态
    /// </summary>
    public static CoreRunState State { get; private set; } = CoreRunState.End;
    /// <summary>
    /// 缓存路径
    /// </summary>
    public static string DownloadDir { get; private set; }

    /// <summary>
    /// 总下载数量
    /// </summary>
    public static int AllSize { get; private set; }
    /// <summary>
    /// 已下载数量
    /// </summary>
    public static int DoneSize { get; private set; }

    /// <summary>
    /// 下载项目队列
    /// </summary>
    private readonly static ConcurrentQueue<DownloadItemObj> s_items = new();
    /// <summary>
    /// 下载线程
    /// </summary>
    private readonly static List<DownloadThread> s_threads = new();
    /// <summary>
    /// 信号量
    /// </summary>
    private static Semaphore s_semaphore;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        ColorMCCore.Stop += Stop;

        DownloadDir = dir + "download";
        Directory.CreateDirectory(DownloadDir);
        Logs.Info(string.Format(LanguageHelper.Get("Core.Http.Info1"),
            ConfigUtils.Config.Http.DownloadThread));
        s_semaphore = new(0, ConfigUtils.Config.Http.DownloadThread + 1);
        s_threads.ForEach(a => a.Close());
        s_threads.Clear();
        for (int a = 0; a < ConfigUtils.Config.Http.DownloadThread; a++)
        {
            DownloadThread thread = new(a);
            s_threads.Add(thread);
        }
        Clear();
    }

    /// <summary>
    /// 停止下载器
    /// </summary>
    private static void Stop()
    {
        Cancel.Cancel();
        s_threads.ForEach(a => a.Close());
    }

    /// <summary>
    /// 停止下载
    /// </summary>
    public static void DownloadStop()
    {
        s_threads.ForEach(a => a.DownloadStop());
        Cancel.Cancel();
        s_items.Clear();
    }

    /// <summary>
    /// 暂停下载
    /// </summary>
    public static void DownloadPause()
    {
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
        foreach (var item in s_threads)
        {
            item.Resume();
        }
    }

    /// <summary>
    /// 清空下载器
    /// </summary>
    private static void Clear()
    {
        Logs.Info(LanguageHelper.Get("Core.Http.Info2"));
        s_items.Clear();
        AllSize = 0;
        DoneSize = 0;
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="list">下载列表</param>
    /// <returns>结果</returns>
    public static async Task<bool> Start(List<DownloadItemObj> list)
    {
        List<string> names = new();
        if (State != CoreRunState.End)
            return false;

        Clear();
        Logs.Info(LanguageHelper.Get("Core.Http.Info4"));
        ColorMCCore.DownloaderUpdate?.Invoke(State = CoreRunState.Init);
        foreach (var item in list)
        {
            if (names.Contains(item.Name) || string.IsNullOrWhiteSpace(item.Url))
            {
                continue;
            }
            ColorMCCore.DownloadItemStateUpdate?.Invoke(-1, item);
            item.Update = (index) =>
            {
                ColorMCCore.DownloadItemStateUpdate?.Invoke(index, item);
            };
            s_items.Enqueue(item);
            names.Add(item.Name);
        }

        Logs.Info(LanguageHelper.Get("Core.Http.Info3"));
        DoneSize = 0;
        AllSize = s_items.Count;
        ColorMCCore.DownloaderUpdate?.Invoke(State = CoreRunState.Start);
        Cancel.Dispose();
        Cancel = new();
        foreach (var item in s_threads)
        {
            item.Start();
        }
        await Task.Run(() =>
        {
            for (int a = 0; a < ConfigUtils.Config.Http.DownloadThread; a++)
            {
                s_semaphore.WaitOne();
            }
        });

        ColorMCCore.DownloaderUpdate?.Invoke(State = CoreRunState.End);

        return AllSize == DoneSize;
    }

    /// <summary>
    /// 获取下载项目
    /// </summary>
    /// <returns></returns>
    public static DownloadItemObj? GetItem()
    {
        if (s_items.TryDequeue(out var item))
        {
            return item;
        }

        return null;
    }

    /// <summary>
    /// 下载线程完成
    /// </summary>
    public static void Done()
    {
        DoneSize++;
    }

    /// <summary>
    /// 线程完成
    /// </summary>
    public static void ThreadDone()
    {
        s_semaphore.Release();
    }

    /// <summary>
    /// 下载错误
    /// </summary>
    /// <param name="index">下载器号</param>
    /// <param name="item">下载项目</param>
    /// <param name="e">错误内容</param>
    public static void Error(int index, DownloadItemObj item, Exception e)
    {
        Logs.Error(string.Format(LanguageHelper.Get("Core.Http.Error1"),
            item.Name), e);
        ColorMCCore.DownloadItemError?.Invoke(index, item, e);
    }

    /// <summary>
    /// buffer大小
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns>大小</returns>
    public static int GetCopyBufferSize(Stream stream)
    {
        const int DefaultCopyBufferSize = 81920;
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
