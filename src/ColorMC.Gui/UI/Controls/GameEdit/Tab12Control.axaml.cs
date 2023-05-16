using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UIBinding;
using System.Collections.ObjectModel;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab12Control : UserControl
{
    private readonly ObservableCollection<SchematicDisplayObj> List = new();
    private GameSettingObj Obj;

    public Tab12Control()
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

        Button1.Click += Button1_Click;

        DataGrid1.ItemsSource = List;

        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            Grid2.IsVisible = true;
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
    }

    private async void Drop(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
        var res = await GameBinding.AddFile(Obj, e.Data, FileType.Schematic);
        if (res)
        {
            Load();
        }
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpPath(Obj.GetSchematicsPath());
    }

    private void DataGrid1_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (DataGrid1.SelectedItem is not SchematicDisplayObj obj)
                    return;

                _ = new GameEditFlyout7(this, obj);
            });
        }
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private async void Button_A1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var res = await GameBinding.AddFile(window as Window, Obj, FileType.Schematic);

        if (res == null)
            return;

        if (res == false)
        {
            window.Info2.Show(App.GetLanguage("Gui.Error12"));
            return;
        }

        window.Info2.Show(App.GetLanguage("GameEditWindow.Tab12.Info3"));
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
        List.AddRange(GameBinding.GetSchematics(Obj));
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

    public void Delete(SchematicDisplayObj obj)
    {
        var window = App.FindRoot(VisualRoot);

        obj.Schematic.Delete();
        window.Info2.Show(App.GetLanguage("GameEditWindow.Tab10.Info5"));
        Load();
    }
}
