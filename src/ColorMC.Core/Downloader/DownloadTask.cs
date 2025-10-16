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
    /// <summary>
    /// 任务是否已经取消
    /// </summary>
    /// <returns></returns>
    internal CancellationToken Token => _cancel.Token;

    /// <summary>
    /// 取消下载
    /// </summary>
    private readonly CancellationTokenSource _cancel = new();
    /// <summary>
    /// 下载项目队列
    /// </summary>
    private readonly ConcurrentQueue<DownloadItem> _items = [];
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
    /// 下次错误数量
    /// </summary>
    private int _errorSize;

    /// <summary>
    /// 处理完成信号量
    /// </summary>
    private readonly Semaphore _semaphore;

    /// <summary>
    /// 下载任务
    /// </summary>
    /// <param name="list">下载项目列表</param>
    /// <param name="arg">GUI下载参数</param>
    public DownloadTask(ICollection<FileItemObj> list, DownloadArg arg)
    {
        var names = new List<string>();

        _semaphore = new Semaphore(0, 2);
        _arg = arg;

        Logs.Info(LanguageHelper.Get("Core.Http.Info4"));

        //装填下载内容
        foreach (var item in list)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Url) || names.Contains(item.Name))
            {
                continue;
            }
            _items.Enqueue(new DownloadItem
            {
                Task = this,
                File = item
            });
            names.Add(item.Name);
        }

        _doneSize = 0;
        _allSize = _items.Count;
        _arg.UpdateTask?.Invoke(UpdateType.AddItems, _allSize);
    }

    public IEnumerable<DownloadItem> GetItems()
    {
        return _items;
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
        }, Token);
    }

    /// <summary>
    /// 取消下载
    /// </summary>
    public void Cancel()
    {
        _semaphore.Release();
        _items.Clear();
        _cancel.Cancel();
    }

    /// <summary>
    /// 下载线程完成
    /// </summary>
    public void Done()
    {
        _doneSize++;
        _arg.UpdateTask?.Invoke(UpdateType.ItemDone, 1);

        ThreadDone();
    }

    /// <summary>
    /// 任务下载错误
    /// </summary>
    public void Error()
    {
        _errorSize++;
        _arg.UpdateTask?.Invoke(UpdateType.ItemDone, 1);

        ThreadDone();
    }

    /// <summary>
    /// 线程完成
    /// </summary>
    public void ThreadDone()
    {
        if (_doneSize + _errorSize >= _items.Count)
        {
            //任务结束
            _semaphore.Release();
            DownloadManager.TaskRunNext(_arg, this);
        }
    }

    /// <summary>
    /// 更新线程状态
    /// </summary>
    /// <param name="index">线程</param>
    /// <param name="obj">下载文件</param>
    public void UpdateItem(int index, FileItemObj obj)
    {
        _arg.UpdateItem?.Invoke(index, obj);
    }
}
