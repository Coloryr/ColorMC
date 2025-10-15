using System;
using System.Collections.Generic;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 方块贴图列表
/// </summary>
public class BlocksObj
{
    /// <summary>
    /// 游戏版本
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// 贴图列表
    /// </summary>
    public Dictionary<string, string> Tex { get; set; }
}
