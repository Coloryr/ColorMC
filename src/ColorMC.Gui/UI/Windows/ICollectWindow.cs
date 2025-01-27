using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Windows;

public interface ICollectWindow
{
    public void SetSelect(CollectItemModel item);
    public void Install(CollectItemModel item);
    public void Install();
    public bool HaveSelect();
}