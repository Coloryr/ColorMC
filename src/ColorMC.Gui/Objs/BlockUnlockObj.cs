using System;
using System.Collections.Generic;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 方块背包
/// </summary>
public class BlockUnlockObj
{
    /// <summary>
    /// 解锁的方块列表
    /// </summary>
    public HashSet<string> List { get; set; }
    /// <summary>
    /// 今日幸运方块
    /// </summary>
    public string Today { get; set; }
    /// <summary>
    /// 获取时间
    /// </summary>
    public DateTime Time { get; set; }
}
