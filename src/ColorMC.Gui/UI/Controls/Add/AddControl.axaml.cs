using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.Utils;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddControl : UserControl, IUserControl, IAddWindow
{
    private readonly List<FileItemControl> List = new();
    private List<SourceType> List2 = new();
    private readonly ObservableCollection<string> List3 = new();
    private readonly Dictionary<int, string> Categories = new();
    private readonly ObservableCollection<FileDisplayObj> List1 = new();
    private readonly ObservableCollection<string> List4 = new();

    /// <summary>
    /// Optifine
    /// </summary>
    public readonly List<OptifineDisplayObj> List5 = new();
    public readonly ObservableCollection<OptifineDisplayObj> List6 = new();

    private FileItemControl? Last;
    private GameSettingObj Obj;
    private bool load = false;
    private bool display = false;
    private FileType now;
    private bool set;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public AddControl() : this(null)
    {

    }

    public AddControl(GameSettingObj? obj)
    {
        Obj = obj;

        InitializeComponent();

        ComboBox1.ItemsSource = GameBinding.GetAddType();
        ComboBox2.ItemsSource = List3;

        ComboBox3.ItemsSource = List4;
        ComboBox6.ItemsSource = List4;
        ComboBox7.ItemsSource = List4;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox2_SelectionChanged;
        ComboBox3.SelectionChanged += ComboBox_SelectionChanged;
        ComboBox4.SelectionChanged += ComboBox_SelectionChanged;
        ComboBox5.SelectionChanged += ComboBox_SelectionChanged;
        ComboBox6.SelectionChanged += ComboBox6_SelectionChanged;

        DataGridFiles.Items = List1;
        DataGridFiles.DoubleTapped += DataGridFiles_DoubleTapped;

        DataGrid1.Items = List6;
        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        ButtonSearch.Click += ButtonSearch_Click;
        ButtonCancel.Click += ButtonCancel_Click;
        ButtonDownload.Click += ButtonDownload_Click;
        Button3.Click += Button3_Click;
        Button4.Click += Button4_Click;
        Button5.Click += Button5_Click;

        Input2.PropertyChanged += Input2_PropertyChanged;
        Input3.PropertyChanged += Input3_PropertyChanged;

        for (int a = 0; a < 20; a++)
        {
            List.Add(new());
        }
    }

    private void DataGrid1_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        Button5_Click(null, null);
    }

    private async void Button5_Click(object? sender, RoutedEventArgs e)
    {
        if (DataGrid1.SelectedItem is not OptifineDisplayObj item)
            return;

        var window = App.FindRoot(VisualRoot);
        var res = await window.Info.ShowWait(string.Format(
            App.GetLanguage("AddGameWindow.Control1.Info1"), item.Version));
        if (!res)
            return;
        window.Info1.Show(App.GetLanguage("AddGameWindow.Control1.Info2"));
        var res1 = await GameBinding.DownloadOptifine(Obj, item);
        window.Info1.Close();
        if (res1.Item1 == false)
        {
            window.Info.Show(res1.Item2!);
        }
        else
        {
            window.Info2.Show(App.GetLanguage("AddGameWindow.Control1.Info3"));
            (window as AddControl)?.OptifineClsoe();
        }
    }

    public async void Load2()
    {
        var window = App.FindRoot(VisualRoot);
        List4.Clear();
        List5.Clear();
        List6.Clear();
        window.Info1.Show(App.GetLanguage("AddGameWindow.Control1.Info4"));
        var list = await GameBinding.GetOptifine();
        window.Info1.Close();
        if (list == null)
        {
            window.Info.Show(App.GetLanguage("AddGameWindow.Control1.Error1"));
            return;
        }

        List5.AddRange(list);

        List4.Add("");
        List4.AddRange(from item in list
                       group item by item.MC into newgroup
                       select newgroup.Key);

        Load3();
    }

    public void Load3()
    {
        List6.Clear();
        if (ComboBox7.SelectedItem is not string item
            || string.IsNullOrWhiteSpace(item))
        {
            List6.AddRange(List5);
        }
        else
        {
            List6.AddRange(from item1 in List5
                           where item1.MC == item
                           select item1);
        }
    }

    private void Button4_Click(object? sender, RoutedEventArgs e)
    {
        Load2();
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        OptifineClsoe();
    }

    private void ComboBox6_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!display || load)
            return;

        Load1();
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

        var window = App.FindRoot(VisualRoot);
        load = true;

        List4.Clear();
        ComboBox4.ItemsSource = null;
        ComboBox5.ItemsSource = null;

        foreach (var item in ListBox_Items.Children)
        {
            (item as FileItemControl)?.Cancel();
        }
        ListBox_Items.Children.Clear();

        var type = List2[ComboBox2.SelectedIndex];
        if (type == SourceType.CurseForge)
        {
            ComboBox4.ItemsSource = GameBinding.GetCurseForgeSortTypes();

            window.Info1.Show(App.GetLanguage("AddModPackWindow.Info4"));
            var list = await GameBinding.GetCurseForgeGameVersions();
            var list1 = await GameBinding.GetCurseForgeCategories(now);
            if (list == null || list1 == null)
            {
#if !DEBUG
                window.Info.Show(App.GetLanguage("AddModPackWindow.Error4"));
#endif
                window.Info1.Close();
                return;
            }
            List4.AddRange(list);

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

            ComboBox3.ItemsSource = list;
            ComboBox5.ItemsSource = list2;

            if (List4.Contains(Obj.Version))
            {
                ComboBox6.SelectedItem = ComboBox3.SelectedItem = Obj.Version;
            }
            else
            {
                ComboBox3.SelectedIndex = 0;
            }

            ComboBox4.SelectedIndex = 1;
            ComboBox5.SelectedIndex = 0;

            Load();
        }
        else if (type == SourceType.Modrinth)
        {
            ComboBox4.ItemsSource = GameBinding.GetModrinthSortTypes();

            window.Info1.Show(App.GetLanguage("AddModPackWindow.Info4"));
            var list = await GameBinding.GetModrinthGameVersions();
            var list1 = await GameBinding.GetModrinthCategories(now);
            if (list == null || list1 == null)
            {
#if !DEBUG
                window.Info.Show(App.GetLanguage("AddModPackWindow.Error4"));
#endif
                window.Info1.Close();
                return;
            }
            List4.AddRange(list);

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

            ComboBox3.ItemsSource = list;
            ComboBox5.ItemsSource = list2;

            if (List4.Contains(Obj.Version))
            {
                ComboBox6.SelectedItem = ComboBox3.SelectedItem = Obj.Version;
            }
            else
            {
                ComboBox3.SelectedIndex = 0;
            }

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

        if (ComboBox1.SelectedIndex == 5)
        {
            OptifineOpen();
            return;
        }

        load = true;

        now = (FileType)(ComboBox1.SelectedIndex + 1);
        ComboBox3.ItemsSource = null;
        ComboBox4.ItemsSource = null;
        ComboBox5.ItemsSource = null;

        Input2.Value = 0;

        List1.Clear();
        List3.Clear();

        List2 = GameBinding.GetSourceList(now);
        List2.ForEach(item => List3.Add(item.GetName()));
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

            ComboBox2.SelectedIndex = 0;
        }
    }

    private void OptifineOpen()
    {
        App.CrossFade300.Start(null, Grid2, CancellationToken.None);

        Load2();
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
        var window = App.FindRoot(VisualRoot);
        var item = DataGridFiles.SelectedItem as FileDisplayObj;
        if (item == null)
            return;

        var res = await window.Info.ShowWait(
            string.Format(set ? App.GetLanguage("AddWindow.Info8") : App.GetLanguage("AddWindow.Info1"),
            item.Name));
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
        var window = App.FindRoot(VisualRoot);
        if (Last == null)
        {
            window.Info.Show(App.GetLanguage("AddWindow.Error1"));
            return;
        }

        Install();
    }

    public void Closed()
    {
        foreach (var item in ListBox_Items.Children)
        {
            if (item is not FileItemControl control)
                return;

            control.Close();
        }
        ListBox_Items.Children.Clear();

        App.AddWindows.Remove(Obj.UUID);

        if (set)
            set = false;
    }

    public void Install()
    {
        App.CrossFade300.Start(null, Grid1, CancellationToken.None);
        Load1();
    }

    public async void Install1(FileDisplayObj data)
    {
        var window = App.FindRoot(VisualRoot);
        var type = List2[ComboBox2.SelectedIndex];
        if (set)
        {
            if (type == SourceType.CurseForge)
            {
                GameBinding.SetModInfo(Obj,
                    data.Data as CurseForgeObj.Data.LatestFiles);
            }
            else if (type == SourceType.Modrinth)
            {
                GameBinding.SetModInfo(Obj,
                    data.Data as ModrinthVersionObj);
            }
            window.Close();
            return;
        }
        var last = Last!;
        last?.SetNowDownload();
        await App.CrossFade300.Start(Grid1, null, CancellationToken.None);
        bool res = false;

        if (now == FileType.DataPacks)
        {
            var list = await GameBinding.GetWorlds(Obj);
            if (list.Count == 0)
            {
                window.Info.Show(App.GetLanguage("AddWindow.Error6"));
                return;
            }

            var world = new List<string>();
            list.ForEach(item => world.Add(item.World.LevelName));
            await window.Info5.Show(App.GetLanguage("AddWindow.Info7"), world);
            if (window.Info5.Cancel)
                return;
            var item = list[window.Info5.Read().Item1];

            try
            {
                res = type switch
                {
                    SourceType.CurseForge => await GameBinding.Download(item.World,
                    data.Data as CurseForgeObj.Data.LatestFiles),
                    SourceType.Modrinth => await GameBinding.Download(item.World,
                    data.Data as ModrinthVersionObj)
                };
            }
            catch (Exception e)
            {
                Logs.Error(App.GetLanguage("AddWindow.Error7"), e);
                res = false;
            }
        }
        else
        {
            try
            {
                res = type switch
                {
                    SourceType.CurseForge => await GameBinding.Download(now, Obj!,
                    data.Data as CurseForgeObj.Data.LatestFiles),
                    SourceType.Modrinth => await GameBinding.Download(now, Obj!,
                    data.Data as ModrinthVersionObj)
                };
            }
            catch (Exception e)
            {
                Logs.Error(App.GetLanguage("AddWindow.Error8"), e);
                res = false;
            }
        }
        if (res)
        {
            window.Info2.Show(App.GetLanguage("AddWindow.Info6"));
            if (last == null)
            {
                window.Close();
                return;
            }
            last.SetDownloaded();
        }
        else
        {
            window.Info.Show(App.GetLanguage("AddWindow.Error5"));
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
        var window = App.FindRoot(VisualRoot);
        window.Info1.Show(App.GetLanguage("AddWindow.Info2"));
        var data = await GameBinding.GetList(now, List2[ComboBox2.SelectedIndex],
            ComboBox3.SelectedItem as string, Input1.Text, (int)Input2.Value!,
            ComboBox4.SelectedIndex, ComboBox5.SelectedIndex < 0 ? "" :
                Categories[ComboBox5.SelectedIndex], Obj.Loader);

        if (data == null)
        {
            window.Info1.Close();
            window.Info.Show(App.GetLanguage("AddWindow.Error2"));
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

        Last?.SetSelect(false);
        Last = null;

        ScrollViewer1.ScrollToHome();
        window.Info1.Close();
    }

    public void OptifineClsoe()
    {
        App.CrossFade300.Start(Grid2, null, CancellationToken.None);

        ComboBox1.SelectedIndex = 0;
        ComboBox2.SelectedIndex = 0;
    }

    private async void Load1(string? id = null)
    {
        List1.Clear();

        var window = App.FindRoot(VisualRoot);
        window.Info1.Show(App.GetLanguage("AddWindow.Info3"));
        List<FileDisplayObj>? list = null;
        var type = List2[ComboBox2.SelectedIndex];
        if (type == SourceType.CurseForge)
        {
            Input3.IsEnabled = true;
            list = await GameBinding.GetPackFile(type, id ??
                (Last!.Data.Data as CurseForgeObj.Data)!.id.ToString(), (int)Input3.Value!,
                ComboBox6.SelectedItem as string, Obj.Loader, now);
        }
        else if (type == SourceType.Modrinth)
        {
            Input3.IsEnabled = false;
            list = await GameBinding.GetPackFile(type, id ??
                (Last!.Data.Data as ModrinthSearchObj.Hit)!.project_id, (int)Input3.Value!,
                ComboBox6.SelectedItem as string, Obj.Loader, now);
        }
        if (list == null)
        {
            window.Info.Show(App.GetLanguage("AddWindow.Error3"));
            window.Info1.Close();
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

        window.Info1.Close();
    }

    public async void GoFile(SourceType type, string pid)
    {
        ComboBox1.SelectedIndex = (int)FileType.Mod - 1;
        ComboBox2.SelectedIndex = (int)type;
        await Task.Run(() =>
        {
            while (!display || load)
                Thread.Sleep(1000);
        });

        App.CrossFade300.Start(null, Grid1, CancellationToken.None);
        Load1(pid);
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    public void Opened()
    {
        DataGridFiles.MakeTran();

        Window.SetTitle(string.Format(App.GetLanguage("AddWindow.Title"), Obj.Name));

        display = true;
    }

    public async Task GoSet()
    {
        set = true;

        ComboBox1.SelectedIndex = (int)FileType.Mod - 1;
        ComboBox2.SelectedIndex = 0;
        ComboBox1.IsEnabled = false;
        await Task.Run(() =>
        {
            while (set)
                Thread.Sleep(1000);
        });
    }
}
