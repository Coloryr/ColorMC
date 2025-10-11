using System.Collections.Generic;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加游戏实例
/// 云同步下载
/// </summary>
public partial class AddGameModel
{
    /// <summary>
    /// 下载云同步游戏实例
    /// </summary>
    /// <returns></returns>
    public async void GameCloudDownload()
    {
        Model.Progress(App.Lang("AddGameWindow.Tab1.Info9"));
        var list = await ColorMCCloudAPI.GetListAsync();
        Model.ProgressClose();
        if (list == null)
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error9"));
            return;
        }
        var list1 = new List<string>();
        list.ForEach(item =>
        {
            if (!string.IsNullOrEmpty(item.Name) && GameBinding.GetGame(item.UUID) == null)
            {
                list1.Add(item.Name);
            }
        });
        var res = await Model.Combo(App.Lang("AddGameWindow.Tab1.Info10"), list1);
        if (res.Cancel)
        {
            return;
        }

        Model.Progress(App.Lang("AddGameWindow.Tab1.Info11"));
        var obj = list[res.Index];
        while (true)
        {
            //替换冲突的名字
            if (GameBinding.GetGameByName(obj.Name) != null)
            {
                var res1 = await Model.ShowAsync(App.Lang("AddGameWindow.Tab1.Info12"));
                if (!res1)
                {
                    Model.ProgressClose();
                    return;
                }
                var res2 = await Model.Input(App.Lang("AddGameWindow.Tab1.Text2"), obj.Name);
                if (res2.Cancel)
                {
                    return;
                }

                obj.Name = res2.Text1!;
            }
            else
            {
                break;
            }
        }
        //下载游戏实例
        var res3 = await GameBinding.DownloadCloudAsync(obj, Group, Model.ShowAsync,
            Tab1GameOverwirte);
        Model.ProgressClose();
        if (!res3.State)
        {
            Model.Show(res3.Data!);
            return;
        }

        WindowManager.ShowGameCloud(GameBinding.GetGame(obj.UUID!)!);
        Done(res3.Data);
    }

    /// <summary>
    /// 下载服务器实例
    /// </summary>
    public async void ServerPackDownload()
    {
        var res = await Model.InputWithEditAsync(App.Lang("AddGameWindow.Tab1.Info13"), false);
        if (res.Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(res.Text1))
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error14"));
            return;
        }

        if (!res.Text1.EndsWith('/'))
        {
            res.Text1 += '/';
        }
        //下载服务器包
        Model.Progress(App.Lang("AddGameWindow.Tab1.Info14"));
        var res1 = await GameBinding.DownloadServerPackAsync(Model, Name, Group, res.Text1,
            Tab1GameOverwirte);
        Model.ProgressClose();
        if (!res1.State)
        {
            if (res1.Data != null)
            {
                Model.Show(res1.Data!);
            }
        }
        else
        {
            Done(res1.Data!);
        }
    }
}
