using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Event = Silk.NET.SDL.Event;
using EventType = Silk.NET.SDL.EventType;

namespace ColorMC.Gui.UI.Model;

public partial class ControlSelectModel : ObservableObject
{
    public ObservableCollection<string> Controls { get; init; } =[];

    public ObservableCollection<string> Configs { get; set; } = [];

    [ObservableProperty]
    private int _controlIndex = -1;
    [ObservableProperty]
    private int _configUUID = -1;

    private readonly List<string> UUIDs = [];

    private Action<ControlSelectModel> _confirm;
    private Action _cancel;

    public ControlSelectModel(int now, string? config, Action<ControlSelectModel> confirm, Action cancel)
    {
        _confirm = confirm;
        _cancel = cancel;

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

        InputControlUtils.OnEvent += Event;
    }

    [RelayCommand]
    public void Confirm()
    {
        _confirm(this);
    }

    [RelayCommand]
    public void Cancel()
    {
        _cancel();
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
        foreach (var item in InputControlUtils.GetNames())
        {
            Controls.Add(item);
        }
    }

    private void Load1()
    {
        UUIDs.Clear();
        Configs.Clear();
        foreach (var item in InputConfigUtils.Configs)
        {
            UUIDs.Add(item.Key);
            Configs.Add(item.Value.Name);
        }
    }
}
