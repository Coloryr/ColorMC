using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.Utils;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Heijden.DNS;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddGameTab2Model : ObservableObject
{
    private IUserControl Con;

    public ObservableCollection<string> GroupList { get; init; } = new();

    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private string group;
    [ObservableProperty]
    private string local;

    [ObservableProperty]
    private int type = -1;

    public AddGameTab2Model(IUserControl con)
    {
        Con = con;

        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);

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
        var window = Con.Window;
        if (BaseBinding.IsDownload)
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error4"));
            return;
        }
        if (Type == -1)
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab2.Error3"));
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
    public async void SelectPack()
    {
        var window = Con.Window;
        var res = await BaseBinding.OpFile(window, FileType.ModPack);
        if (!string.IsNullOrWhiteSpace(res))
        {
            Local = res;
        }
    }

    [RelayCommand]
    public async void AddGroup()
    {
        var window = Con.Window;
        await window.InputInfo.ShowOne(App.GetLanguage("AddGameWindow.Tab1.Info5"), false);
        if (window.InputInfo.Cancel)
        {
            return;
        }

        var res = window.InputInfo.Read().Item1;
        if (string.IsNullOrWhiteSpace(res))
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error2"));
            return;
        }

        if (!GameBinding.AddGameGroup(res))
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Error3"));
            return;
        }

        window.NotifyInfo.Show(App.GetLanguage("AddGameWindow.Tab1.Info6"));

        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
    }

    private void PackUpdate(int size, int now)
    {
        var window = Con.Window;
        window.ProgressInfo.Progress((double)now / size);
    }

    private void PackState(CoreRunState state)
    {
        var window = Con.Window;
        if (state == CoreRunState.Read)
        {
            window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab2.Info1"));
        }
        else if (state == CoreRunState.Init)
        {
            window.ProgressInfo.NextText(App.GetLanguage("AddGameWindow.Tab2.Info2"));
        }
        else if (state == CoreRunState.GetInfo)
        {
            window.ProgressInfo.NextText(App.GetLanguage("AddGameWindow.Tab2.Info3"));
        }
        else if (state == CoreRunState.Download)
        {
            window.ProgressInfo.NextText(App.GetLanguage("AddGameWindow.Tab2.Info4"));
            window.ProgressInfo.Progress(-1);
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

        var window = Con.Window;
        if (string.IsNullOrWhiteSpace(Local))
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab2.Error2"));
            return;
        }

        window.ProgressInfo.Show(App.GetLanguage("AddGameWindow.Tab2.Info6"));
        var res = await GameBinding.AddPack(Local, type, Name, Group);
        window.ProgressInfo.Close();
        if (res.Item1)
        {
            App.MainWindow?.Window.NotifyInfo.Show(App.GetLanguage("AddGameWindow.Tab2.Info5"));
            App.MainWindow?.Load();
            window.Close();
        }
        else
        {
            window.OkInfo.Show(App.GetLanguage("AddGameWindow.Tab2.Error1"));
        }
    }

    public void AddFile(string file)
    {
        Local = file;
    }

    private async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        var window = Con.Window;
        window.ProgressInfo.Close();
        var test = await window.OkInfo.ShowWait(
            string.Format(App.GetLanguage("AddGameWindow.Info2"), obj.Name));
        window.ProgressInfo.Show();
        return test;
    }
}
