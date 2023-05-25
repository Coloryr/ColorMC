using Avalonia.Controls;
using Avalonia.Controls.Selection;
using ColorMC.Gui.UI.Model.ConfigEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Flyouts;

public class ConfigFlyout1
{
    private readonly TreeDataGridRowSelectionModel<NbtNodeModel> List;

    public ConfigFlyout1(Control con, ITreeDataGridSelection list)
    {
        List = (list as TreeDataGridRowSelectionModel<NbtNodeModel>)!;

        bool delete = false, add = false, edit = false;

        if (List.Count == 1)
        {
            add = true;
            edit = true;
            var item = List.SelectedItem!;
            if (item.Top == null)
            {
                delete = true;
            }
        }

        else
        {
            add = false;
            edit = false;
            foreach (var item in List.SelectedItems)
            {
                if (item?.Top != null)
                {
                    delete = false;
                    break;
                }
            }
        }

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("ConfigEditWindow.Flyouts1.Text1"), add, Button1_Click),
            (App.GetLanguage("ConfigEditWindow.Flyouts1.Text2"), delete, Button2_Click),
            (App.GetLanguage("ConfigEditWindow.Flyouts1.Text3"), edit, Button3_Click),
        }, con);
    }

    private void Button1_Click()
    { 
        
    }

    private void Button2_Click()
    {

    }

    private void Button3_Click()
    {

    }
}
