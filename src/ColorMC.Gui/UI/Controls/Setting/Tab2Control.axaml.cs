using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Gui.UI.Controls.Setting;

/// <summary>
/// Æô¶¯Æ÷ÉèÖÃ´°¿Ú
/// </summary>
public partial class Tab2Control : UserControl
{
    public Tab2Control()
    {
        InitializeComponent();

        if (SystemInfo.Os == OsType.Android)
        {
            var con = ColorMCGui.PhoneGetSetting?.Invoke();
            if (con is Control con1)
            {
                PhoneSetting.Children.Add(con1);
            }
        }
    }
}
