using System.Collections.Generic;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加资源<br/>
/// 下载标记
/// </summary>
public partial class AddControlModel
{
    /// <summary>
    /// 下载标记
    /// </summary>
    private record DownloadItemInfo
    {
        /// <summary>
        /// 资源类型
        /// </summary>
        public FileType Type;
        /// <summary>
        /// 下载源
        /// </summary>
        public SourceType Source;
        /// <summary>
        /// 项目ID
        /// </summary>
        public string PID;
    }

    /// <summary>
    /// 正在下载的项目
    /// </summary>
    private readonly HashSet<DownloadItemInfo> _nowDownload = [];

    /// <summary>
    /// 开始下载项目
    /// </summary>
    /// <param name="info">下载项目</param>
    private void StartDownload(DownloadItemInfo info)
    {
        _nowDownload.Add(info);

        foreach (var item in DisplayList)
        {
            if (item.FileType == info.Type && item.SourceType == info.Source
                && item.ID == info.PID)
            {
                item.NowDownload = true;
            }
        }
    }

    /// <summary>
    /// 下载项目结束
    /// </summary>
    /// <param name="info">下载项目</param>
    /// <param name="done">是否下载完成</param>
    private void StopDownload(DownloadItemInfo info, bool done)
    {
        _nowDownload.Remove(info);

        foreach (var item in DisplayList)
        {
            if (item.FileType == info.Type && item.SourceType == info.Source
                && item.ID == info.PID)
            {
                item.NowDownload = false;
                item.IsDownload = done;
            }
        }
    }

    /// <summary>
    /// 测试是否正在下载
    /// </summary>
    /// <param name="item">项目显示</param>
    private void TestFileItem(FileItemModel item)
    {
        foreach (var info in _nowDownload)
        {
            if (item.FileType == info.Type && item.SourceType == info.Source
                && item.ID == info.PID)
            {
                item.NowDownload = true;
            }
        }
    }
}
