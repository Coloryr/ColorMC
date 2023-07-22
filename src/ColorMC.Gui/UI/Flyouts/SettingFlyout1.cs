using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.UIBinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ColorMC.Gui.UI.Flyouts;

public class SettingFlyout1
{
    private readonly IEnumerable<JavaDisplayObj> _list;
    private readonly SettingTab5Model _model;
    public SettingFlyout1(Control con, SettingTab5Model model, IList list)
    {
        _model = model;
        _list = list.Cast<JavaDisplayObj>();

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("SettingWindow.Flyouts.Text1"), true, Button1_Click),
        }, con);
    }

    private void Button1_Click()
    {
        foreach (var item in _list)
        {
            JavaBinding.RemoveJava(item.Name);
        }

        _model.Load();
    }
}
