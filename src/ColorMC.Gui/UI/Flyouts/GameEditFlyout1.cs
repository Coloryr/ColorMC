﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout1
{
    public GameEditFlyout1(Control con, IList list, GameEditModel model)
    {
        ModDisplayModel obj = null!;
        bool single = false;
        IEnumerable<ModDisplayModel> mods;
        mods = list.Cast<ModDisplayModel>();
        if (mods.Count() == 1)
        {
            single = true;
            obj = mods.ToList()[0];
        }

        _ = new FlyoutsControl(
        [
            (App.Lang("GameEditWindow.Flyouts1.Text1"), true, () =>
            {
                if (single)
                {
                    model.DisEMod(obj);
                }
                else
                {
                    foreach (var item in mods)
                    {
                        model.DisEMod(item);
                    }
                }
            }),
            (App.Lang("GameEditWindow.Flyouts1.Text2"), true, ()=>
            {
                if (single)
                {
                    model.DeleteMod(obj);
                }
                else
                {
                    model.DeleteMod(mods);
                }
            }),
            (App.Lang("Button.OpFile"), single, ()=>
            {
                PathBinding.OpFile(obj.Local);
            }),
            (App.Lang("GameEditWindow.Flyouts1.Text6"), true, async ()=>
            {
                var list = new List<IStorageFile>();
                if (App.TopLevel is { } top)
                {
                    foreach (var item in mods)
                    {
                        var data = await top.StorageProvider.TryGetFileFromPathAsync(item.Local);
                        if (data == null)
                            continue;

                        list.Add(data);
                    }
                    await BaseBinding.CopyFileClipboard(list);
                }
            }),
            (App.Lang("GameEditWindow.Flyouts1.Text3"), single, ()=>
            {
                WebBinding.OpenMcmod(obj);
            }),
            (App.Lang("GameEditWindow.Flyouts1.Text4"), single
                && !string.IsNullOrWhiteSpace(obj?.Url), ()=>
                {
                    BaseBinding.OpUrl(obj!.Url);
                }),
            (App.Lang("GameEditWindow.Flyouts1.Text5"), single
                && !string.IsNullOrWhiteSpace(obj?.PID)
                && !string.IsNullOrWhiteSpace(obj?.FID), ()=>
                {
                    App.ShowAdd(obj!.Obj.Game, obj);
                }),
        ], con);
    }
}
