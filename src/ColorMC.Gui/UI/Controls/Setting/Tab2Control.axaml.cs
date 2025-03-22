using Avalonia.Controls;

namespace ColorMC.Gui.UI.Controls.Setting;

/// <summary>
/// ���������ô���
/// </summary>
public partial class Tab2Control : UserControl
{
    public Tab2Control()
    {
        InitializeComponent();

#if Phone
        if (SystemInfo.Os == OsType.Android)
        {
            var con = ColorMCGui.PhoneGetSetting?.Invoke();
            if (con is Control con1)
            {
                PhoneSetting.Children.Add(con1);
            }
        }
#endif
    }
}
