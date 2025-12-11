using System;
using System.Linq;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddModPackControlModel
{
    /// <summary>
    /// 安装所选项目
    /// </summary>
    /// <param name="item"></param>
    public override async void Install(FileItemModel item)
    {
        SetSelect(item);
        var dialog = Window.ShowProgress(LangUtils.Get("AddModPackWindow.Text19"));
        var res = await WebBinding.GetFileListAsync(item.Obj.Source,
               item.Obj.Pid, 0, null, Loaders.Normal);
        Window.CloseDialog(dialog);
        if (res == null || res.List == null || res.List.Count == 0)
        {
            Window.Show(LangUtils.Get("AddModPackWindow.Text39"));
            return;
        }

        var item1 = res.List.First();
        if (item1.IsDownload)
        {
            var res1 = await Window.ShowChoice(LangUtils.Get("AddModPackWindow.Text40"));
            if (!res1)
            {
                return;
            }
        }

        Install(item1);
    }

    /// <summary>
    /// 安装所选文件
    /// </summary>
    /// <param name="data"></param>
    public override async void Install(FileVersionItemModel data)
    {
        if (data.IsDownload)
        {
            return;
        }

        if (GameManager.IsDownload(data.Obj))
        {
            Window.Show(LangUtils.Get("AddModPackWindow.Text46"));
            return;
        }

        GameRes res;
        FileItemDownloadModel info;

        var gui = new OverGameGui(Window);

        GameManager.StartDownload(data.Obj);
        var select = _lastSelect;
        if (data.Obj.Source == SourceType.CurseForge)
        {
            var data1 = (data.Data as CurseForgeModObj.CurseForgeDataObj)!;
            info = new FileItemDownloadModel
            {
                Window = Window,
                Name = data1.DisplayName
            };
            AddDownload(info);
            var pack = new ModPackGui(info);
            res = await AddGameHelper.InstallCurseForge(_group, data1, select?.Logo, gui, pack, info.Token);
            pack.Stop();
            RemoveDownload(info);
        }
        else if (data.Obj.Source == SourceType.Modrinth)
        {
            var data1 = (data.Data as ModrinthVersionObj)!;
            info = new FileItemDownloadModel
            {
                Window = Window,
                Name = data1.Name
            };
            AddDownload(info);
            var pack = new ModPackGui(info);
            res = await AddGameHelper.InstallModrinth(_group, data1, select?.Logo, gui, pack);
            pack.Stop();
            RemoveDownload(info);
        }
        else
        {
            throw new InvalidOperationException();
        }

        GameManager.StopDownload(data.Obj, res.State);

        if (!info.Token.IsCancellationRequested)
        {
            if (!res.State)
            {
                Window.Show(LangUtils.Get("AddGameWindow.Tab1.Text50"));
            }
            else
            {
                Done(res.Game!.UUID);
            }
        }
    }
}
