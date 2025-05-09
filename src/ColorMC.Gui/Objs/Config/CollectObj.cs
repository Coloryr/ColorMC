using System.Collections.Generic;
using System.Text.Json.Serialization;
using ColorMC.Core.Objs;

namespace ColorMC.Gui.Objs.Config;

/// <summary>
/// 收藏项目
/// </summary>
public record CollectItemObj
{
    [JsonIgnore]
    public string UUID;
    /// <summary>
    /// 下载源
    /// </summary>
    public SourceType Source { get; set; }
    /// <summary>
    /// 资源类型
    /// </summary>
    public FileType FileType { get; set; }
    /// <summary>
    /// 资源名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 项目ID
    /// </summary>
    public string Pid { get; set; }
    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }
    /// <summary>
    /// 网址
    /// </summary>
    public string Url { get; set; }
}

/// <summary>
/// 资源收藏
/// </summary>
public record CollectObj
{
    /// <summary>
    /// 收藏项目列表
    /// </summary>
    public Dictionary<string, CollectItemObj> Items { get; set; }
    /// <summary>
    /// 收藏分组列表
    /// </summary>
    public Dictionary<string, List<string>> Groups { get; set; }

    /// <summary>
    /// 显示模组
    /// </summary>
    public bool Mod { get; set; }
    /// <summary>
    /// 显示资源包
    /// </summary>
    public bool ResourcePack { get; set; }
    /// <summary>
    /// 显示光影包
    /// </summary>
    public bool Shaderpack { get; set; }
}
