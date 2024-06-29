using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel
{
    public ObservableCollection<ServerInfoObj> ServerList { get; init; } = [];

    [ObservableProperty]
    private ServerInfoObj? _serverItem;
    [ObservableProperty]
    private (string?, ushort) _iPPort;

    [ObservableProperty]
    private bool _serverEmptyDisplay;

    partial void OnServerItemChanged(ServerInfoObj? value)
    {
        IPPort = (value?.IP, 0);
    }

    private async void AddServer()
    {
        var (Cancel, Text1, Text2) = await Model.ShowInput(
            App.Lang("GameEditWindow.Tab10.Info1"),
            App.Lang("GameEditWindow.Tab10.Info2"), false);
        if (Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Text1) || string.IsNullOrWhiteSpace(Text2))
        {
            Model.Show(App.Lang("GameEditWindow.Tab10.Error1"));
            return;
        }

        Model.Progress(App.Lang("GameEditWindow.Tab10.Info6"));
        await GameBinding.AddServer(_obj, Text1, Text2);
        Model.ProgressClose();
        Model.Notify(App.Lang("GameEditWindow.Tab10.Info3"));
        LoadServer();
    }

    public async void LoadServer()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab10.Info4"));
        ServerList.Clear();
        ServerList.AddRange(await GameBinding.GetServers(_obj));
        Model.ProgressClose();
        ServerEmptyDisplay = ServerList.Count == 0;
    }

    public async void DeleteServer(ServerInfoObj obj)
    {
        Model.Progress(App.Lang("GameEditWindow.Tab10.Info6"));
        await GameBinding.DeleteServer(_obj, obj);
        Model.ProgressClose();
        Model.Notify(App.Lang("GameEditWindow.Tab10.Info5"));
        LoadServer();
    }

    public void SetChoiseTab10()
    {
        Model.SetChoiseContent(_useName, App.Lang("Button.Refash"));
        Model.SetChoiseCall(_useName, choise: LoadServer);
    }

    public void RemoveChoiseTab10()
    {
        Model.RemoveChoiseData(_useName);
    }
}
