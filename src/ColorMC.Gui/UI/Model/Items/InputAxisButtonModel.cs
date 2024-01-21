using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.Utils.LaunchSetting;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class InputAxisButtonModel(SettingModel setting) : InputButtonModel(setting)
{
    [ObservableProperty]
    private short? _start;
    [ObservableProperty]
    private short? _end;

    [ObservableProperty]
    private short _nowValue;

    public new string Icon => IconConverter.GetInputAxisIcon(InputKey);

    public string UUID;

    public InputAxisObj GenoObj()
    {
        return new(Obj)
        {
            Start = Start == null ? (short)0 : (short)Start,
            End = End == null ? short.MaxValue : (short)End,
            InputKey = InputKey,
        };
    }
}
