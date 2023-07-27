using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddGameTab2Model : AddGameControlModel
{
    public List<string> PackTypeList => GameBinding.GetPackType();

    [ObservableProperty]
    private string _local;

    [ObservableProperty]
    private int _type = -1;

    public AddGameTab2Model(IUserControl con) : base(con)
    {
        ColorMCCore.PackState = PackState;
        ColorMCCore.PackUpdate = PackUpdate;
    }

    partial void OnLocalChanged(string value)
    {
        if (File.Exists(value))
        {
            if (value.EndsWith(".mrpack"))
            {
                Type = 2;
                return;
            }
            if (value.EndsWith(".zip"))
            {
                using ZipFile zFile = new(value);
                if (zFile.GetEntry("game.json") != null)
                {
                    Type = 0;
                }
                else if (zFile.GetEntry("mcbbs.packmeta") != null)
                {
                    Type = 4;
                }
                else if (zFile.GetEntry("instance.cfg") != null)
                {
                    Type = 3;
                }
                else if (zFile.GetEntry("manifest.json") != null)
                {
                    Type = 1;
                }
            }
        }
    }

    [RelayCommand]
    public void Add()
    {
        if (BaseBinding.IsDownload)
        {
            Show(App.GetLanguage("AddGameWindow.Tab1.Error4"));
            return;
        }
        if (Type == -1)
        {
            Show(App.GetLanguage("AddGameWindow.Tab2.Error3"));
            return;
        }

        switch (Type)
        {
            case 0:
                AddPack(PackType.ColorMC);
                break;
            case 1:
                AddPack(PackType.CurseForge);
                break;
            case 2:
                AddPack(PackType.Modrinth);
                break;
            case 3:
                AddPack(PackType.MMC);
                break;
            case 4:
                AddPack(PackType.HMCL);
                break;
        }
    }

    [RelayCommand]
    public async Task SelectPack()
    {
        var res = await BaseBinding.OpFile(Window, FileType.ModPack);
        if (!string.IsNullOrWhiteSpace(res))
        {
            Local = res;
        }
    }

    private void PackUpdate(int size, int now)
    {
        ProgressUpdate((double)now / size);
    }

    private void PackState(CoreRunState state)
    {
        if (state == CoreRunState.Read)
        {
            Progress(App.GetLanguage("AddGameWindow.Tab2.Info1"));
        }
        else if (state == CoreRunState.Init)
        {
            ProgressUpdate(App.GetLanguage("AddGameWindow.Tab2.Info2"));
        }
        else if (state == CoreRunState.GetInfo)
        {
            ProgressUpdate(App.GetLanguage("AddGameWindow.Tab2.Info3"));
        }
        else if (state == CoreRunState.Download)
        {
            ProgressUpdate(App.GetLanguage("AddGameWindow.Tab2.Info4"));
            ProgressUpdate(-1);
        }
        else if (state == CoreRunState.End)
        {
            Name = "";
            Group = "";
        }
    }

    private async void AddPack(PackType type)
    {
        ColorMCCore.GameOverwirte = GameOverwirte;

        if (string.IsNullOrWhiteSpace(Local))
        {
            Show(App.GetLanguage("AddGameWindow.Tab2.Error2"));
            return;
        }

        Progress(App.GetLanguage("AddGameWindow.Tab2.Info6"));
        var res = await GameBinding.AddPack(Local, type, Name, Group);
        ProgressClose();
        if (!res.Item1)
        {
            Show(App.GetLanguage("AddGameWindow.Tab2.Error1"));
            return;
        }

        App.MainWindow?.Window.NotifyInfo.Show(App.GetLanguage("AddGameWindow.Tab2.Info5"));
        App.MainWindow?.LoadMain();
        Window.Close();
    }

    public void AddFile(string file)
    {
        Local = file;
    }

    private async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        ProgressClose();
        var test = await ShowWait(
            string.Format(App.GetLanguage("AddGameWindow.Info2"), obj.Name));
        Progress();
        return test;
    }
}
