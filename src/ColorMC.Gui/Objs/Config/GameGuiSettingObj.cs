using System.Collections.Generic;

namespace ColorMC.Gui.Objs.Config;

public record GameLogSettingObj
{
    /// <summary>
    /// 自动换行
    /// </summary>
    public bool WordWrap { get; set; }
    /// <summary>
    /// 自动下拉
    /// </summary>
    public bool Auto { get; set; }

    public bool EnableNone { get; set; }
    public bool EnableInfo { get; set; }
    public bool EnableWarn { get; set; }
    public bool EnableError { get; set; }
    public bool EnableDebug { get; set; }
}

public record GameModSettingObj
{
    /// <summary>
    /// 模组分组列表
    /// </summary>
    public Dictionary<string, HashSet<string>> Groups { get; set; }
    /// <summary>
    /// 备注
    /// </summary>
    public Dictionary<string, string?> ModName { get; set; }
}

public record GameGuiSettingObj
{
    /// <summary>
    /// 日志设置
    /// </summary>
    public GameLogSettingObj Log { get; set; }
    /// <summary>
    /// 模组显示设置
    /// </summary>
    public GameModSettingObj Mod { get; set; }
    /// <summary>
    /// 显示方块
    /// </summary>
    public string Block { get; set; }
    /// <summary>
    /// 自动打开日志窗口
    /// </summary>
    public bool LogAutoShow { get; set; }
    /// <summary>
    /// 排列顺序
    /// </summary>
    public int Order { get; set; }
}
