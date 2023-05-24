using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.ServerPack;

public abstract partial class ServerPackTabModel : ObservableObject
{
    protected IUserControl Con;
    public ServerPackObj Obj { get; }

    public ServerPackTabModel(IUserControl con, ServerPackObj obj)
    {
        Con = con;
        Obj = obj;
    }
}
