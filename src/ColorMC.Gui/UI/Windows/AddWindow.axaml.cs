using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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

public partial class AddWindow : Window
{
    private readonly List<FileItemControl> List = new();
    private readonly ObservableCollection<FileDisplayObj> List1 = new();
    private FileItemControl? Last;
    private GameSettingObj Obj;

    public AddWindow()
    {
        InitializeComponent();

        this.Init();
        Icon = App.Icon;
        Border1.MakeResizeDrag(this);

        ComboBox1.Items = GameBinding.GetCurseForgeSortTypes();
        ComboBox3.Items = GameBinding.GetSortOrder();

        ComboBox1.SelectedIndex = 1;
        ComboBox3.SelectedIndex = 1;

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

    public void Go(FileType file)
    { 
        
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;

        Head.Title = Title = string.Format(App.GetLanguage("AddWindow.Title"), obj.Name);
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
        //var item = DataGridFiles.SelectedItem as FileDisplayObj;
        //if (item == null)
        //    return;

        //var res = await Info.ShowWait(
        //    string.Format(App.GetLanguage("AddWorldWindow.Info1"),
        //    item.Data.displayName));
        //if (res)
        //{
        //    Install1(item.Data);
        //}
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
            Info.Show(App.GetLanguage("AddWorldWindow.Error1"));
            return;
        }

        Install();
    }

    private void AddModPackWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;

        App.AddWindows.Remove(Obj);
    }

    public void Install()
    {
        App.CrossFade300.Start(null, GridVersion, CancellationToken.None);
        Load1();
    }

    public async void Install1(CurseForgeObj.Data.LatestFiles data)
    {
        Info1.Show(App.GetLanguage("GameEditWindow.Tab5.Info5"));
        var res = await GameBinding.DownloadWorld(Obj!, data);
        Info1.Close();
        if (res)
        {
            Info2.Show(App.GetLanguage("GameEditWindow.Tab5.Info4"));
        }
        else
        {
            Info.Show(App.GetLanguage("GameEditWindow.Tab5.Error2"));
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
        //Info1.Show(App.GetLanguage("AddWorldWindow.Info2"));
        //var data = await GameBinding.GetWorldList(ComboBox2.SelectedItem as string,
        //    ComboBox1.SelectedIndex + 1, Input1.Text, (int)Input2.Value!, ComboBox3.SelectedIndex);

        //if (data == null)
        //{
        //    Info.Show(App.GetLanguage("AddWorldWindow.Error2"));
        //    Info1.Close();
        //    return;
        //}

        //ListBox_Items.Children.Clear();
        //int a = 0;
        //foreach (var item in data.data)
        //{
        //    var control = List[a];
        //    control.Load(item);
        //    ListBox_Items.Children.Add(control);
        //    a++;
        //}

        //ScrollViewer1.ScrollToHome();
        //Info1.Close();
    }

    private async void Load1()
    {
        //List1.Clear();
        //Info1.Show(App.GetLanguage("AddWorldWindow.Info3"));
        //var data = await GameBinding.GetPackFile(Last!.Data.id, (int)Input3.Value!);

        //if (data == null)
        //{
        //    Info.Show(App.GetLanguage("AddWorldWindow.Error3"));
        //    Info1.Close();
        //    return;
        //}

        //foreach (var item in data.data)
        //{
        //    List1.Add(new()
        //    {
        //        Name = item.displayName,
        //        Size = UIUtils.MakeFileSize1(item.fileLength),
        //        Download = item.downloadCount,
        //        Time = DateTime.Parse(item.fileDate).ToString(),
        //        Data = item
        //    });
        //}
        //Info1.Close();
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private async void AddModPackWindow_Opened(object? sender, EventArgs e)
    {
        DataGridFiles.MakeTran();
        Info1.Show(App.GetLanguage("AddWorldWindow.Info4"));
        var list = await GameBinding.GetCurseForgeGameVersions();
        Info1.Close();
        if (list == null)
        {
#if !DEBUG
            Info.Show(App.GetLanguage("AddWorldWindow.Error4"));
#endif
            return;
        }

        ComboBox2.Items = list;
        if (list.Contains(Obj!.Version))
        {
            ComboBox2.SelectedItem = Obj.Version;
        }
        Load();
    }

    public void Update()
    {
        App.Update(this, Image_Back, Border1, Border2);
    }
}
