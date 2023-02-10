using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Windows;

public partial class AddJavaWindow : Window
{
    private List<JavaDownloadDisplayObj> List1 = new();
    private ObservableCollection<JavaDownloadDisplayObj> List = new();
    private bool load = true;
    public AddJavaWindow()
    {
        InitializeComponent();

        Head.SetWindow(this);
        this.BindFont();
        Icon = App.Icon;
        Rectangle1.MakeResizeDrag(this);

        DataGrid1.Items = List;

        ComboBox1.Items = JavaBinding.GetJavaType();

        ComboBox1.SelectedIndex = 0;
        ComboBox2.SelectedIndex = 0;
        ComboBox3.SelectedIndex = 0;
        ComboBox4.SelectedIndex = 0;

        Switch();

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox2_SelectionChanged;
        ComboBox3.SelectionChanged += ComboBox3_SelectionChanged;
        ComboBox4.SelectionChanged += ComboBox4_SelectionChanged;

        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;

        Button1.Click += Button1_Click;

        Button1_Click(null, null);
    }

    private async void DataGrid1_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataGrid1.SelectedItem is not JavaDownloadDisplayObj obj)
            return;

        var res = await Info.ShowWait(string.Format("是否要下载 {0}", obj.Name));
        if (!res)
            return;

        Info1.Show("正在下载Java");
        var res1 = await JavaBinding.DownloadJava(obj);
        Info1.Close();
        if (!res1.Item1)
        {
            Info.Show(res1.Item2!);
            return;
        }

        Info2.Show("下载Java成功");
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
        Info1.Show("正在获取版本");

        load = true;

        List1.Clear();
        List.Clear();

        var res = await JavaBinding.GetJavaList(ComboBox1.SelectedIndex, ComboBox2.SelectedIndex, ComboBox3.SelectedIndex);

        if (res.Item1)
        {
            ComboBox2.Items = res.Os;
            ComboBox3.Items = res.MainVersion;
            ComboBox4.Items = res.Arch;

            List1.AddRange(res.Item5!);

            Select();

            Info1.Close();
        }
        else
        {
            Info1.Close();
            Info.Show("获取失败");
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

        var list =
               from item in List1
               where (arch1 ? (item.Arch == arch) : true)
               && (version1 ? (item.MainVersion == version) : true)
               && (ComboBox1.SelectedIndex == 0 ? true : os1 ? (item.Os == os) : true)
               select item;

        List.AddRange(list);
    }

    private void Switch()
    {
        ComboBox2.Items = null;
        ComboBox3.Items = null;
        ComboBox4.Items = null;
        ComboBox3.SelectedIndex = 0;
        ComboBox2.SelectedIndex = 0;
        ComboBox4.SelectedIndex = 0;
    }
}
