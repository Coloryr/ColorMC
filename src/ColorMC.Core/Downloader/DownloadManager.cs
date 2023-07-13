using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Buffers;
using System.Collections.Concurrent;

namespace ColorMC.Core.Downloader;

/// <summary>
/// 下载器
/// </summary>
public static class DownloadManager
{
    /// <summary>
    /// 下载项目队列
    /// </summary>
    private readonly static ConcurrentQueue<DownloadItemObj> Items = new();
    /// <summary>
    /// 项目名字
    /// </summary>
    private readonly static List<string> Name = new();
    /// <summary>
    /// 下载线程
    /// </summary>
    private readonly static List<DownloadThread> threads = new();
    /// <summary>
    /// 信号量
    /// </summary>
    private static Semaphore semaphore;
    /// <summary>
    /// 下载状态
    /// </summary>
    public static CoreRunState State { get; private set; } = CoreRunState.End;
    /// <summary>
    /// 缓存路径
    /// </summary>
    public static string DownloadDir { get; private set; } = "";

    /// <summary>
    /// 总下载数量
    /// </summary>
    public static int AllSize { get; private set; }
    /// <summary>
    /// 已下载数量
    /// </summary>
    public static int DoneSize { get; private set; }

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
        semaphore = new(0, ConfigUtils.Config.Http.DownloadThread + 1);
        threads.ForEach(a => a.Close());
        threads.Clear();
        for (int a = 0; a < ConfigUtils.Config.Http.DownloadThread; a++)
        {
            DownloadThread thread = new(a);
            threads.Add(thread);
        }
        Clear();
    }

    /// <summary>
    /// 停止下载器
    /// </summary>
    private static void Stop()
    {
        threads.ForEach(a => a.Close());
        threads.Clear();
    }

    /// <summary>
    /// 停止下载
    /// </summary>
    public static void DownloadStop()
    {
        threads.ForEach(a => a.DownloadStop());

        Name.Clear();
        Items.Clear();
    }

    /// <summary>
    /// 暂停下载
    /// </summary>
    public static void DownloadPause()
    {
        foreach (var item in threads)
        {
            item.Pause();
        }
    }

    /// <summary>
    /// 继续下载
    /// </summary>
    public static void DownloadResume()
    {
        foreach (var item in threads)
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
        Name.Clear();
        Items.Clear();
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
        if (State != CoreRunState.End)
            return false;

        Clear();
        Logs.Info(LanguageHelper.Get("Core.Http.Info4"));
        ColorMCCore.DownloaderUpdate?.Invoke(State = CoreRunState.Init);
        foreach (var item in list)
        {
            if (Name.Contains(item.Name) || string.IsNullOrWhiteSpace(item.Url))
                continue;
            ColorMCCore.DownloadItemStateUpdate?.Invoke(-1, item);
            item.Update = (index) =>
            {
                ColorMCCore.DownloadItemStateUpdate?.Invoke(index, item);
            };
            Items.Enqueue(item);
            Name.Add(item.Name);
        }

        Logs.Info(LanguageHelper.Get("Core.Http.Info3"));
        DoneSize = 0;
        AllSize = Items.Count;
        ColorMCCore.DownloaderUpdate?.Invoke(State = CoreRunState.Start);
        foreach (var item in threads)
        {
            item.Start();
        }
        await Task.Run(() =>
        {
            for (int a = 0; a < ConfigUtils.Config.Http.DownloadThread; a++)
            {
                semaphore.WaitOne();
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
        if (Items.TryDequeue(out var item))
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
        semaphore.Release();
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
