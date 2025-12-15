using System.Collections.Generic;
using System.Collections.ObjectModel;
using ColorMC.Gui.Joystick;
using CommunityToolkit.Mvvm.ComponentModel;
using Event = Silk.NET.SDL.Event;
using EventType = Silk.NET.SDL.EventType;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 手柄与配置选择
/// </summary>
public partial class JoystickSettingModel : BaseDialogModel
{
    /// <summary>
    /// 手柄列表
    /// </summary>
    public ObservableCollection<string> Controls { get; init; } = [];
    /// <summary>
    /// 配置列表
    /// </summary>
    public ObservableCollection<string> Configs { get; set; } = [];
    /// <summary>
    /// 选择的手柄
    /// </summary>
    [ObservableProperty]
    private int _controlIndex = -1;
    /// <summary>
    /// 选择的配置
    /// </summary>
    [ObservableProperty]
    private int _configUUID = -1;

    /// <summary>
    /// 配置列表
    /// </summary>
    private readonly List<string> UUIDs = [];

    public JoystickSettingModel(string name, int now, string? config) : base(name)
    {
        Load();
        Load1();

        ControlIndex = now;
        if (config == null)
        {
            ConfigUUID = -1;
        }
        else
        {
            ConfigUUID = UUIDs.IndexOf(config);
        }

        JoystickInput.OnEvent += Event;
    }

    /// <summary>
    /// 获取当前选择的UUID
    /// </summary>
    /// <returns></returns>
    public string GetUUID()
    {
        return UUIDs[ConfigUUID];
    }

    /// <summary>
    /// 触发了手柄事件
    /// </summary>
    /// <param name="sdlEvent"></param>
    private void Event(Event sdlEvent)
    {
        EventType type = (EventType)sdlEvent.Type;
        if (type is EventType.Controllerdeviceadded
            or EventType.Controllerdeviceremoved)
        {
            Load();
        }
    }

    /// <summary>
    /// 加载手柄列表
    /// </summary>
    private void Load()
    {
        Controls.Clear();
        foreach (var item in JoystickInput.GetNames())
        {
            Controls.Add(item);
        }
    }

    /// <summary>
    /// 加载配置列表
    /// </summary>
    private void Load1()
    {
        UUIDs.Clear();
        Configs.Clear();
        foreach (var item in JoystickConfig.Configs)
        {
            UUIDs.Add(item.Key);
            Configs.Add(item.Value.Name);
        }
    }
}
