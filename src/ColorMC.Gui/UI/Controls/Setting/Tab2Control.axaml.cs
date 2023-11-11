using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Setting;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab2Control : UserControl
{
    public Tab2Control()
    {
        InitializeComponent();

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;

        if (SystemInfo.Os == OsType.Android)
        {
            var con = ColorMCGui.PhoneGetSetting?.Invoke();
            if (con is Control con1)
            {
                PhoneSetting.Children.Add(con1);
            }
        }
    }

    public void Reset()
    {
        ScrollViewer1.ScrollToHome();
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is SettingModel model && model.NowView == 0)
        {
            model.WhellChange(e.Delta.Y);
        }
    }
}
