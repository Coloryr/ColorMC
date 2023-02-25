using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Add;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace ColorMC.Gui.UI.Windows;

public partial class AddModPackWindow : Window, IAddWindow
{
    /// <summary>
    /// 控件
    /// </summary>
    private readonly List<FileItemControl> List = new();
    /// <summary>
    /// 类型
    /// </summary>
    private readonly Dictionary<int, string> Categories = new();
    /// <summary>
    /// 数据
    /// </summary>
    private readonly ObservableCollection<FileDisplayObj> List1 = new();
    private FileItemControl? Last;
    private bool load = false;

    public AddModPackWindow()
    {
        InitializeComponent();

        this.Init();
        Icon = App.Icon;
        Border1.MakeResizeDrag(this);

        ComboBox4.Items = GameBinding.GetSourceList();

        ComboBox1.SelectionChanged += ComboBox_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox_SelectionChanged;
        ComboBox3.SelectionChanged += ComboBox_SelectionChanged;
        ComboBox4.SelectionChanged += ComboBox4_SelectionChanged;

        DataGridFiles.Items = List1;
        DataGridFiles.DoubleTapped += DataGridFiles_DoubleTapped;

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        ButtonSearch.Click += ButtonSearch_Click;
        ButtonCancel.Click += ButtonCancel_Click;
        ButtonDownload.Click += ButtonDownload_Click;

        Input2.PropertyChanged += Input2_PropertyChanged;
        Input3.PropertyChanged += Input3_PropertyChanged;

        Opened += AddModPackWindow_Opened;
        Closed += AddModPackWindow_Closed;

        for (int a = 0; a < 50; a++)
        {
            List.Add(new());
        }

        App.PicUpdate += Update;

        Update();
    }

    private void ComboBox_SelectionChanged(object? sender,
        SelectionChangedEventArgs e)
    {
        if (load)
            return;

        Load();
    }

    private async void ComboBox4_SelectionChanged(object? sender,
        SelectionChangedEventArgs e)
    {
        load = true;
        ComboBox1.Items = null;
        ComboBox2.Items = null;
        ComboBox3.Items = null;
        ListBox_Items.Children.Clear();

        if (ComboBox4.SelectedIndex == 0)
        {
            ComboBox3.Items = GameBinding.GetCurseForgeSortTypes();

            Info1.Show(App.GetLanguage("AddModPackWindow.Info4"));
            var list = await GameBinding.GetCurseForgeGameVersions();
            var list1 = await GameBinding.GetCurseForgeCategories();
            Info1.Close();
            if (list == null || list1 == null)
            {
#if !DEBUG
            Info.Show(App.GetLanguage("AddModPackWindow.Error4"));
#endif
                return;
            }

            Categories.Clear();
            Categories.Add(0, "");
            int a = 1;
            foreach (var item in list1)
            {
                Categories.Add(a++, item.Key);
            }

            var list2 = new List<string>()
            {
                ""
            };

            list2.AddRange(list1.Values);

            ComboBox2.Items = list;
            ComboBox1.Items = list2;

            ComboBox1.SelectedIndex = 0;
            ComboBox2.SelectedIndex = 0;
            ComboBox3.SelectedIndex = 1;

            Load();
        }
        else if (ComboBox4.SelectedIndex == 1)
        {
            ComboBox3.Items = GameBinding.GetModrinthSortTypes();

            Info1.Show(App.GetLanguage("AddModPackWindow.Info4"));
            var list = await GameBinding.GetModrinthGameVersions();
            var list1 = await GameBinding.GetModrinthCategories();
            Info1.Close();
            if (list == null || list1 == null)
            {
#if !DEBUG
            Info.Show(App.GetLanguage("AddModPackWindow.Error4"));
#endif
                return;
            }

            Categories.Clear();
            Categories.Add(0, "");
            int a = 1;
            foreach (var item in list1)
            {
                Categories.Add(a++, item.Key);
            }

            var list2 = new List<string>()
            {
                ""
            };

            list2.AddRange(list1.Values);

            ComboBox2.Items = list;
            ComboBox1.Items = list2;

            ComboBox1.SelectedIndex = 0;
            ComboBox2.SelectedIndex = 0;
            ComboBox3.SelectedIndex = 0;

            Load();
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
        var item = DataGridFiles.SelectedItem as FileDisplayObj;
        if (item == null)
            return;

        var res = await Info.ShowWait(
            string.Format(App.GetLanguage("AddModPackWindow.Info1"), item.Name));
        if (res)
        {
            Install1(item);
        }
    }

    private void ButtonCancel_Click(object? sender, RoutedEventArgs e)
    {
        App.CrossFade300.Start(GridVersion, null, CancellationToken.None);
    }

    private void ButtonSearch_Click(object? sender, RoutedEventArgs e)
    {
        Load1();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        if (Last == null)
        {
            Info.Show(App.GetLanguage("AddModPackWindow.Error1"));
            return;
        }

        Install();
    }

    private void AddModPackWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;

        App.AddModPackWindow = null;
    }

