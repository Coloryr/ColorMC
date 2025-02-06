using System.Collections.Concurrent;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Downloader;

/// <summary>
/// 下载任务
/// </summary>
internal class DownloadTask
{
    internal CancellationToken Token => _cancel.Token;

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
    private readonly int _allSize;
    /// <summary>
    /// 下载时GUI返回参数
    /// </summary>
    private readonly DownloadArg _arg;
    /// <summary>
    /// 已下载数量
    /// </summary>
    private int _doneSize;
    /// <summary>
    /// 结束线程数量
    /// </summary>
    private int _doneThreadCount;

    /// <summary>
    /// 处理完成信号量
    /// </summary>
    private readonly Semaphore _semaphore;

    /// <summary>
    /// 下载任务
    /// </summary>
    /// <param name="list">下载项目列表</param>
    /// <param name="arg">GUI下载参数</param>
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

        _doneSize = 0;
        _allSize = _items.Count;
    }

    /// <summary>
    /// 等待下载完成
    /// </summary>
    /// <returns>是否成功下载</returns>
    public Task<bool> WaitDone()
    {
        return Task.Run(() =>
        {
            _semaphore.WaitOne();

            if (_cancel.IsCancellationRequested)
            {
                return false;
            }

            return _allSize == _doneSize;
        });
    }

    /// <summary>
    /// 更新数据
    /// </summary>
    public void Update()
    {
        _arg.UpdateTask?.Invoke(_allSize, _doneSize);
    }

    /// <summary>
    /// 取消下载
    /// </summary>
    public void Cancel()
    {
        _items.Clear();
        _cancel.Cancel();
    }

    /// <summary>
    /// 获取下载项目
    /// </summary>
    /// <returns>下载项目</returns>
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
        _doneSize++;
        Update();
    }

    /// <summary>
    /// 线程完成
    /// </summary>
    public void ThreadDone()
    {
        lock (this)
        {
            _doneThreadCount++;
        }
        if (_doneThreadCount >= DownloadManager.ThreadCount)
        {
            //任务结束
            _semaphore.Release();
            DownloadManager.TaskDone(_arg);
        }
    }
}
