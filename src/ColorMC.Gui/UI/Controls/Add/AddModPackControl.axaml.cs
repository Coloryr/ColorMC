using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.FTB;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using DynamicData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddModPackControl : UserControl, IUserControl, IAddWindow
{
    private readonly Dictionary<int, string> Categories = new();
    private readonly ObservableCollection<FileDisplayObj> List1 = new();
    private readonly ObservableCollection<string> List4 = new();

    private FileItemControl? Last;
    private bool load = false;

    public AddModPackControl()
    {
        InitializeComponent();

        ComboBox1.ItemsSource = GameBinding.GetSourceList();
        ComboBox3.ItemsSource = List4;

        ComboBox6.ItemsSource = List4;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox_SelectionChanged;
        ComboBox3.SelectionChanged += ComboBox_SelectionChanged;
        ComboBox4.SelectionChanged += ComboBox_SelectionChanged;

        ComboBox6.SelectionChanged += ComboBox6_SelectionChanged;

        DataGridFiles.Items = List1;
        DataGridFiles.DoubleTapped += DataGridFiles_DoubleTapped;

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        ButtonSearch.Click += ButtonSearch_Click;
        ButtonCancel.Click += ButtonCancel_Click;
        ButtonDownload.Click += ButtonDownload_Click;

        Input2.PropertyChanged += Input2_PropertyChanged;
        Input3.PropertyChanged += Input3_PropertyChanged;

        Input1.KeyDown += Input1_KeyDown;
    }

    private void Input1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Button1_Click(null, null);
        }
    }

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    private void ComboBox6_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Load1();
    }

    private void ComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (load)
            return;

        ComboBox6.SelectedItem = ComboBox1.SelectedItem;

        Load();
    }

    private void Lock()
    {
        ComboBox1.IsEnabled = false;
        ComboBox2.IsEnabled = false;
        ComboBox3.IsEnabled = false;
        Input1.IsEnabled = false;
        ComboBox4.IsEnabled = false;
        Input2.IsEnabled = false;

        Button2.IsEnabled = false;
    }

    private void Unlock()
    {
        ComboBox1.IsEnabled = true;
        ComboBox2.IsEnabled = true;
        ComboBox3.IsEnabled = true;
        Input1.IsEnabled = true;
        ComboBox4.IsEnabled = true;
        Input2.IsEnabled = true;

        if (ComboBox1.SelectedIndex == 2)
        {
            ComboBox3.IsEnabled = false;
            ComboBox4.IsEnabled = false;
            Input2.IsEnabled = false;

            if (ComboBox4.SelectedIndex == 4)
            {
                Input1.IsEnabled = true;
            }
            else
            {
                Input1.IsEnabled = false;
            }
        }
    }

    private async void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        load = true;

        Lock();

        ComboBox2.ItemsSource = null;
        ComboBox4.ItemsSource = null;

        List4.Clear();
        Categories.Clear();
        foreach (var item in ListBox_Items.Children)
        {
            if (item is FileItemControl con)
            {
                con.Dispose();
            }
        }
        ListBox_Items.Children.Clear();

        var window = App.FindRoot(VisualRoot);
        switch (ComboBox1.SelectedIndex)
        {
            case 0:
            case 1:
                ComboBox4.ItemsSource = ComboBox1.SelectedIndex == 0 ?
                    GameBinding.GetCurseForgeSortTypes() :
                    GameBinding.GetModrinthSortTypes();

                window.Info1.Show(App.GetLanguage("AddModPackWindow.Info4"));
                var list = ComboBox1.SelectedIndex == 0 ?
                    await GameBinding.GetCurseForgeGameVersions() :
                    await GameBinding.GetModrinthGameVersions();
                var list1 = ComboBox1.SelectedIndex == 0 ?
                    await GameBinding.GetCurseForgeCategories() :
                    await GameBinding.GetModrinthCategories();
                window.Info1.Close();
                if (list == null || list1 == null)
                {
#if !DEBUG
                window.Info.Show(App.GetLanguage("AddModPackWindow.Error4"));
                window.Close();
#endif
                    return;
                }
                List4.AddRange(list);

                Categories.Add(0, "");
                var a = 1;
                foreach (var item in list1)
                {
                    Categories.Add(a++, item.Key);
                }

                var list2 = new List<string>()
                {
                    ""
                };

                list2.AddRange(list1.Values);

                ComboBox2.ItemsSource = list2;

                ComboBox2.SelectedIndex = 0;
                ComboBox3.SelectedIndex = 0;
                ComboBox4.SelectedIndex = ComboBox1.SelectedIndex == 0 ? 1 : 0;

                Load();
                break;
            case 2:
                list = WebBinding.GetFTBTypeList();

                ComboBox2.ItemsSource = list;
                ComboBox2.SelectedIndex = 0;

                Load();
                break;
        }

        load = false;
    }

    private void Input3_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        var property = e.Property.Name;
        if (property == "Value")
        {
            Load1();
        }
    }

    private void Input2_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        var property = e.Property.Name;
        if (property == "Value")
        {
            Load();
        }
    }

    private void ButtonDownload_Click(object? sender, RoutedEventArgs e)
    {
        DataGridFiles_DoubleTapped(sender, e);
    }

    private async void DataGridFiles_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var item = DataGridFiles.SelectedItem as FileDisplayObj;
        if (item == null)
            return;

        var res = await window.Info.ShowWait(
            string.Format(App.GetLanguage("AddModPackWindow.Info1"), item.Name));
        if (res)
        {
            Install1(item);
        }
    }

    private void ButtonCancel_Click(object? sender, RoutedEventArgs e)
    {
        App.CrossFade300.Start(Grid1, null, CancellationToken.None);
    }

    private void ButtonSearch_Click(object? sender, RoutedEventArgs e)
    {
        Load1();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        if (Last == null)
        {
            var window = App.FindRoot(VisualRoot);
            window.Info.Show(App.GetLanguage("AddModPackWindow.Error1"));
            return;
        }

        Install();
    }

    public void Closed()
    {
        foreach (var item in ListBox_Items.Children)
        {
            if (item is FileItemControl con)
            {
                con.Dispose();
            }
        }
        ListBox_Items.Children.Clear();

        App.AddModPackWindow = null;
    }

    public void Install()
    {
        App.CrossFade300.Start(null, Grid1, CancellationToken.None);
        Load1();
    }

    public void Install1(FileDisplayObj data)
    {
        App.ShowAddGame();
        if (data.SourceType == SourceType.CurseForge)
        {
            App.AddGameWindow?.Install(
                (data.Data as CurseForgeObjList.Data.LatestFiles)!,
                (Last!.Data.Data as CurseForgeObjList.Data)!);
        }
        else if (data.SourceType == SourceType.Modrinth)
        {
            App.AddGameWindow?.Install(
                (data.Data as ModrinthVersionObj)!,
                (Last!.Data.Data as ModrinthSearchObj.Hit)!);
        }
        else if (data.SourceType == SourceType.FTB)
        {
            App.AddGameWindow?.Install(
                (data.Data as FTBModpackObj.Versions)!,
                (Last!.Data.Data as FTBModpackObj)!);
        }
        var window = App.FindRoot(VisualRoot);
        window.Close();
    }

    public void SetSelect(FileItemControl last)
    {
        Button2.IsEnabled = true;
        Last?.SetSelect(false);
        Last = last;
        Last.SetSelect(true);
    }

    private async void Load()
    {
        var window = App.FindRoot(VisualRoot);
        if (ComboBox1.SelectedIndex == 2 && ComboBox2.SelectedIndex == 4
            && Input1.Text?.Length < 3)
        {
            window.Info.Show(App.GetLanguage("AddModPackWindow.Error6"));
            Unlock();
            return;
        }

        window.Info1.Show(App.GetLanguage("AddModPackWindow.Info2"));
        var data = await WebBinding.GetPackList((SourceType)ComboBox1.SelectedIndex,
            ComboBox3.SelectedItem as string, Input1.Text, (int)Input2.Value!,
            ComboBox1.SelectedIndex == 2 ? ComboBox2.SelectedIndex : ComboBox4.SelectedIndex,
            ComboBox1.SelectedIndex == 2 ? "" :
                ComboBox2.SelectedIndex < 0 ? "" : Categories[ComboBox2.SelectedIndex]);

        if (data == null)
        {
            window.Info.Show(App.GetLanguage("AddModPackWindow.Error2"));
            window.Info1.Close();
            Unlock();
            return;
        }

        foreach (var item in ListBox_Items.Children)
        {
            if (item is FileItemControl con)
            {
                con.Dispose();
            }
        }
        ListBox_Items.Children.Clear();
        int a = 0;
        foreach (var item in data)
        {
            var control = new FileItemControl(item);
            ListBox_Items.Children.Add(control);
            a++;
        }

        Last?.SetSelect(false);
        Last = null;

        Grid3.IsVisible = ListBox_Items.Children.Count == 0;

        ScrollViewer1.ScrollToHome();
        window.Info1.Close();
        Unlock();
    }

    private async void Load1()
    {
        var window = App.FindRoot(VisualRoot);
        List1.Clear();
        window.Info1.Show(App.GetLanguage("AddModPackWindow.Info3"));
        List<FileDisplayObj>? list = null;
        if (ComboBox1.SelectedIndex == 0)
        {
            Input3.IsEnabled = true;
            list = await WebBinding.GetPackFile((SourceType)ComboBox1.SelectedIndex,
                (Last!.Data.Data as CurseForgeObjList.Data)!.id.ToString(), (int)Input3.Value!,
                ComboBox6.SelectedItem as string, Loaders.Normal);
        }
        else if (ComboBox1.SelectedIndex == 1)
        {
            Input3.IsEnabled = false;
            list = await WebBinding.GetPackFile((SourceType)ComboBox1.SelectedIndex,
                (Last!.Data.Data as ModrinthSearchObj.Hit)!.project_id, (int)Input3.Value!,
                ComboBox6.SelectedItem as string, Loaders.Normal);
        }
        else if (ComboBox1.SelectedIndex == 2)
        {
            Input3.IsEnabled = false;
            list = WebBinding.GetFTBPackFile(Last!.Data.Data as FTBModpackObj);
        }
        if (list == null)
        {
            window.Info.Show(App.GetLanguage("AddModPackWindow.Error3"));
            window.Info1.Close();
            return;
        }
        List1.AddRange(list);

        window.Info1.Close();
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(Input1.Text) && Input2.Value != 0)
        {
            Input2.Value = 0;
            return;
        }

        Load();
    }

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("AddModPackWindow.Title"));

        DataGridFiles.MakeTran();

        ComboBox1.SelectedIndex = 0;
    }
}
