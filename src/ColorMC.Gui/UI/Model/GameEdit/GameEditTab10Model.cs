using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab10Model : GameEditTabModel
{
    public ObservableCollection<ServerInfoObj> ServerList { get; init; } = new();

    [ObservableProperty]
    private ServerInfoObj? item;
    [ObservableProperty]
    private string? iP;

    public GameEditTab10Model(IUserControl con, GameSettingObj obj) : base(con, obj) 
    {
        
    }

    partial void OnItemChanged(ServerInfoObj? value)
    {
        IP = value?.IP;
    }

    [RelayCommand]
    public void Load()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab10.Info4"));
        ServerList.Clear();
        ServerList.AddRange(GameBinding.GetServers(Obj));
        window.ProgressInfo.Close();
    }

    [RelayCommand]
    public async void Add()
    {
        var window = Con.Window;
        await window.InputInfo.ShowInput(App.GetLanguage("GameEditWindow.Tab10.Info1"),
            App.GetLanguage("GameEditWindow.Tab10.Info2"), false);
        var res = window.InputInfo.Read();

        if (string.IsNullOrWhiteSpace(res.Item1) || string.IsNullOrWhiteSpace(res.Item2))
        {
            window.OkInfo.Show(App.GetLanguage("GameEditWindow.Tab10.Error1"));
            return;
        }

        GameBinding.AddServer(Obj, res.Item1, res.Item2);
        window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab10.Info3"));
        Load();
    }

    public void Delete(ServerInfoObj obj)
    {
        var window = Con.Window;
        GameBinding.DeleteServer(Obj, obj);
        window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab10.Info5"));
        Load();
    }
}
