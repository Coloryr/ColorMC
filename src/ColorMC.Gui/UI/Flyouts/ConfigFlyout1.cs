using Avalonia.Controls;
using Avalonia.Controls.Selection;
using ColorMC.Core.Chunk;
using ColorMC.Core.Nbt;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.GameConfigEdit;
using ColorMC.Gui.UI.Model.Items;

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

        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("Button.Add"), add, () =>
            {
                _model.AddItem(_item!);
            }),
            new FlyoutMenuObj(App.Lang("Button.Delete"), delete, ()=>
            {
                if (_item == null)
                {
                    _model.Delete(_list.SelectedItems);
                }
                else
                {
                    _model.Delete(_item!);
                }
            }),
            new FlyoutMenuObj(App.Lang("ConfigEditWindow.Flyouts.Text3"), editKey, () =>
            {
                _model.SetKey(_item!);
            }),
            new FlyoutMenuObj(App.Lang("ConfigEditWindow.Flyouts.Text4"), editValue, () =>
            {
                _model.SetValue(_item!);
            }),
            new FlyoutMenuObj(App.Lang("ConfigEditWindow.Flyouts.Text5"), true, _model.Find),
        ], con);
    }
}
