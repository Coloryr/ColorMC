using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public abstract partial class GameEditTabModel : ObservableObject
{
    protected IUserControl Con;
    public GameSettingObj Obj { get; init; }

    public GameEditTabModel(IUserControl con, GameSettingObj obj)
    {
        Con = con;
        Obj = obj;
    }
}
