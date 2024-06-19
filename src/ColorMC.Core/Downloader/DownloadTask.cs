using System.Collections.Concurrent;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Downloader;

internal class DownloadTask
{
    /// <summary>
    /// 取消下载
    /// </summary>
    private readonly CancellationTokenSource _cancel = new();

    /// <summary>
    /// 下载项目队列
    /// </summary>
    private readonly ConcurrentQueue<DownloadItemObj> _items = [];

    /// <summary>
    /// 总下载数量
    /// </summary>
    public int AllSize { get; private set; }
    /// <summary>
    /// 已下载数量
    /// </summary>
    public int DoneSize { get; private set; }

    public CancellationToken Token => _cancel.Token;

    private DownloadArg _arg;

    private int _threadCount;

    /// <summary>
    /// 处理完成信号量
    /// </summary>
    private readonly Semaphore _semaphore;

    public DownloadTask(ICollection<DownloadItemObj> list, DownloadArg arg)
    {
        var names = new List<string>();

        _semaphore = new(0, 2);
        _arg = arg;

        Logs.Info(LanguageHelper.Get("Core.Http.Info4"));

        //装填下载内容
        foreach (var item in list)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Url) || names.Contains(item.Name))
            {
                continue;
            }
            _items.Enqueue(item);
            item.UpdateD = arg.UpdateItem;
            names.Add(item.Name);
        }

        DoneSize = 0;
        AllSize = _items.Count;
    }

    public Task<bool> WaitDone()
    {
        return Task.Run(() =>
        {
            _semaphore.WaitOne();

            if (_cancel.IsCancellationRequested)
            {
                return false;
            }

            return AllSize == DoneSize;
        });
    }

    public void Update()
    {
        _arg.UpdateTask?.Invoke(AllSize, DoneSize);
    }

    public void Cancel()
    {
        _items.Clear();
    }

    /// <summary>
    /// 获取下载项目
    /// </summary>
    /// <returns></returns>
    public DownloadItemObj? GetItem()
    {
        if (_items.TryDequeue(out var item))
        {
            return item;
        }

        return null;
    }

    /// <summary>
    /// 下载线程完成
    /// </summary>
    public void Done()
    {
        DoneSize++;
        Update();
    }

    /// <summary>
    /// 线程完成
    /// </summary>
    public void ThreadDone()
    {
        _threadCount++;
        if (_threadCount >= ConfigUtils.Config.Http.DownloadThread)
        {
            //任务结束
            _semaphore.Release();
            DownloadManager.TaskDone(_arg);
        }
    }
}