    public void Install()
    {
        App.CrossFade300.Start(null, GridVersion, CancellationToken.None);
        Load1();
    }

    public void Install1(FileDisplayObj data)
    {
        App.ShowAddGame();
        if (data.SourceType == SourceType.CurseForge)
        {
            App.AddGameWindow!.Install(
                (data.Data as CurseForgeObj.Data.LatestFiles)!,
                (Last!.Data.Data as CurseForgeObj.Data)!);
        }
        else if (data.SourceType == SourceType.Modrinth)
        {
            App.AddGameWindow!.Install(
                (data.Data as ModrinthVersionObj)!,
                (Last!.Data.Data as ModrinthSearchObj.Hit)!);
        }
        Close();
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
        Info1.Show(App.GetLanguage("AddModPackWindow.Info2"));
        var data = await GameBinding.GetPackList(
            (SourceType)ComboBox4.SelectedIndex, ComboBox2.SelectedItem as string,
            Input1.Text, (int)Input2.Value!, ComboBox3.SelectedIndex,
            ComboBox1.SelectedIndex < 0 ? "" :
                Categories[ComboBox1.SelectedIndex]);

        if (data == null)
        {
            Info.Show(App.GetLanguage("AddModPackWindow.Error2"));
            Info1.Close();
            return;
        }

        ListBox_Items.Children.Clear();
        int a = 0;
        foreach (var item in data)
        {
            var control = List[a];
            control.Load(item);
            ListBox_Items.Children.Add(control);
            a++;
        }

        ScrollViewer1.ScrollToHome();
        Info1.Close();
    }

    private async void Load1()
    {
        List1.Clear();
        Info1.Show(App.GetLanguage("AddModPackWindow.Info3"));
        List<FileDisplayObj>? list = null;
        if (ComboBox4.SelectedIndex == 0)
        {
            Input3.IsEnabled = true;
            list = await GameBinding.GetPackFile((SourceType)ComboBox4.SelectedIndex, (Last!.Data.Data as CurseForgeObj.Data)!.id.ToString(), (int)Input3.Value!);
        }
        else if (ComboBox4.SelectedIndex == 1)
        {
            Input3.IsEnabled = false;
            list = await GameBinding.GetPackFile((SourceType)ComboBox4.SelectedIndex, (Last!.Data.Data as ModrinthSearchObj.Hit)!.project_id, (int)Input3.Value!);
        }
        if (list == null)
        {
            Info.Show(App.GetLanguage("AddModPackWindow.Error3"));
            Info1.Close();
            return;
        }
        List1.AddRange(list);

        Info1.Close();
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void AddModPackWindow_Opened(object? sender, EventArgs e)
    {
        DataGridFiles.MakeTran();

        ComboBox4.SelectedIndex = 0;
    }

    public void Update()
    {
        App.Update(this, Image_Back, Border1, Border2);
    }
}
