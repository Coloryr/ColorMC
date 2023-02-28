using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using DynamicData;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab5Control : UserControl
{
    private readonly ObservableCollection<JavaDisplayObj> List = new();
    public Tab5Control()
    {
        InitializeComponent();

        Button_SelectFile.Click += Button_SelectFile_Click;
        Button_Add.Click += Button_Add_Click;

        Button_R1.PointerExited += Button_R1_PointerLeave;
        Button_R.PointerEntered += Button_R_PointerEnter;

        Button_R1.Click += Button_R1_Click;

        Button_D1.PointerExited += Button_D1_PointerLeave;
        Button_D.PointerEntered += Button_D_PointerEnter;

        Button_D1.Click += Button_D1_Click;

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;

        Expander_R.ContentTransition = App.CrossFade100;

        DataGrid1.Items = List;
        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;

        LayoutUpdated += Tab5Control_LayoutUpdated;
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpenDownloadJavaPath();
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowAddJava();
    }

    private void DataGrid1_CellPointerPressed(object? sender,
        DataGridCellPointerPressedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var window = (VisualRoot as SettingWindow)!;
            var java = DataGrid1.SelectedItem as JavaDisplayObj;
            if (java == null)
                return;

            if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                new SettingFlyout(window, java).ShowAt(this, true);
            }
        });
    }

    private void Tab5Control_LayoutUpdated(object? sender, EventArgs e)
    {
        DataGrid1.MakeTran();
        Expander_R.MakePadingNull();
        Expander_D.MakePadingNull();
    }

    private async void Button_D1_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as SettingWindow)!;
        var res = await window.Info.ShowWait(App.GetLanguage("SettingWindow.Tab5.Info3"));
        if (!res)
            return;

        JavaBinding.RemoveAllJava();
        Load();
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Button_D1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_D.IsExpanded = false;
    }

    private void Button_D_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_D.IsExpanded = true;
    }

    private void Button_R1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_R.IsExpanded = false;
    }

    private void Button_R_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_R.IsExpanded = true;
    }

    private void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as SettingWindow)!;

        var name = TextBox1.Text;
        var local = TextBox2.Text;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(local))
        {
            window.Info.Show(App.GetLanguage("Error8"));
            return;
        }

        try
        {
            window.Info1.Show(App.GetLanguage("SettingWindow.Tab5.Info1"));

            var res = JavaBinding.AddJava(name, local);
            if (res.Item1 == null)
            {
                window.Info1.Close();
                window.Info.Show(res.Item2!);
                return;
            }

            TextBox1.Text = "";
            TextBox2.Text = "";
            window.Info1.Close();

            Load();
        }
        finally
        {

        }
    }

    private async void Button_SelectFile_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as SettingWindow)!;
        var file = await BaseBinding.OpFile(window,
             App.GetLanguage("SettingWindow.Tab5.Info2"),
             new string[] { SystemInfo.Os == OsType.Windows ? "*.exe" : "" },
             App.GetLanguage("SettingWindow.Tab5.Info4"),
             storage: JavaBinding.GetSuggestedStartLocation());

        if (file?.Any() == true)
        {
            var item = file[0];
            TextBox2.Text = item.GetPath();
            var info = JavaBinding.GetJavaInfo(item.GetPath());
            if (info != null)
            {
                TextBox1.Text = info.Type + "_" + info.Version;
            }
        }
    }

    public void Load()
    {
        List.Clear();
        List.AddRange(JavaBinding.GetJavas());
    }
}
