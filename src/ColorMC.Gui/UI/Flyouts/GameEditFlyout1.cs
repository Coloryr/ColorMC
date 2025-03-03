using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
            new FlyoutMenuObj(App.Lang("GameEditWindow.Flyouts.Text1"), true, () =>
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
            new FlyoutMenuObj(App.Lang("GameEditWindow.Flyouts.Text2"), true, () =>
            {
                if(single)
                {
                    model.DeleteMod(obj);
                }
                else
                {
                    model.DeleteMod(mods);
                }
            }),
            new FlyoutMenuObj(App.Lang("Button.OpFile"), single, () =>
            {
                PathBinding.OpenFileWithExplorer(obj.Local);
            }),
            new FlyoutMenuObj(App.Lang("GameEditWindow.Flyouts.Text6"), true, async() =>
            {
                var list = new List <IStorageFile>();
                if(TopLevel.GetTopLevel(con) is { } top)
                {
                    foreach(var item in mods)
                    {
                        var data = await top.StorageProvider.TryGetFileFromPathAsync(item.Local);
                        if(data == null)
                            continue;
                        list.Add(data);
                    }
                    await BaseBinding.CopyFileClipboard(top, list);
                }
            }),
            new FlyoutMenuObj(App.Lang("GameEditWindow.Flyouts.Text3"), single, () =>
            {
                WebBinding.OpenMcmod(obj);
            }),
            new FlyoutMenuObj(App.Lang("GameEditWindow.Flyouts.Text4"), single
                && ! string.IsNullOrWhiteSpace(obj ?.Url), () =>
                {
                    BaseBinding.OpenUrl(obj !.Url);
                }),
            new FlyoutMenuObj(App.Lang("GameEditWindow.Flyouts.Text5"), single
                && ! string.IsNullOrWhiteSpace(obj ?.PID) && ! string.IsNullOrWhiteSpace(obj ?.FID), () =>
                {
                    WindowManager.ShowAdd(obj!.Obj.Game, obj);
                }),
        ], con);
    }
}
