using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    [RelayCommand]
    public async Task MakeEnable()
    {
        if (Enable)
        {
            return;
        }

        Progress("启用云同步中");
        var res = await WebBinding.StartCloud(Obj);
        ProgressClose();
        if (res == null)
        {
            ShowOk("云服务器错误", Window.Close);
            return;
        }
        if (res.Value == AddSaveState.Exist)
        {
            Show("游戏实例已经启用同步了");
            return;
        }
        else if (res.Value == AddSaveState.Error)
        {
            Show("游戏实例启用同步错误");
            return;
        }

        Notify("同步已启用");
        Enable = true;
    }

    [RelayCommand]
    public async Task MakeDisable()
    {
        if (!Enable)
        {
            return;
        }

        var ok = await ShowWait("关闭云同步会删除服务器上的所有东西，是否继续");
        if (!ok)
        {
            return;
        }

        Progress("关闭云同步中");
        var res = await WebBinding.StopCloud(Obj);
        ProgressClose();
        if (res == null)
        {
            ShowOk("云服务器错误", Window.Close);
            return;
        }
        if (!res.Value)
        {
            Show("云同步关闭失败");
            return;
        }
         
        Notify("同步已关闭");
        Enable = false;
    }

    public async void Load()
    {
        if (!GameCloudUtils.Connect)
        {
            ShowOk("云服务器未链接", Window.Close);
            return;
        }
        Progress("检查云同步中");
        var res = await WebBinding.CheckCloud(Obj);
        ProgressClose();
        if (res == null)
        {
            ShowOk("云服务器错误", Window.Close);
            return;
        }
        Enable = (bool)res;
    }
}
