using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 游戏实例
/// 模组右键菜单
/// </summary>
public static class GameEditFlyout1
{
    public static void Show(Control con, IReadOnlyList<object?> list, GameEditModel model)
    {
        ModNodeModel obj = null!;
        bool single = false;
        var mods = list.Cast<ModNodeModel>();
        if (mods.Count() == 1)
        {
            var item = mods.First();
            if (item.IsGroup)
            {
                if (item.Children.Count == 0)
                {
                    return;
                }
                else if (item.Children.Count == 1)
                {
                    single = true;
                    obj = item.Children.First();
                }
                else
                {
                    mods = item.Children;
                }
            }
            else
            {
                single = true;
                obj = item;
            }
        }
        else
        {
            bool group = false;
            bool isitem = false;
            foreach (var item in mods)
            {
                if (item.IsGroup)
                {
                    group = true;
                    continue;
                }
                else
                {
                    isitem = true;
                }

                if (group && isitem)
                {
                    break;
                }
            }

            mods = [.. mods.Where(item => !item.IsGroup)];
        }

        new FlyoutsControl(
        [
            new FlyoutMenuModel(LangUtils.Get("GameEditWindow.Flyouts.Text1"), true, () =>
            {
                if (single)
                {
                    model.DisableEnableMod(obj);
                }
                else
                {
                    model.DisableEnableMod(mods);
                }

                model.ModTreeUpdate();
            }),
            new FlyoutMenuModel(LangUtils.Get("GameEditWindow.Flyouts.Text2"), true, () =>
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
            new FlyoutMenuModel(LangUtils.Get("Button.OpFile"), single, () =>
            {
                PathBinding.OpenFileWithExplorer(obj.Local);
            }),
            new FlyoutMenuModel(LangUtils.Get("GameEditWindow.Flyouts.Text6"), true, async() =>
            {
                var list = new List <IStorageFile>();
                if(TopLevel.GetTopLevel(con) is { } top)
                {
                    if (single)
                    {
                        var data = await top.StorageProvider.TryGetFileFromPathAsync(obj.Local);
                        if (data != null)
                        {
                            list.Add(data);
                        }
                    }
                    foreach (var item in mods)
                    {
                        if (item.IsGroup)
                        {
                            continue;
                        }
                        var data = await top.StorageProvider.TryGetFileFromPathAsync(item.Local);
                        if (data == null)
                        {
                            continue;
                        }
                        list.Add(data);
                    }
                    await BaseBinding.CopyFileClipboardAsync(top, list);
                }
            }),
            new FlyoutMenuModel(LangUtils.Get("GameEditWindow.Flyouts.Text3"), single, () =>
            {
                WebBinding.OpenMcmod(obj);
            }),
            new FlyoutMenuModel(LangUtils.Get("GameEditWindow.Flyouts.Text15"), true, () =>
            {
                model.SetText(mods);
            }),
            new FlyoutMenuModel(LangUtils.Get("GameEditWindow.Flyouts.Text16"), single, () =>
            {
                model.SetProjectId(obj);
            }),
            new FlyoutMenuModel(LangUtils.Get("GameEditWindow.Flyouts.Text17"), true, () =>
            {
                model.SetGroup(mods);
            }),
            new FlyoutMenuModel(LangUtils.Get("GameEditWindow.Flyouts.Text4"), single
                && !string.IsNullOrWhiteSpace(obj ?.Url), () =>
                {
                    BaseBinding.OpenUrl(obj !.Url);
                }),
            new FlyoutMenuModel(LangUtils.Get("GameEditWindow.Flyouts.Text5"), single
                && !string.IsNullOrWhiteSpace(obj?.PID) && ! string.IsNullOrWhiteSpace(obj?.FID), () =>
                {
                    WindowManager.ShowAdd(obj!.Obj.Game, obj);
                }),
        ]).Show(con);
    }
}
