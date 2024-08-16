using System;
using System.Linq;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 项目显示
/// </summary>
public partial class FileVersionItemModel : SelectItemModel
{
    public IAddWindow? Add { get; set; }

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

    public FileVersionItemModel(CurseForgeModObj.Data data, FileType type)
    {
        Data = data;

        ID = data.modId.ToString();
        ID1 = data.id.ToString();
        Name = data.displayName;
        Size = UIUtils.MakeFileSize1(data.fileLength);
        Download = data.downloadCount;
        Time = DateTime.Parse(data.fileDate);
        FileType = type;
        SourceType = SourceType.CurseForge;
    }

    public FileVersionItemModel(ModrinthVersionObj data, FileType type1)
    {
        Data = data;

        ID = data.project_id;
        ID1 = data.id;
        Name = data.name;
        var file = data.files.FirstOrDefault(a => a.primary) ?? data.files[0];
        Size = UIUtils.MakeFileSize1(file.size);
        Download = data.downloads;
        Time = DateTime.Parse(data.date_published);
        FileType = type1;
        SourceType = SourceType.Modrinth;
    }

    public void SetSelect()
    {
        Add?.SetSelect(this);
    }

    public void Back()
    {
        Add?.BackVersion();
    }

    public void Next()
    {
        Add?.NextVersion();
    }

    internal void Install()
    {
        Add?.Install(this);
    }
}

