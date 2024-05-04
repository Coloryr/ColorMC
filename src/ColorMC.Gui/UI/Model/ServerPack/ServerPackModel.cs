﻿using System.Collections.Generic;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackModel : MenuModel
{
    public ServerPackObj Obj { get; }

    public override List<MenuObj> TabItems { get; init; } =
    [
        new() { Icon = "/Resource/Icon/GameExport/item1.svg",
            Text = App.Lang("ServerPackWindow.Tabs.Text1") },
        new() { Icon = "/Resource/Icon/GameExport/item2.svg",
            Text = App.Lang("ServerPackWindow.Tabs.Text2") },
        new() { Icon = "/Resource/Icon/GameExport/item3.svg",
            Text = App.Lang("ServerPackWindow.Tabs.Text3") },
        new() { Icon = "/Resource/Icon/GameExport/item4.svg",
            Text = App.Lang("ServerPackWindow.Tabs.Text4") },
    ];

    private readonly string _name;

    public ServerPackModel(BaseModel model, ServerPackObj obj) : base(model)
    {
        _name = ToString() ?? "ServerPackModel";

        Obj = obj;
    }

    public async void Gen()
    {
        var local = await PathBinding.SelectPath(PathType.ServerPackPath);
        if (local == null)
            return;

        Obj.Text = Text;

        Model.Progress(App.Lang("ServerPackWindow.Tab1.Info1"));
        var res = await GameBinding.GenServerPack(Obj, local, Model.ShowWait);
        Model.ProgressClose();
        if (res)
        {
            Model.Notify(App.Lang("ServerPackWindow.Tab1.Info2"));
        }
        else
        {
            Model.Show(App.Lang("ServerPackWindow.Tab1.Error3"));
        }
    }

    public void RemoveChoise()
    {
        Model.RemoveChoiseData(_name);
    }

    protected override void Close()
    {
        ModList.Clear();
        ConfigList.Clear();
        NameList.Clear();
    }
}
