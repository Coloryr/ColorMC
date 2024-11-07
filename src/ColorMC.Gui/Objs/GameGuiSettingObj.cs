using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

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
    public Dictionary<string, string> ModName { get; set; }
}
