using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Core.Objs;

public record ModExportObj
{
    /// <summary>
    /// 校验
    /// </summary>
    public string Sha1 { get; set; }
    /// <summary>
    /// 校验
    /// </summary>
    public string Sha512 { get; set; }
    /// <summary>
    /// 下载地址
    /// </summary>
    public string Url { get; set; }
    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 打包类型
    /// </summary>
    public PackType Type { get; set; }
    /// <summary>
    /// 模组数据
    /// </summary>
    public ModInfoObj? Obj1 { get; set; }
    /// <summary>
    /// 模组数据
    /// </summary>
    public ModObj Obj { get; set; }

    public bool Export { get; set; }
    public string? PID { get; set; }
    public string? FID { get; set; }
    public SourceType? Source { get; set; }
}

public record ModExport1Obj
{
    /// <summary>
    /// 校验
    /// </summary>
    public string Sha1 { get; set; }
    /// <summary>
    /// 校验
    /// </summary>
    public string Sha512 { get; set; }
    /// <summary>
    /// 下载网址
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string Path { get; set; }
    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; set; }
    /// <summary>
    /// 打包类型
    /// </summary>
    public PackType Type;
}