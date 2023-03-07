using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ColorMC.Core.Utils;
using System.Collections.ObjectModel;
using DynamicData;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddControl : UserControl, IUserControl
{
    /// <summary>
    /// �ؼ�
    /// </summary>
    private readonly List<FileItemControl> List = new();
    /// <summary>
    /// ����Դ
    /// </summary>
    private List<SourceType> List2 = new();
    private readonly ObservableCollection<string> List3 = new();
    /// <summary>
    /// ����
    /// </summary>
    private readonly Dictionary<int, string> Categories = new();
    /// <summary>
    /// ����
    /// </summary>
    private readonly ObservableCollection<FileDisplayObj> List1 = new();
    /// <summary>
    /// ��Ϸ�汾
    /// </summary>
    private readonly ObservableCollection<string> List4 = new();

    private FileItemControl? Last;
    private GameSettingObj Obj;
    private bool load = false;
    private bool display = false;
    private FileType now;
    private bool set;

    public UserControl Con => this;

    public AddControl()
    {
        InitializeComponent();

        ComboBox1.Items = GameBinding.GetAddType();
        ComboBox2.Items = List3;

        ComboBox3.Items = List4;
        ComboBox6.Items = List4;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox2_SelectionChanged;
        ComboBox3.SelectionChanged += ComboBox_SelectionChanged;
        ComboBox4.SelectionChanged += ComboBox_SelectionChanged;
        ComboBox5.SelectionChanged += ComboBox_SelectionChanged;
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

        for (int a = 0; a < 20; a++)
        {
            List.Add(new());
        }
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

        var window = (VisualRoot as IBaseWindow)!;
        load = true;

        List4.Clear();
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

            window.Info1.Show(App.GetLanguage("AddModPackWindow.Info4"));
            var list = await GameBinding.GetCurseForgeGameVersions();
            var list1 = await GameBinding.GetCurseForgeCategories(now);
            if (list == null || list1 == null)
            {
#if !DEBUG
            Info.Show(App.GetLanguage("AddModPackWindow.Error4"));
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

            ComboBox3.Items = list;
            ComboBox5.Items = list2;

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
            ComboBox4.Items = GameBinding.GetModrinthSortTypes();

            window.Info1.Show(App.GetLanguage("AddModPackWindow.Info4"));
            var list = await GameBinding.GetModrinthGameVersions();
            var list1 = await GameBinding.GetModrinthCategories(now);
            if (list == null || list1 == null)
            {
#if !DEBUG
            Info.Show(App.GetLanguage("AddModPackWindow.Error4"));
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

            ComboBox3.Items = list;
            ComboBox5.Items = list2;

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
        ComboBox3.Items = null;
        ComboBox4.Items = null;
        ComboBox5.Items = null;

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
        App.CrossFade300.Start(null, Optifine, CancellationToken.None);

        Optifine.Load();
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;

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
        var window = (VisualRoot as IBaseWindow)!;
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
        App.CrossFade300.Start(GridVersion, null, CancellationToken.None);
    }

    private void ButtonSearch_Click(object? sender, RoutedEventArgs e)
    {
        Load1();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as IBaseWindow)!;
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
        App.CrossFade300.Start(null, GridVersion, CancellationToken.None);
        Load1();
    }

    public async void Install1(FileDisplayObj data)
    {
        var window = (VisualRoot as IBaseWindow)!;
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
            window.Window.Close();
            return;
        }
        var last = Last!;
        last?.SetNowDownload();
        await App.CrossFade300.Start(GridVersion, null, CancellationToken.None);
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

        }
        else
        {
            res = type switch
            {
                SourceType.CurseForge => await GameBinding.Download(now, Obj!,
                data.Data as CurseForgeObj.Data.LatestFiles),
                SourceType.Modrinth => await GameBinding.Download(now, Obj!,
                data.Data as ModrinthVersionObj)
            };
        }
        if (res)
        {
            window.Info2.Show(App.GetLanguage("AddWindow.Info6"));
            if (last == null)
            {
                window.Window.Close();
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
        var window = (VisualRoot as IBaseWindow)!;
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

        ScrollViewer1.ScrollToHome();
        window.Info1.Close();
    }

    public void OptifineClsoe()
    {
        App.CrossFade300.Start(Optifine, null, CancellationToken.None);

        ComboBox1.SelectedIndex = 0;
    }

    private async void Load1(string? id = null)
    {
        List1.Clear();

        var window = (VisualRoot as IBaseWindow)!;
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

        App.CrossFade300.Start(null, GridVersion, CancellationToken.None);
        Load1(pid);
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    public void Opened()
    {
        DataGridFiles.MakeTran();

        var window = (VisualRoot as IBaseWindow)!;
        string name = string.Format(App.GetLanguage("AddWindow.Title"), Obj.Name);

        window.Head.Title = name;
        window.Window.Title = name;

        display = true;
    }

    public async Task GoSet()
    {
        set = true;

        ComboBox1.SelectedIndex = (int)FileType.Mod - 1;
        ComboBox2.SelectedIndex = 0;
        await Task.Run(() =>
        {
            while (set)
                Thread.Sleep(1000);
        });
    }

    public void Update()
    {
        
    }

    public void Closing()
    {
        
    }
}
