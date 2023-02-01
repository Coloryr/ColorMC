using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.CurseForge;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace ColorMC.Gui.UI.Windows;

public partial class AddResourcePackWindow : Window, IBase1Window
{
    private readonly List<CurseForgeControl> List = new();
    private readonly ObservableCollection<FileDisplayObj> List1 = new();
    private CurseForgeControl? Last;
    private Tab8Control Tab;
    private GameSettingObj Obj;

    public AddResourcePackWindow()
    {
        InitializeComponent();

        FontFamily = Program.Font;

        Icon = App.Icon;

        Rectangle1.MakeResizeDrag(this);

        ComboBox1.Items = GameBinding.GetCurseForgeTypes();
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

        Opened += AddCurseForgeWindow_Opened;
        Closed += AddCurseForgeWindow_Closed;

        for (int a = 0; a < 50; a++)
        {
            List.Add(new());
        }

        App.PicUpdate += Update;

        Update();
    }

    public void SetTab8Control(GameSettingObj obj, Tab8Control tab)
    {
        Obj = obj;
        Tab = tab;

        Head.Title = string.Format(Localizer.Instance["AddResourcePackWindow.Title"], obj.Name);
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
            string.Format(Localizer.Instance["AddResourcePackWindow.Info1"],
            item.File.displayName));
        if (res)
        {
            Install1(item.File);
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
            Info.Show(Localizer.Instance["AddResourcePackWindow.Error1"]);
            return;
        }

        Install();
    }

    private void AddCurseForgeWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;

        Tab.CloseAddResourcepack();
    }

    public void Install()
    {
        App.CrossFade300.Start(null, GridVersion, CancellationToken.None);
        Load1();
    }

    public void Install1(CurseForgeObj.Data.LatestFiles data)
    {
        Close();
        Tab.AddResourcepack(data);
    }

    public void SetSelect(CurseForgeControl last)
    {
        Button2.IsEnabled = true;
        Last?.SetSelect(false);
        Last = last;
        Last.SetSelect(true);
    }

    private async void Load()
    {
        Info1.Show(Localizer.Instance["AddResourcePackWindow.Info2"]);
        var data = await GameBinding.GetResourcepackList(ComboBox2.SelectedItem as string,
            ComboBox1.SelectedIndex + 1, Input1.Text, (int)Input2.Value, ComboBox3.SelectedIndex);

        if (data == null)
        {
            Info.Show(Localizer.Instance["AddResourcePackWindow.Error2"]);
            Info1.Close();
            return;
        }

        ListBox_Items.Children.Clear();
        int a = 0;
        foreach (var item in data.data)
        {
            var control = List[a];
            control.SetWindow(this);
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
        Info1.Show(Localizer.Instance["AddResourcePackWindow.Info3"]);
        var data = await GameBinding.GetPackFile(Last!.Data.id, (int)Input3.Value!);

        if (data == null)
        {
            Info.Show(Localizer.Instance["AddResourcePackWindow.Error3"]);
            Info1.Close();
            return;
        }

        foreach (var item in data.data)
        {
            List1.Add(new()
            {
                Name = item.displayName,
                Size = UIUtils.MakeFileSize1(item.fileLength),
                Download = item.downloadCount,
                Time = DateTime.Parse(item.fileDate).ToString(),
                File = item
            });
        }
        Info1.Close();
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private async void AddCurseForgeWindow_Opened(object? sender, EventArgs e)
    {
        DataGridFiles.MakeTran();
        Info1.Show(Localizer.Instance["AddResourcePackWindow.Info4"]);
        var list = await GameBinding.GetCurseForgeGameVersions();
        Info1.Close();
        if (list == null)
        {
            Info.Show(Localizer.Instance["AddWorldWindow.Error4"]);
            return;
        }

        ComboBox2.Items = list;
        if (list.Contains(Obj.Version))
        {
            ComboBox2.SelectedItem = Obj.Version;
        }
        Load();
    }

    public void Update()
    {
        App.Update(this, Image_Back, Grid1);
    }
}
