using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 游戏实例
/// 模组右键菜单
/// </summary>
public static class GameEditFlyout1
{
    public static void Show(Control con, IList list, GameEditModel model)
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

        new FlyoutsControl(
        [
            new FlyoutMenuModel(LanguageUtils.Get("GameEditWindow.Flyouts.Text1"), true, () =>
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
            new FlyoutMenuModel(LanguageUtils.Get("GameEditWindow.Flyouts.Text2"), true, () =>
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
            new FlyoutMenuModel(LanguageUtils.Get("Button.OpFile"), single, () =>
            {
                PathBinding.OpenFileWithExplorer(obj.Local);
            }),
            new FlyoutMenuModel(LanguageUtils.Get("GameEditWindow.Flyouts.Text6"), true, async() =>
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
                    await BaseBinding.CopyFileClipboardAsync(top, list);
                }
            }),
            new FlyoutMenuModel(LanguageUtils.Get("GameEditWindow.Flyouts.Text3"), single, () =>
            {
                WebBinding.OpenMcmod(obj);
            }),
            new FlyoutMenuModel(LanguageUtils.Get("GameEditWindow.Flyouts.Text4"), single
                && ! string.IsNullOrWhiteSpace(obj ?.Url), () =>
                {
                    BaseBinding.OpenUrl(obj !.Url);
                }),
            new FlyoutMenuModel(LanguageUtils.Get("GameEditWindow.Flyouts.Text5"), single
                && ! string.IsNullOrWhiteSpace(obj ?.PID) && ! string.IsNullOrWhiteSpace(obj ?.FID), () =>
                {
                    WindowManager.ShowAdd(obj!.Obj.Game, obj);
                }),
        ]).Show(con);
    }
}
