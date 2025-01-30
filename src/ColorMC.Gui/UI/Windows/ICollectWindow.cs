using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Windows;

public interface ICollectWindow
{
    void SetSelect(CollectItemModel item);
    void Install(CollectItemModel item);
    void Install();
    bool HaveSelect();
    void DeleteSelect();
    void GroupSelect();
    bool HaveGroup();
}