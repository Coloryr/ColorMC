using ColorMC.Core.Objs;
using System;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 文件显示
/// </summary>
public record FileItemObj
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name;
    /// <summary>
    /// 介绍
    /// </summary>
    public string Summary;
    /// <summary>
    /// 作者
    /// </summary>
    public string Author;
    /// <summary>
    /// 下载次数
    /// </summary>
    public long DownloadCount;
    /// <summary>
    /// 更新日期
    /// </summary>
    public string ModifiedDate;
    /// <summary>
    /// 图标网址
    /// </summary>
    public string? Logo;
    /// <summary>
    /// 是否已经下载
    /// </summary>
    public bool IsDownload;

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
    /// 数据
    /// </summary>
    public object Data;
    public object? Data1;
}

