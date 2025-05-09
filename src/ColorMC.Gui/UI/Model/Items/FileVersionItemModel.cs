using System;
using System.Linq;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
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

    /// <summary>
    /// 文件类型
    /// </summary>
    public FileType FileType;
    /// <summary>
    /// 下载源
    /// </summary>
    public SourceType SourceType;

    /// <summary>
    /// Mod用
    /// </summary>
    public string ID;
    /// <summary>
    /// Mod用
    /// </summary>
    public string ID1;

    /// <summary>
    /// 数据
    /// </summary>
    public object Data;

    public FileVersionItemModel(CurseForgeModObj.CurseForgeDataObj data, FileType type)
    {
        Data = data;

        ID = data.ModId.ToString();
        ID1 = data.Id.ToString();
        Name = data.DisplayName;
        Size = UIUtils.MakeFileSize1(data.FileLength);
        Download = data.DownloadCount;
        Time = DateTime.Parse(data.FileDate);
        FileType = type;
        SourceType = SourceType.CurseForge;
    }

    public FileVersionItemModel(ModrinthVersionObj data, FileType type1)
    {
        Data = data;

        ID = data.ProjectId;
        ID1 = data.Id;
        Name = data.Name;
        var file = data.Files.FirstOrDefault(a => a.Primary) ?? data.Files[0];
        Size = UIUtils.MakeFileSize1(file.Size);
        Download = data.Downloads;
        Time = DateTime.Parse(data.DatePublished);
        FileType = type1;
        SourceType = SourceType.Modrinth;
    }

    /// <summary>
    /// 选中
    /// </summary>
    public void SetSelect()
    {
        Add?.SetSelect(this);
    }

    /// <summary>
    /// 上一页
    /// </summary>
    public void Back()
    {
        Add?.BackVersion();
    }

    /// <summary>
    /// 下一页
    /// </summary>
    public void Next()
    {
        Add?.NextVersion();
    }

    /// <summary>
    /// 安装
    /// </summary>
    public void Install()
    {
        Add?.Install(this);
    }
}

