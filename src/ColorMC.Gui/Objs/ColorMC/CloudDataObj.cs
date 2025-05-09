using System;
using System.Collections.Generic;

namespace ColorMC.Gui.Objs.ColorMC;

/// <summary>
/// 云同步配置储存
/// </summary>
public record CloudDataObj
{
    /// <summary>
    /// 配置文件更新时间
    /// </summary>
    public DateTime ConfigTime { get; set; }
    /// <summary>
    /// 配置列表
    /// </summary>
    public List<string> Config { get; set; }
}

/// <summary>
/// 云游戏实例
/// </summary>
public record CloundListObj
{
    /// <summary>
    /// 游戏实例UUID
    /// </summary>
    public string UUID { get; set; }
    /// <summary>
    /// 游戏实例名字
    /// </summary>
    public string Name { get; set; }
}

/// <summary>
/// 云存档
/// </summary>
public record CloudWorldObj
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 最后游玩时间
    /// </summary>
    public string Time { get; set; }
    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }
    /// <summary>
    /// 文件列表
    /// </summary>
    public Dictionary<string, string> Files { get; set; }
}
