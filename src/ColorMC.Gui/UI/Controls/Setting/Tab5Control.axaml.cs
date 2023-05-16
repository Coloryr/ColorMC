using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using System;
using System.Collections.ObjectModel;
using System.Threading;

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

        Button_D1.PointerExited += Button_D1_PointerLeave;
        Button_D.PointerEntered += Button_D_PointerEnter;

        Button_R1.Click += Button_R1_Click;
        Button_D1.Click += Button_D1_Click;
        Button_R.Click += Button_R1_Click;
        Button_D.Click += Button_D1_Click;

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;

        DataGrid1.ItemsSource = List;
        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;

        LayoutUpdated += Tab5Control_LayoutUpdated;
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var list = JavaBinding.FindJava();
        if (list == null)
        {
            window.Info.Show(App.GetLanguage("SettingWindow.Tab5.Error1"));
            return;
        }

        list.ForEach(item => JvmPath.AddItem(item.Type + "_" + item.Version, item.Path));
        Load();
        window.Info2.Show(App.GetLanguage("SettingWindow.Tab5.Info4"));
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
            var java = DataGrid1.SelectedItem as JavaDisplayObj;
            if (java == null)
                return;

            if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                _ = new SettingFlyout1(this, DataGrid1.SelectedItems);
            }
        });
    }

    private void Tab5Control_LayoutUpdated(object? sender, EventArgs e)
    {
        DataGrid1.MakeTran();
    }

    private async void Button_D1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
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
        App.CrossFade100.Start(Button_D1, null, CancellationToken.None);
    }

    private void Button_D_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_D1, CancellationToken.None);
    }

    private void Button_R1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_R1, null, CancellationToken.None);
    }

    private void Button_R_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_R1, CancellationToken.None);
    }

    private void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);

        var name = TextBox1.Text;
        var local = TextBox2.Text;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(local))
        {
            window.Info.Show(App.GetLanguage("Gui.Error8"));
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
        var window = App.FindRoot(VisualRoot);
        var file = await BaseBinding.OpFile(window, FileType.Java);

        if (file != null)
        {
            TextBox2.Text = file;
            var info = JavaBinding.GetJavaInfo(file);
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
