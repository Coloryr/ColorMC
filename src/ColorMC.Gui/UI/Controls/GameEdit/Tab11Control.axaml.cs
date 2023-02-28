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
using System.Linq;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab11Control : UserControl
{
    private readonly ObservableCollection<ShaderpackDisplayObj> List = new();
    private GameSettingObj Obj;

    public Tab11Control()
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
        BaseBinding.OpPath(Obj, PathType.ShaderpacksPath);
    }

    private void DataGrid1_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (DataGrid1.SelectedItem is not ShaderpackDisplayObj obj)
                    return;

                new GameEditFlyout6(this, obj).ShowAt(this, true);
            });
        }
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private async void Button_A1_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as GameEditWindow)!;
        var res = await BaseBinding.AddFile(window, Obj, FileType.Shaderpack);
        if (res == null)
            return;

        if (res == false)
        {
            window.Info2.Show(App.GetLanguage("Error12"));
            return;
        }

        window.Info2.Show(App.GetLanguage("GameEditWindow.Tab11.Info3"));
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

    private void Load()
    {
        var window = (VisualRoot as GameEditWindow)!;
        window.Info1.Show(App.GetLanguage("GameEditWindow.Tab10.Info4"));
        List.Clear();
        List.AddRange(GameBinding.GetShaderpacks(Obj));
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

    public void Delete(ShaderpackDisplayObj obj)
    {
        var window = (VisualRoot as GameEditWindow)!;
        obj.Shaderpack.Delete();
        window.Info2.Show(App.GetLanguage("GameEditWindow.Tab10.Info5"));
        Load();
    }
}
