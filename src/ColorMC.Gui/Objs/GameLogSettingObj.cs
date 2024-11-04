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
