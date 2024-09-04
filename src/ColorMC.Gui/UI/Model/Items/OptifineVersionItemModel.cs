using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Objs.OptiFine;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Model.Items;

public partial class OptifineVersionItemModel(OptifineObj obj) : SelectItemModel
{
    public IAddOptifineWindow? Add { get; set; }

    public string Version => obj.Version;
    public string MCVersion => obj.MCVersion;
    public string Forge => obj.Forge;
    public string Date => obj.Date;

    public OptifineObj Obj => obj;

    public void SetSelect()
    {
        Add?.SetSelect(this);
    }

    public void Install()
    {
        Add?.Install(this);
    }
}
