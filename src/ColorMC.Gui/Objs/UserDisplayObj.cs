using ColorMC.Core.Objs;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 账户显示
/// </summary>
public record UserDisplayObj
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Use { get; set; }
    /// <summary>
    /// 用户名
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// UUID
    /// </summary>
    public string UUID { get; set; }
    /// <summary>
    /// 类型
    /// </summary>
    public string Type { get; set; }
    /// <summary>
    /// 附加信息
    /// </summary>
    public string Text1 { get; set; }
    /// <summary>
    /// 附加信息
    /// </summary>
    public string Text2 { get; set; }

    public AuthType AuthType;
}