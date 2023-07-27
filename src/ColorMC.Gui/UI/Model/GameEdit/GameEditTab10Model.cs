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

public partial class GameEditTab10Model : GameEditModel
{
    public ObservableCollection<ServerInfoObj> ServerList { get; init; } = new();

    [ObservableProperty]
    private ServerInfoObj? _item;
    [ObservableProperty]
    private (string?, ushort) _iPPort;

    public GameEditTab10Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    partial void OnItemChanged(ServerInfoObj? value)
    {
        IPPort = (value?.IP, 0);
    }

    [RelayCommand]
    public async Task Load()
    {
        Progress(App.GetLanguage("GameEditWindow.Tab10.Info4"));
        ServerList.Clear();
        ServerList.AddRange(await GameBinding.GetServers(Obj));
        ProgressClose();
    }

    [RelayCommand]
    public async Task Add()
    {
        var (Cancel, Text1, Text2) = await ShowInput(
            App.GetLanguage("GameEditWindow.Tab10.Info1"),
            App.GetLanguage("GameEditWindow.Tab10.Info2"), false);
        if (Cancel)
            return;

        if (string.IsNullOrWhiteSpace(Text1) || string.IsNullOrWhiteSpace(Text2))
        {
            Show(App.GetLanguage("GameEditWindow.Tab10.Error1"));
            return;
        }

        GameBinding.AddServer(Obj, Text1, Text2);
        Notify(App.GetLanguage("GameEditWindow.Tab10.Info3"));
        await Load();
    }

    public async void Delete(ServerInfoObj obj)
    {
        GameBinding.DeleteServer(Obj, obj);
        Notify(App.GetLanguage("GameEditWindow.Tab10.Info5"));
        await Load();
    }
}
