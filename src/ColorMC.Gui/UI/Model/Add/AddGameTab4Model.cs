﻿using System.Collections.Generic;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddGameModel
{
    /// <summary>
    /// 下载云同步游戏实例
    /// </summary>
    /// <returns></returns>
    public async void GameCloudDownload()
    {
        Model.Progress(App.Lang("AddGameWindow.Tab1.Info9"));
        var list = await GameCloudUtils.GetList();
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
        var res = await Model.ShowCombo(App.Lang("AddGameWindow.Tab1.Info10"), list1);
        if (res.Cancel)
        {
            return;
        }

        Model.Progress(App.Lang("AddGameWindow.Tab1.Info11"));
        var obj = list[res.Index];
        while (true)
        {
            if (GameBinding.GetGameByName(obj.Name) != null)
            {
                var res1 = await Model.ShowWait(App.Lang("AddGameWindow.Tab1.Info12"));
                if (!res1)
                {
                    Model.ProgressClose();
                    return;
                }
                var (Cancel, Text1) = await Model.ShowEdit(App.Lang("AddGameWindow.Tab1.Text2"), obj.Name);
                if (Cancel)
                {
                    return;
                }

                obj.Name = Text1!;
            }
            else
            {
                break;
            }
        }
        var res3 = await GameBinding.DownloadCloud(obj, Group, Model.ShowWait,
            Tab1GameOverwirte);
        Model.ProgressClose();
        if (!res3.State)
        {
            Model.Show(res3.Message!);
            return;
        }

        WindowManager.ShowGameCloud(GameBinding.GetGame(obj.UUID!)!);
        Done(res3.Message);
    }

    /// <summary>
    /// 下载服务器实例
    /// </summary>
    public async void ServerPackDownload()
    {
        var (Cancel, Text) = await Model.ShowInputOne(App.Lang("AddGameWindow.Tab1.Info13"), false);
        if (Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Text))
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error14"));
            return;
        }

        if (!Text.EndsWith('/'))
        {
            Text += '/';
        }

        Model.Progress(App.Lang("AddGameWindow.Tab1.Info14"));
        var res1 = await GameBinding.DownloadServerPack(Model, Name, Group, Text,
            Tab1GameOverwirte);
        Model.ProgressClose();
        if (!res1.State)
        {
            if (res1.Message != null)
            {
                Model.Show(res1.Message!);
            }
        }
        else
        {
            Done(res1.Message!);
        }
    }
}
