using ColorMC.Core.GuiHandle;
using ColorMC.Core.Objs;

namespace ColorMC.Core.Downloader;

/// <remarks>
/// 下载任务
/// </remarks>
/// <param name="gui">GUI下载参数</param>
internal class DownloadTask(IDownloadGui? gui, IProgressGui? pgui)
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
    /// 总下载数量
    /// </summary>
    private int _allSize;
    /// <summary>
    /// 已下载数量
    /// </summary>
    private int _doneSize = 0;
    /// <summary>
    /// 下次错误数量
    /// </summary>
    private int _errorSize;

    /// <summary>
    /// 处理完成信号量
    /// </summary>
    private readonly Semaphore _semaphore = new(0, 2);

    /// <summary>
    /// 设置任务数量
    /// </summary>
    public void SetSize(int size)
    {
        _allSize = size;
        gui?.UpdateTask(UpdateType.AddItems, _allSize);
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

            return _errorSize == 0;
        }, Token);
    }

    /// <summary>
    /// 取消下载
    /// </summary>
    public void Cancel()
    {
        _cancel.Cancel();
        _semaphore.Release();

        DownloadManager.TaskDone(gui, this);
    }

    /// <summary>
    /// 下载线程完成
    /// </summary>
    public void Done()
    {
        _doneSize++;
        gui?.UpdateTask(UpdateType.ItemDone, 1);
        pgui?.SetNowProcess(_doneSize + _errorSize, _allSize);

        ItemDone();
    }

    /// <summary>
    /// 任务下载错误
    /// </summary>
    public void Error()
    {
        _errorSize++;
        gui?.UpdateTask(UpdateType.ItemDone, 1);
        pgui?.SetNowProcess(_doneSize + _errorSize, _allSize);

        ItemDone();
    }

    /// <summary>
    /// 线程完成
    /// </summary>
    private void ItemDone()
    {
        if (_doneSize + _errorSize >= _allSize)
        {
            //任务结束
            _semaphore.Release();
            DownloadManager.TaskDone(gui, this);
        }
    }

    /// <summary>
    /// 更新线程状态
    /// </summary>
    /// <param name="index">线程</param>
    /// <param name="obj">下载文件</param>
    public void UpdateItem(int index, FileItemObj obj)
    {
        gui?.UpdateItem(index, obj);
    }
}
