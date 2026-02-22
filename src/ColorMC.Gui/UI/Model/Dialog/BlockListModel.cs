using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Model.Dialog;

public class BlockListModel(string window) : BaseDialogModel(window)
{
    public string Text { get; set; }

    public ObservableCollection<BlockItemModel> Blocks { get; init; } = [];
}
