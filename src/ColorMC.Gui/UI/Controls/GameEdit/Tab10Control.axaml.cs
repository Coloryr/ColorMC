using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UIBinding;
using DynamicData;
using System.Collections.ObjectModel;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab10Control : UserControl
{
    private readonly ObservableCollection<ServerInfoObj> List = new();
    private GameSettingObj Obj;

    public Tab10Control()
    {
        InitializeComponent();

        Button_A1.PointerExited += Button_A1_PointerLeave;
        Button_A.PointerEntered += Button_A_PointerEnter;

        Button_R1.PointerExited += Button_R1_PointerLeave;
        Button_R.PointerEntered += Button_R_PointerEnter;

        Button_R.Click += Button_R1_Click;
        Button_R1.Click += Button_R1_Click;
        Button_A.Click += Button_A1_Click;
        Button_A1.Click += Button_A1_Click;

        DataGrid1.Items = List;

        DataGrid1.SelectionChanged += DataGrid1_SelectionChanged;
        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;
    }

    private void DataGrid1_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (DataGrid1.SelectedItem is not ServerInfoObj obj)
                    return;

                _ = new GameEditFlyout5(this, obj);
            });
        }
    }

    private void DataGrid1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataGrid1.SelectedItem is not ServerInfoObj obj)
            return;

        ServerMotd.Load(obj.IP);
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private async void Button_A1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        await window.Info3.ShowInput(App.GetLanguage("GameEditWindow.Tab10.Info1"),
            App.GetLanguage("GameEditWindow.Tab10.Info2"), false);
        var res = window.Info3.Read();

        if (string.IsNullOrWhiteSpace(res.Item1) || string.IsNullOrWhiteSpace(res.Item2))
        {
            window.Info.Show(App.GetLanguage("GameEditWindow.Tab10.Error1"));
            return;
        }

        GameBinding.AddServer(Obj, res.Item1, res.Item2);
        window.Info2.Show(App.GetLanguage("GameEditWindow.Tab10.Info3"));
        Load();
    }

    private void Button_A1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_A1, null, CancellationToken.None);
    }

    private void Button_A_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_A1, CancellationToken.None);
    }
    private void Button_R1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_R1, null, CancellationToken.None);
    }

    private void Button_R_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_R1, CancellationToken.None);
    }

    private void Load()
    {
        var window = App.FindRoot(VisualRoot);
        window.Info1.Show(App.GetLanguage("GameEditWindow.Tab10.Info4"));
        List.Clear();
        List.AddRange(GameBinding.GetServers(Obj));
        window.Info1.Close();
    }

    public void Update()
    {
        if (Obj == null)
            return;

        Load();
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }

    public void Delete(ServerInfoObj obj)
    {
        var window = App.FindRoot(VisualRoot);
        GameBinding.DeleteServer(Obj, obj);
        window.Info2.Show(App.GetLanguage("GameEditWindow.Tab10.Info5"));
        Load();
    }
}
