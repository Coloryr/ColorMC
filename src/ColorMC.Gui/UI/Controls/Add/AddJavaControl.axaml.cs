using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddJavaControl : UserControl, IUserControl
{
    private readonly List<JavaDownloadDisplayObj> List1 = new();
    private readonly ObservableCollection<JavaDownloadDisplayObj> List = new();
    private bool load = true;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public AddJavaControl()
    {
        InitializeComponent();

        ColorMCCore.JavaUnzip = JavaUnzip;

        DataGrid1.ItemsSource = List;

        ComboBox1.ItemsSource = JavaBinding.GetJavaType();

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox2_SelectionChanged;
        ComboBox3.SelectionChanged += ComboBox3_SelectionChanged;
        ComboBox4.SelectionChanged += ComboBox4_SelectionChanged;

        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;

        Button1.Click += Button1_Click;
    }

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("AddJavaWindow.Title"));

        ComboBox1.SelectedIndex = 0;
    }

    public void Closed()
    {
        App.AddJavaWindow = null;
    }

    private void JavaUnzip()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var window = App.FindRoot(VisualRoot);
            window.ProgressInfo.NextText(App.GetLanguage("AddJavaWindow.Info5"));
        });
    }

    private async void DataGrid1_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataGrid1.SelectedItem is not JavaDownloadDisplayObj obj)
            return;

        var window = App.FindRoot(VisualRoot);
        var res = await window.OkInfo.ShowWait(string.Format(
            App.GetLanguage("AddJavaWindow.Info1"), obj.Name));
        if (!res)
            return;

        if (ConfigBinding.GetAllConfig().Item2?.WindowMode != true)
        {
            window.ProgressInfo.Show(App.GetLanguage("AddJavaWindow.Info2"));
        }
        var res1 = await JavaBinding.DownloadJava(obj);
        window.ProgressInfo.Close();
        if (!res1.Item1)
        {
            window.OkInfo.Show(res1.Item2!);
            return;
        }

        window.NotifyInfo.Show(App.GetLanguage("AddJavaWindow.Info3"));
    }

    private void ComboBox4_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (load)
            return;

        Select();
    }

    private void ComboBox3_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (load)
            return;

        if (ComboBox1.SelectedIndex == 0)
        {
            Button1_Click(null, null);
        }
        else
        {
            Select();
        }
    }

    private void ComboBox2_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (load)
            return;

        if (ComboBox1.SelectedIndex == 0)
        {
            Button1_Click(null, null);
        }
        else
        {
            Select();
        }
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Switch();
        Button1_Click(null, null);
    }

    private async void Button1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        window.ProgressInfo.Show(App.GetLanguage("AddJavaWindow.Info4"));

        load = true;

        List1.Clear();
        List.Clear();

        var res = await JavaBinding.GetJavaList(ComboBox1.SelectedIndex, ComboBox2.SelectedIndex, ComboBox3.SelectedIndex);

        if (res.Item1)
        {
            ComboBox2.ItemsSource = res.Os;
            ComboBox3.ItemsSource = res.MainVersion;
            ComboBox4.ItemsSource = res.Arch;

            List1.AddRange(res.Item5!);

            Select();

            window.ProgressInfo.Close();
        }
        else
        {
            window.ProgressInfo.Close();
#if !DEBUG
            window.Info.Show(App.GetLanguage("AddJavaWindow.Error1"));
#endif
        }

        load = false;
    }

    private void Select()
    {
        List.Clear();

        string arch = "";
        string version = "";
        string os = "";
        if (ComboBox4.SelectedItem is string temp)
        {
            arch = temp;
        }
        if (ComboBox3.SelectedItem is string temp1)
        {
            version = temp1;
        }
        if (ComboBox2.SelectedItem is string temp2)
        {
            os = temp2;
        }
        bool arch1 = !string.IsNullOrWhiteSpace(arch);
        bool version1 = !string.IsNullOrWhiteSpace(version);
        bool os1 = !string.IsNullOrWhiteSpace(os);

        var list1 = from item in List1
                    where (!arch1 || (item.Arch == arch))
                    && (!version1 || (item.MainVersion == version))
                    && (ComboBox1.SelectedIndex == 0 || !os1 || (item.Os == os))
                    select item;

        if (list1.Count() > 100 && !(arch1 && version1 && os1))
        {
            Grid1.IsVisible = true;
        }
        else
        {
            Grid1.IsVisible = false;
            List.AddRange(list1);
        }
    }

    private void Switch()
    {
        ComboBox2.ItemsSource = null;
        ComboBox3.ItemsSource = null;
        ComboBox4.ItemsSource = null;
        ComboBox3.SelectedIndex = 0;
        ComboBox2.SelectedIndex = 0;
        ComboBox4.SelectedIndex = 0;
    }
}
