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
    /// 取消下载
    /// </summary>
    private static CancellationTokenSource s_cancel = new();
    /// <summary>
    /// 下载状态
    /// </summary>
    public static DownloadState State { get; private set; } = DownloadState.End;
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
    private readonly static ConcurrentQueue<DownloadItemObj> s_items = [];
    /// <summary>
    /// 下载线程
    /// </summary>
    private readonly static List<DownloadThread> s_threads = [];
    /// <summary>
    /// 处理完成信号量
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

        LoadThread();
    }

    private static void LoadThread()
    {
        Logs.Info(string.Format(LanguageHelper.Get("Core.Http.Info1"),
            ConfigUtils.Config.Http.DownloadThread));
        s_semaphore = new(0, ConfigUtils.Config.Http.DownloadThread + 1);
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
        s_cancel.Cancel();
        s_threads.ForEach(a => a.Close());
    }

    /// <summary>
    /// 停止下载
    /// </summary>
    public static void DownloadStop()
    {
        s_cancel.Cancel();
        s_threads.ForEach(a => a.DownloadStop());
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

    public static Task<bool> StartAsync(ICollection<DownloadItemObj> list)
    {
        if (ColorMCCore.OnDownload == null)
        {
            return StartAsync(list, null, null);
        }
        else
        {
            return ColorMCCore.OnDownload(list);
        }
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="list">下载列表</param>
    /// <returns>结果</returns>
    public static async Task<bool> StartAsync(ICollection<DownloadItemObj> list,
        ColorMCCore.DownloaderUpdate? update, ColorMCCore.DownloadItemUpdate? update1)
    {
        var names = new List<string>();
        //下载器是否在运行
        if (State != DownloadState.End)
        {
            return false;
        }

        Clear();
        Logs.Info(LanguageHelper.Get("Core.Http.Info4"));

        update?.Invoke(State = DownloadState.Init);

        //装填下载内容
        foreach (var item in list)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Url)
                || names.Contains(item.Name))
            {
                continue;
            }
            item.UpdateD = update1;
            update1?.Invoke(item);
            s_items.Enqueue(item);
            names.Add(item.Name);
        }

        Logs.Info(LanguageHelper.Get("Core.Http.Info3"));
        DoneSize = 0;
        AllSize = s_items.Count;

        update?.Invoke(State = DownloadState.Start);

        s_cancel.Dispose();
        s_cancel = new();
        foreach (var item in s_threads)
        {
            item.Start(s_cancel.Token);
        }
        await Task.Run(() =>
        {
            for (int a = 0; a < ConfigUtils.Config.Http.DownloadThread; a++)
            {
                s_semaphore.WaitOne();
            }
        });

        update?.Invoke(State = DownloadState.End);

        if (s_cancel.IsCancellationRequested)
        {
            return false;
        }

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
    public static void Error(DownloadItemObj item, Exception e)
    {
        Logs.Error(string.Format(LanguageHelper.Get("Core.Http.Error1"), item.Name), e);
    }

    /// <summary>
    /// 计算buffer大小
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns>大小</returns>
    public static int GetCopyBufferSize(Stream stream)
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
