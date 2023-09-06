using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel : GameModel
{
    public ObservableCollection<ServerInfoObj> ServerList { get; init; } = new();

    [ObservableProperty]
    private ServerInfoObj? _serverItem;
    [ObservableProperty]
    private (string?, ushort) _iPPort;

    partial void OnServerItemChanged(ServerInfoObj? value)
    {
        IPPort = (value?.IP, 0);
    }

    [RelayCommand]
    public async Task LoadServer()
    {
        Model.Progress(App.GetLanguage("GameEditWindow.Tab10.Info4"));
        ServerList.Clear();
        ServerList.AddRange(await GameBinding.GetServers(Obj));
        Model.ProgressClose();
    }

    [RelayCommand]
    public async Task AddServer()
    {
        var (Cancel, Text1, Text2) = await Model.ShowInput(
            App.GetLanguage("GameEditWindow.Tab10.Info1"),
            App.GetLanguage("GameEditWindow.Tab10.Info2"), false);
        if (Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Text1) || string.IsNullOrWhiteSpace(Text2))
        {
            Model.Show(App.GetLanguage("GameEditWindow.Tab10.Error1"));
            return;
        }

        Model.Progress(App.GetLanguage("GameEditWindow.Tab10.Info6"));
        await GameBinding.AddServer(Obj, Text1, Text2);
        Model.ProgressClose();
        Model.Notify(App.GetLanguage("GameEditWindow.Tab10.Info3"));
        await LoadServer();
    }

    public async void DeleteServer(ServerInfoObj obj)
    {
        Model.Progress(App.GetLanguage("GameEditWindow.Tab10.Info6"));
        await GameBinding.DeleteServer(Obj, obj);
        Model.ProgressClose();
        Model.Notify(App.GetLanguage("GameEditWindow.Tab10.Info5"));
        await LoadServer();
    }
}
