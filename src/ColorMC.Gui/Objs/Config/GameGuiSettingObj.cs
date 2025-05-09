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
    public bool EnableModId { get; set; }
    public bool EnableName { get; set; }
    public bool EnableVersion { get; set; }
    public bool EnableLoader { get; set; }
    public bool EnableSide { get; set; }
    public bool EnableText { get; set; }
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
    /// 备注
    /// </summary>
    public Dictionary<string, string?> ModName { get; set; }
    /// <summary>
    /// 是否标星
    /// </summary>
    public bool IsStar { get; set; }
}
