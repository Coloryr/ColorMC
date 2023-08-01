using Avalonia.Controls;
using Avalonia.Controls.Selection;
using ColorMC.Core.Chunk;
using ColorMC.Core.Nbt;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameConfigEdit;

namespace ColorMC.Gui.UI.Flyouts;

public class ConfigFlyout1
{
    private readonly TreeDataGridRowSelectionModel<NbtNodeModel> _list;
    private readonly GameConfigEditModel _model;
    private readonly NbtNodeModel? _item;

    public ConfigFlyout1(Control con, ITreeDataGridSelection list, GameConfigEditModel model)
    {
        _model = model;
        _list = (list as TreeDataGridRowSelectionModel<NbtNodeModel>)!;

        if (_list.Count == 0)
            return;

        bool delete = false, add = false, editKey = false, editValue = false;

        if (_list.Count == 1)
        {
            _item = _list.SelectedItem!;

            add = _item.NbtType switch
            {
                NbtType.NbtList => true,
                NbtType.NbtCompound => true,
                _ => false
            };

            if (_item.Top != null)
            {
                delete = true;
                editValue = _item.NbtType switch
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

            if (_item.Top?.Nbt is ChunkNbt
                && _item.Key is "xPos" or "yPos" or "zPos")
            {
                editKey = false;
                delete = false;
                editValue = false;
            }
            else
            {
                editKey = !string.IsNullOrWhiteSpace(_item.Key);
            }
        }
        else
        {
            add = false;
            editKey = false;
            foreach (var item in _list.SelectedItems)
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
        _model.AddItem(_item!);
    }

    private void Button2_Click()
    {
        if (_item == null)
        {
            _model.Delete(_list.SelectedItems);
        }
        else
        {
            _model.Delete(_item!);
        }
    }

    private void Button3_Click()
    {
        _model.SetKey(_item!);
    }

    private void Button4_Click()
    {
        _model.SetValue(_item!);
    }

    private void Button5_Click()
    {
        _model.Find();
    }
}
