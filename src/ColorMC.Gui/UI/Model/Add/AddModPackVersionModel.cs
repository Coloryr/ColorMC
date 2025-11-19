using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddModPackControlModel
{
    /// <summary>
    /// 是否显示文件列表
    /// </summary>
    [ObservableProperty]
    private bool _displayVersion = false;

    /// <summary>
    /// 游戏列表显示
    /// </summary>
    /// <param name="value"></param>
    partial void OnDisplayVersionChanged(bool value)
    {
        if (value)
        {
            Model.PushBack(back: () =>
            {
                DisplayVersion = false;
            });
        }
        else
        {
            Model.PopBack();
            Model.Title = LanguageUtils.Get("AddModPackWindow.Title");
        }
    }

    /// <summary>
    /// 安装所选文件
    /// </summary>
    /// <param name="data"></param>
    public async void Install(FileVersionItemModel data)
    {
        if (data.IsDownload)
        {
            return;
        }

        var select = _last;
        if (data.SourceType == SourceType.CurseForge)
        {
            Model.Progress(LanguageUtils.Get("AddGameWindow.Tab1.Text30"));
            var zip = new ZipGui(Model);
            var res = await AddGameHelper.InstallCurseForge(null, _group, (data.Data as CurseForgeModObj.CurseForgeDataObj)!,
                select?.Logo,
                new CreateGameGui(Model), zip);
            zip.Stop();
            Model.ProgressClose();

            if (!res.State)
            {
                Model.Show(LanguageUtils.Get("AddGameWindow.Tab1.Text50"));
            }
            else
            {
                Done(res.Game!.UUID);
            }
        }
        else if (data.SourceType == SourceType.Modrinth)
        {
            Model.Progress(LanguageUtils.Get("AddGameWindow.Tab1.Text30"));
            var zip = new ZipGui(Model);
            var res = await AddGameHelper.InstallModrinth(null, _group, (data.Data as ModrinthVersionObj)!,
                select?.Logo, new CreateGameGui(Model), zip);
            zip.Stop();
            Model.ProgressClose();

            if (!res.State)
            {
                Model.Show(LanguageUtils.Get("AddGameWindow.Tab1.Text50"));
            }
            else
            {
                Done(res.Game!.UUID);
            }
        }
    }

    private void LoadVersion()
    {
        if (DisplayVersion == false)
        {
            return;
        }

        if (_last == null)
        {
            return;
        }

        DisplayFile.LoadVersion((SourceType)Source, _last.Pid);
    }
}
