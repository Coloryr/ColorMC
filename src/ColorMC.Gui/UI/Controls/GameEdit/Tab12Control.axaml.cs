using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using DynamicData;
using System;
using System.Collections.ObjectModel;

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

        Button_R1.Click += Button_R1_Click;
        Button_A1.Click += Button_A1_Click;

        Button1.Click += Button1_Click;

        DataGrid1.Items = List;

        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;
        LayoutUpdated += Tab10Control_LayoutUpdated1;
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

                new GameEditFlyout7(this, obj).ShowAt(this, true);
            });
        }
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private async void Button_A1_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as IBaseWindow)!;
        var res = await BaseBinding.AddFile(window as Window, Obj, FileType.Schematic);

        if (res == null)
            return;

        if (res == false)
        {
            window.Info2.Show(App.GetLanguage("Error12"));
            return;
        }

        window.Info2.Show(App.GetLanguage("GameEditWindow.Tab12.Info3"));
        Load();
    }

    private void Tab10Control_LayoutUpdated1(object? sender, EventArgs e)
    {
        Expander_A.MakePadingNull();
        Expander_R.MakePadingNull();
    }

    private void Button_A1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_A.IsExpanded = false;
    }

    private void Button_A_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_A.IsExpanded = true;
    }
    private void Button_R1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_R.IsExpanded = false;
    }

    private void Button_R_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_R.IsExpanded = true;
    }

    private async void Load()
    {
        var window = (VisualRoot as IBaseWindow)!;
        window.Info1.Show(App.GetLanguage("GameEditWindow.Tab10.Info4"));
        List.Clear();
        List.AddRange(await GameBinding.GetSchematics(Obj));
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
        var window = (VisualRoot as IBaseWindow)!;

        obj.Schematic.Delete();
        window.Info2.Show(App.GetLanguage("GameEditWindow.Tab10.Info5"));
        Load();
    }
}
