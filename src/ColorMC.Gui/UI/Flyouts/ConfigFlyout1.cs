using Avalonia.Controls;
using Avalonia.Controls.Selection;
using ColorMC.Core.Nbt;
using ColorMC.Gui.UI.Model.ConfigEdit;

namespace ColorMC.Gui.UI.Flyouts;

public class ConfigFlyout1
{
    private readonly TreeDataGridRowSelectionModel<NbtNodeModel> List;
    private readonly ConfigEditModel Model;
    private readonly NbtNodeModel? Item;

    public ConfigFlyout1(Control con, ITreeDataGridSelection list, ConfigEditModel model)
    {
        Model = model;
        List = (list as TreeDataGridRowSelectionModel<NbtNodeModel>)!;

        if (List.Count == 0)
            return;

        bool delete = false, add = false, editKey = false, editValue = false;

        if (List.Count == 1)
        {
            Item = List.SelectedItem!;

            add = Item.NbtType switch
            {
                NbtType.NbtList => true,
                NbtType.NbtCompound => true,
                _ => false
            };

            editKey = !string.IsNullOrWhiteSpace(Item.Key);
            if (Item.Top != null)
            {
                delete = true;
                editValue = Item.NbtType switch
                {
                    NbtType.NbtByte => true,
                    NbtType.NbtShort => true,
                    NbtType.NbtInt => true,
                    NbtType.NbtLong => true,
                    NbtType.NbtFloat => true,
                    NbtType.NbtDouble => true,
                    NbtType.NbtString => true,
                    NbtType.NbtByteArray => true,
                    NbtType.NbtIntArray => true,
                    NbtType.NbtLongArray => true,
                    _ => false
                };
            }
        }
        else
        {
            add = false;
            editKey = false;
            foreach (var item in List.SelectedItems)
            {
                if (item?.Top != null)
                {
                    delete = true;
                    break;
                }
            }
        }

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("ConfigEditWindow.Flyouts1.Text1"), add, Button1_Click),
            (App.GetLanguage("ConfigEditWindow.Flyouts1.Text2"), delete, Button2_Click),
            (App.GetLanguage("ConfigEditWindow.Flyouts1.Text3"), editKey, Button3_Click),
            (App.GetLanguage("ConfigEditWindow.Flyouts1.Text4"), editValue, Button4_Click),
            (App.GetLanguage("ConfigEditWindow.Flyouts1.Text5"), true, Button5_Click),
        }, con);
    }

    private void Button1_Click()
    {
        Model.AddItem(Item!);
    }

    private void Button2_Click()
    {
        if (Item == null)
        {
            Model.Delete(List.SelectedItems);
        }
        else
        {
            Model.Delete(Item!);
        }
    }

    private void Button3_Click()
    {
        Model.SetKey(Item!);
    }

    private void Button4_Click()
    {
        Model.SetValue(Item!);
    }

    private void Button5_Click()
    {
        Model.Find();
    }
}
