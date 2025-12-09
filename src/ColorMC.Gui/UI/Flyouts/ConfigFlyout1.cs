using Avalonia.Controls;
using Avalonia.Controls.Selection;
using ColorMC.Core.Chunk;
using ColorMC.Core.Nbt;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameConfigEdit;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 配置文件编辑页面
/// Nbt标签右键菜单
/// </summary>
public static class ConfigFlyout1
{
    public static void Show(Control con, ITreeDataGridSelection list, GameConfigEditModel model)
    {
        var _list = (list as TreeDataGridRowSelectionModel<NbtNodeModel>)!;

        if (_list.Count == 0)
            return;

        bool delete = false, add = false, editKey = false, editValue = false;

        NbtNodeModel? item = null;
        if (_list.Count == 1 && _list.SelectedItem != null)
        {
            item = _list.SelectedItem;

            add = item.NbtType switch
            {
                NbtType.NbtList => true,
                NbtType.NbtCompound => true,
                _ => false
            };

            if (item.Parent != null)
            {
                delete = true;
                editValue = item.NbtType switch
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

            if (item.Parent?.Nbt is ChunkNbt
                && item.Key is "xPos" or "yPos" or "zPos")
            {
                editKey = false;
                delete = false;
                editValue = false;
            }
            else
            {
                editKey = !string.IsNullOrWhiteSpace(item.Key);
            }
        }
        else
        {
            add = false;
            editKey = false;
            foreach (var item1 in _list.SelectedItems)
            {
                if (item1?.Parent != null)
                {
                    delete = true;
                    break;
                }
            }
        }

        new FlyoutsControl(
        [
            new FlyoutMenuModel(LangUtils.Get("Button.Add"), add, () =>
            {
                model.AddItem(item);
            }),
            new FlyoutMenuModel(LangUtils.Get("Button.Delete"), delete, ()=>
            {
                if (item == null)
                {
                    model.Delete(_list.SelectedItems);
                }
                else
                {
                    model.Delete(item!);
                }
            }),
            new FlyoutMenuModel(LangUtils.Get("ConfigEditWindow.Flyouts.Text3"), editKey, () =>
            {
                model.SetKey(item);
            }),
            new FlyoutMenuModel(LangUtils.Get("ConfigEditWindow.Flyouts.Text4"), editValue, () =>
            {
                model.SetValue(item);
            }),
            new FlyoutMenuModel(LangUtils.Get("ConfigEditWindow.Flyouts.Text5"), true, model.Find),
        ]).Show(con);
    }
}
