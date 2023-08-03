using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameCloud;

public partial class GameCloudModel : GameEditModel
{
    [ObservableProperty]
    private bool _enable;

    public string UUID => Obj.UUID;

    public GameCloudModel(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    partial void OnEnableChanged(bool value)
    {

    }

    public void Load()
    {
        if (!GameCloudUtils.Connect)
        {
            Window.Close();
        }


    }
}
