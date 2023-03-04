using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Add;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace ColorMC.Gui.UI.Windows;

public partial class AddWindow : Window, IAddWindow
{
    /// <summary>
    /// 控件
    /// </summary>
    private readonly List<FileItemControl> List = new();
    /// <summary>
    /// 下载源
    /// </summary>
    private List<SourceType> List2 = new();
    private readonly ObservableCollection<string> List3 = new();
    /// <summary>
    /// 类型
    /// </summary>
    private readonly Dictionary<int, string> Categories = new();
    /// <summary>
    /// 数据
    /// </summary>
    private readonly ObservableCollection<FileDisplayObj> List1 = new();

    private FileItemControl? Last;
    private GameSettingObj Obj;
    private bool load = false;
    private bool display = false;
    private FileType now;

    public AddWindow()
    {
        InitializeComponent();

        this.Init();
        Icon = App.Icon;
        Border1.MakeResizeDrag(this);

        ComboBox1.Items = GameBinding.GetAddType();
        ComboBox2.Items = List3;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox2_SelectionChanged;
        ComboBox3.SelectionChanged += ComboBox_SelectionChanged;
        ComboBox4.SelectionChanged += ComboBox_SelectionChanged;
        ComboBox5.SelectionChanged += ComboBox_SelectionChanged;

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

    private void ComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!display || load)
            return;

