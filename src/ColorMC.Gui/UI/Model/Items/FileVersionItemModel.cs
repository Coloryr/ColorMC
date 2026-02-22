using System;
using System.Linq;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 项目显示
/// </summary>
public partial class FileVersionItemModel : SelectItemModel
{
    /// <summary>
    /// 添加
    /// </summary>
    public IAddFileControl? AddFile { get; set; }
    public IAddControl? Add { get; set; }

    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 下载次数
    /// </summary>
    public long Download { get; set; }
    /// <summary>
    /// 大小
    /// </summary>
    public string Size { get; set; }
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime Time { get; set; }
    /// <summary>
    /// 是否已经下载
    /// </summary>
    public bool IsDownload { get; set; }

    public readonly SourceItemObj Obj;

    /// <summary>
    /// 数据
    /// </summary>
    public object Data;

    public string ProjectName;

    public FileVersionItemModel(CurseForgeModObj.CurseForgeDataObj data, FileType type, string project)
    {
        Data = data;

        ProjectName = project;

        Obj = new SourceItemObj
        {
            Fid = data.Id.ToString(),
            Pid = data.ModId.ToString(),
            Source = SourceType.CurseForge,
            Type = type
        };

        Name = data.DisplayName;
        Size = UIUtils.MakeFileSize1(data.FileLength);
        Download = data.DownloadCount;
        Time = DateTime.Parse(data.FileDate);
    }

    public FileVersionItemModel(ModrinthVersionObj data, FileType type, string project)
    {
        Data = data;

        Obj = new SourceItemObj
        {
            Fid = data.Id,
            Pid = data.ProjectId,
            Source = SourceType.Modrinth,
            Type = type
        };

        Name = data.Name;
        var file = data.Files.FirstOrDefault(a => a.Primary) ?? data.Files[0];
        Size = UIUtils.MakeFileSize1(file.Size);
        Download = data.Downloads;
        Time = DateTime.Parse(data.DatePublished);
        ProjectName = project;
    }

    /// <summary>
    /// 选中
    /// </summary>
    public void SetSelect()
    {
        AddFile?.SetSelect(this);
    }

    /// <summary>
    /// 上一页
    /// </summary>
    public void Back()
    {
        AddFile?.LastVersionPage();
    }

    /// <summary>
    /// 下一页
    /// </summary>
    public void Next()
    {
        AddFile?.NextVersionPage();
    }

    /// <summary>
    /// 安装
    /// </summary>
    public void Install()
    {
        Add?.Install(this);
    }
}

