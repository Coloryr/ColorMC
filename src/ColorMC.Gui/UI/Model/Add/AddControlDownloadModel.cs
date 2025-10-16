using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddControlModel
{
    public record DownloadItemInfo
    {
        public FileType Type;
        public SourceType Source;
        public string PID;
    }

    private readonly HashSet<DownloadItemInfo> _nowDownload = [];

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