        Load();
    }

    private async void ComboBox2_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!display)
            return;

        load = true;

        ComboBox3.Items = null;
        ComboBox4.Items = null;
        ComboBox5.Items = null;

        foreach (var item in ListBox_Items.Children)
        {
            (item as FileItemControl)?.Cancel();
        }
        ListBox_Items.Children.Clear();

        var type = List2[ComboBox2.SelectedIndex];
        if (type == SourceType.CurseForge)
        {
            ComboBox4.Items = GameBinding.GetCurseForgeSortTypes();

            Info1.Show(App.GetLanguage("AddModPackWindow.Info4"));
            var list = await GameBinding.GetCurseForgeGameVersions();
            var list1 = await GameBinding.GetCurseForgeCategories(now);
            if (list == null || list1 == null)
            {
#if !DEBUG
            Info.Show(App.GetLanguage("AddModPackWindow.Error4"));
#endif
                Info1.Close();
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

            ComboBox3.Items = list;
            ComboBox5.Items = list2;

            ComboBox3.SelectedIndex = 0;
            ComboBox4.SelectedIndex = 1;
            ComboBox5.SelectedIndex = 0;

            Load();
        }
        else if (type == SourceType.Modrinth)
        {
            ComboBox4.Items = GameBinding.GetModrinthSortTypes();

            Info1.Show(App.GetLanguage("AddModPackWindow.Info4"));
            var list = await GameBinding.GetModrinthGameVersions();
            var list1 = await GameBinding.GetModrinthCategories(now);
            if (list == null || list1 == null)
            {
#if !DEBUG
            Info.Show(App.GetLanguage("AddModPackWindow.Error4"));
#endif
                Info1.Close();
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

            ComboBox3.Items = list;
            ComboBox5.Items = list2;

            ComboBox3.SelectedIndex = 0;
            ComboBox4.SelectedIndex = 0;
            ComboBox5.SelectedIndex = 0;

            Load();
        }

        load = false;
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!display)
            return;

        load = true;
        now = (FileType)(ComboBox1.SelectedIndex + 1);
        ComboBox3.Items = null;
        ComboBox4.Items = null;
        ComboBox5.Items = null;

        Input2.Value = 0;

        List1.Clear();
        List3.Clear();

        if(now == FileType.Optifne)
        {
            OptifineOpen();
            return;
        }

        List2 = GameBinding.GetSourceList(now);
        List2.ForEach(item => List3.Add(item.GetName()));

        ComboBox2.SelectedIndex = 0;
    }

    public void Go(FileType file)
    {
        if (file == FileType.Optifne)
        {
            OptifineOpen();
        }
        else
        {
            ComboBox1.SelectedIndex = (int)file - 1;
        }
    }

    private void OptifineOpen()
    {
        App.CrossFade300.Start(null, Optifine, CancellationToken.None);

        Optifine.Load();
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;

        Head.Title = Title = string.Format(App.GetLanguage("AddWindow.Title"), obj.Name);

        Optifine.SetGame(obj);
    }

    private void Input3_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (!display || load)
            return;

        var property = e.Property.Name;
        if (property == "Value")
        {
            Load1();
        }
    }

    private void Input2_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (!display || load)
            return;

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
            string.Format(App.GetLanguage("AddWindow.Info1"),
            item.Name));
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
            Info.Show(App.GetLanguage("AddWindow.Error1"));
            return;
        }

        Install();
    }

    private void AddModPackWindow_Closed(object? sender, EventArgs e)
    {
        foreach (var item in ListBox_Items.Children)
        {
            if (item is not FileItemControl control)
                return;

            control.Close();
        }
        ListBox_Items.Children.Clear();

        App.PicUpdate -= Update;

        App.AddWindows.Remove(Obj.UUID);
    }

    public void Install()
    {
        App.CrossFade300.Start(null, GridVersion, CancellationToken.None);
        Load1();
    }

    public async void Install1(FileDisplayObj data)
    {
        var last = Last!;
        last.SetNowDownload();
        await App.CrossFade300.Start(GridVersion, null, CancellationToken.None);
        var type = List2[ComboBox2.SelectedIndex];
        bool res = false;
        if (now == FileType.DataPacks)
        {
            var list = await Obj.GetWorlds();
            if (list.Count == 0)
            {
                Info.Show(App.GetLanguage("AddWindow.Error6"));
                return;
            }

            var world = new List<string>();
            list.ForEach(item => world.Add(item.LevelName));
            await Info5.Show(App.GetLanguage("AddWindow.Info7"), world);
            if (Info5.Cancel)
                return;
            var item = list[Info5.Read().Item1];
            res = type switch
            {
                SourceType.CurseForge => await GameBinding.Download(item,
                    data.Data as CurseForgeObj.Data.LatestFiles),
                SourceType.Modrinth => await GameBinding.Download(item,
                    data.Data as ModrinthVersionObj)
            };
        }
        else
        {
            res = type switch
            {
                SourceType.CurseForge => await GameBinding.Download(now, Obj!, data.Data as CurseForgeObj.Data.LatestFiles),
                SourceType.Modrinth => await GameBinding.Download(now, Obj!, data.Data as ModrinthVersionObj)
            };
        }
        if (res)
        {
            Info2.Show(App.GetLanguage("AddWindow.Info6"));
            last.SetDownloaded();
        }
        else
        {
            Info.Show(App.GetLanguage("AddWindow.Error5"));
        }
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
        Info1.Show(App.GetLanguage("AddWindow.Info2"));
        var data = await GameBinding.GetList(now, List2[ComboBox2.SelectedIndex], ComboBox3.SelectedItem as string,
            Input1.Text, (int)Input2.Value!, ComboBox4.SelectedIndex,
            ComboBox5.SelectedIndex < 0 ? "" :
                Categories[ComboBox5.SelectedIndex]);

        if (data == null)
        {
            Info1.Close();
            Info.Show(App.GetLanguage("AddWindow.Error2"));
            return;
        }

        ListBox_Items.Children.Clear();
        int a = 0;
        if (now == FileType.Mod)
        {
            foreach (var item in data)
            {
                if (Obj.Mods.ContainsKey(item.ID))
                {
                    item.IsDownload = true;
                }
                var control = List[a];
                control.Load(item);
                ListBox_Items.Children.Add(control);
                a++;
            }
        }
        else
        {
            foreach (var item in data)
            {
                var control = List[a];
                control.Load(item);
                ListBox_Items.Children.Add(control);
                a++;
            }
        }

        ScrollViewer1.ScrollToHome();
        Info1.Close();
    }

    public void OptifineClsoe()
    {
        App.CrossFade300.Start(Optifine, null, CancellationToken.None);

        ComboBox1.SelectedIndex = 0;
    }

    private async void Load1()
    {
        List1.Clear();
        Info1.Show(App.GetLanguage("AddWindow.Info3"));
        List<FileDisplayObj>? list = null;
        var type = List2[ComboBox2.SelectedIndex];
        if (type == SourceType.CurseForge)
        {
            Input3.IsEnabled = true;
            list = await GameBinding.GetPackFile(type, (Last!.Data.Data as CurseForgeObj.Data)!.id.ToString(), (int)Input3.Value!, now);
        }
        else if (type == SourceType.Modrinth)
        {
            Input3.IsEnabled = false;
            list = await GameBinding.GetPackFile(type, (Last!.Data.Data as ModrinthSearchObj.Hit)!.project_id, (int)Input3.Value!, now);
        }
        if (list == null)
        {
            Info.Show(App.GetLanguage("AddWindow.Error3"));
            Info1.Close();
            return;
        }

        if (now == FileType.Mod)
        {
            foreach (var item in list)
            {
                if (Obj.Mods.TryGetValue(item.ID, out var value)
                    && value.FileId == item.ID1)
                {
                    item.IsDownload = true;
                }
                List1.Add(item);
            }
        }
        else
        {
            foreach (var item in list)
            {
                List1.Add(item);
            }
        }

        Info1.Close();
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void AddModPackWindow_Opened(object? sender, EventArgs e)
    {
        DataGridFiles.MakeTran();

        display = true;
    }

    public void Update()
    {
        App.Update(this, Image_Back, Border1, Border2);
    }
}
