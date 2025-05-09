using System.Collections.Generic;
using Avalonia.Input;

namespace ColorMC.Gui.Objs.Config;

/// <summary>
/// 按钮设置
/// </summary>
public record InputKeyObj
{
    /// <summary>
    /// 绑定的按钮
    /// </summary>
    public Key Key { get; set; }
    /// <summary>
    /// 绑定的按钮
    /// </summary>
    public KeyModifiers KeyModifiers { get; set; }
    /// <summary>
    /// 绑定的按钮
    /// </summary>
    public MouseButton MouseButton { get; set; }
}

/// <summary>
/// 摇杆设置
/// </summary>
public record InputAxisObj : InputKeyObj
{
    /// <summary>
    /// 输入的摇杆
    /// </summary>
    public byte InputKey { get; set; }
    /// <summary>
    /// 死区开始
    /// </summary>
    public short Start { get; set; }
    /// <summary>
    /// 死区结束
    /// </summary>
    public short End { get; set; }
    /// <summary>
    /// 回弹取消
    /// </summary>
    public bool BackCancel { get; set; }

    public InputAxisObj() { }

    public InputAxisObj(InputKeyObj obj)
    {
        Key = obj.Key;
        KeyModifiers = obj.KeyModifiers;
        MouseButton = obj.MouseButton;
    }
}

/// <summary>
/// 手柄配置
/// </summary>
public record InputControlObj
{
    public string Name { get; set; }
    public string UUID { get; set; }
    public int RotateAxis { get; set; }
    public int RotateDeath { get; set; }
    public float RotateRate { get; set; }
    public bool ItemCycle { get; set; }
    public byte ItemCycleLeft { get; set; }
    public byte ItemCycleRight { get; set; }
    public int CursorAxis { get; set; }
    public int CursorDeath { get; set; }
    public float CursorRate { get; set; }
    public float DownRate { get; set; }
    public int ToBackValue { get; set; }
    public Dictionary<byte, InputKeyObj> Keys { get; set; }
    public Dictionary<string, InputAxisObj> AxisKeys { get; set; }
}

