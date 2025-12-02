using System.Linq;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
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
        var dialog = Window.ShowProgress(LanguageUtils.Get("AddModPackWindow.Text19"));
        var res = await WebBinding.GetFileListAsync(item.SourceType,
               item.Pid, 0, null, Loaders.Normal);
        Window.CloseDialog(dialog);
        if (res == null || res.List == null || res.List.Count == 0)
        {
            Window.Show(LanguageUtils.Get("AddModPackWindow.Text39"));
            return;
        }
        var item1 = res.List.First();
        if (item1.IsDownload)
        {
            var res1 = await Window.ShowChoice(LanguageUtils.Get("AddModPackWindow.Text40"));
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

        var select = _lastSelect;
        if (data.SourceType == SourceType.CurseForge)
        {
            var data1 = (data.Data as CurseForgeModObj.CurseForgeDataObj)!;
            var info = new FileItemDownloadModel(Window)
            {
                Name = data1.DisplayName,
                Source = data.SourceType,
                PID = data1.Id.ToString(),
                Type = FileType.ModPack
            };
            StartDownload(info);
            var gui = new OverGameGui(Window);
            var pack = new ModpackGui(info);
            var res = await AddGameHelper.InstallCurseForge(_group, data1, select?.Logo, gui, pack, info.Token);
            pack.Stop();
            StopDownload(info, res.State);
            if (info.Token.IsCancellationRequested)
            {
                return;
            }

            if (!res.State)
            {
                Window.Show(LanguageUtils.Get("AddGameWindow.Tab1.Text50"));
            }
            else
            {
                Done(res.Game!.UUID);
            }
        }
        else if (data.SourceType == SourceType.Modrinth)
        {
            var data1 = (data.Data as ModrinthVersionObj)!;
            var info = new FileItemDownloadModel(Window)
            {
                Name = data1.Name,
                Source = data.SourceType,
                PID = data1.Id,
                Type = FileType.ModPack
            };
            StartDownload(info);
            var pack = new ModpackGui(info);
            var gui = new OverGameGui(Window);
            var res = await AddGameHelper.InstallModrinth(_group, data1, select?.Logo, gui, pack);
            pack.Stop();
            StopDownload(info, res.State);
            if (!res.State)
            {
                Window.Show(LanguageUtils.Get("AddGameWindow.Tab1.Text50"));
            }
            else
            {
                Done(res.Game!.UUID);
            }
        }
    }
}
