using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs;

public record InputKeyObj
{
    public Key Key { get; set; }
    public KeyModifiers KeyModifiers { get; set; }
    public MouseButton MouseButton { get; set; }
}

public record InputAxisObj : InputKeyObj
{
    public byte InputKey { get; set; }
    public short Start { get; set; }
    public short End { get; set; }
    public bool BackCancel { get; set; }

    public InputAxisObj() { }

    public InputAxisObj(InputKeyObj obj)
    {
        Key = obj.Key;
        KeyModifiers = obj.KeyModifiers;
        MouseButton = obj.MouseButton;
    }
}

public record InputControlObj
{
    public string Name { get; set; }
    public string UUID { get; set; }
    public bool Enable { get; set; }
    public int RotateAxis { get; set; }
    public int RotateDeath { get; set; }
    public float RotateRate { get; set; }
    public bool ItemCycle { get; set; }
    public byte ItemCycleLeft { get; set; }
    public byte ItemCycleRight { get; set; }
    public int CursorAxis { get; set; }
    public int CursorDeath { get; set; }
    public float CursorRate { get; set; }
    public int ToBackValue { get; set; }
    public Dictionary<byte, InputKeyObj> Keys { get; set; }
    public Dictionary<string, InputAxisObj> AxisKeys { get; set; }
}

