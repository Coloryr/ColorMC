using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.Utils.LaunchSetting;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class InputAxisButtonModel(SettingModel setting) 
    : InputButtonModel(setting)
{
    [ObservableProperty]
    private short? _start;
    [ObservableProperty]
    private short? _end;

    [ObservableProperty]
    private short _nowValue;

    [ObservableProperty]
    private bool _backCancel;

    public new string Icon => IconConverter.GetInputAxisIcon(InputKey);

    public string UUID;

    private bool _changeStart;

    partial void OnBackCancelChanged(bool value)
    {
        setting.InputSave(this);
    }

    partial void OnStartChanged(short? value)
    {
        if (_changeStart)
        {
            return;
        }
        _changeStart = true;

        if (value == null)
        {
            Start = 0;
        }

        _changeStart = false;

        setting.InputSave(this);
    }

    partial void OnEndChanged(short? value)
    {
        if (_changeStart)
        {
            return;
        }
        _changeStart = true;

        if (value == null)
        {
            End = short.MaxValue;
        }

        _changeStart = false;

        setting.InputSave(this);
    }

    public InputAxisObj GenObj()
    {
        return new(Obj)
        {
            BackCancel = BackCancel,
            Start = Start == null ? (short)0 : (short)Start,
            End = End == null ? short.MaxValue : (short)End,
            InputKey = InputKey
        };
    }
}
