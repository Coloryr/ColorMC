using ColorMC.Core.Objs;

namespace ColorMC.Core.GuiHandle;

/// <summary>
/// 下载界面调用所需参数
/// </summary>
public interface IDownloadGui
{
    /// <summary>
    /// 下载状态更新
    /// </summary>
    /// <param name="thread">当前线程</param>
    /// <param name="state">是否还在下载</param>
    /// <param name="count">下载任务总数</param>
    public void Update(int thread, bool state, int count);
    /// <summary>
    /// 下载任务更新
    /// </summary>
    /// <param name="type">更新类型</param>
    /// <param name="data">更新数据</param>
    public void UpdateTask(UpdateType type, int data);
    /// <summary>
    /// 下载项目状态更新
    /// </summary>
    /// <param name="thread">线程号</param>
    /// <param name="obj">下载项目</param>
    public void UpdateItem(int thread, FileItemObj obj);
}
