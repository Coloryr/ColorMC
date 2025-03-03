using ColorMC.Gui.Joystick;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Event = Silk.NET.SDL.Event;
using EventType = Silk.NET.SDL.EventType;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class JoystickSettingModel : ObservableObject
{
    public ObservableCollection<string> Controls { get; init; } = [];

    public ObservableCollection<string> Configs { get; set; } = [];

    [ObservableProperty]
    private int _controlIndex = -1;
    [ObservableProperty]
    private int _configUUID = -1;

    private readonly List<string> UUIDs = [];

    public Action TopConfirm, TopCancel;

    public JoystickSettingModel(int now, string? config)
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

    [RelayCommand]
    public void Confirm()
    {
        TopConfirm();
    }

    [RelayCommand]
    public void Cancel()
    {
        TopCancel();
    }

    public string GetUUID()
    {
        return UUIDs[ConfigUUID];
    }

    private void Event(Event sdlEvent)
    {
        EventType type = (EventType)sdlEvent.Type;
        if (type is EventType.Controllerdeviceadded
            or EventType.Controllerdeviceremoved)
        {
            Load();
            return;
        }
    }

    private void Load()
    {
        Controls.Clear();
        foreach (var item in JoystickInput.GetNames())
        {
            Controls.Add(item);
        }
    }

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
