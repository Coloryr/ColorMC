using Avalonia;
using Avalonia.Animation;
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
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public partial class AddModWindow : Window
{
    private readonly List<CurseForge1Control> List = new();
    private readonly ObservableCollection<FileDisplayObj> List1 = new();
    private CurseForge1Control? Last;
    private Tab4Control Tab;
    private GameSettingObj Obj;
    public AddModWindow()
    {
        InitializeComponent();

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

        Opened += AddModWindow_Opened;
        Closed += AddModWindow_Closed;

        for (int a = 0; a < 50; a++)
        {
            List.Add(new());
        }

        App.PicUpdate += Update;

        Update();
    }

    public void SetTab4Control(GameSettingObj obj, Tab4Control tab)
    {
        Obj = obj;
        Tab = tab;

        Head.Title = string.Format(Localizer.Instance["AddModWindow.Title"], obj.Name);
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
            string.Format(Localizer.Instance["AddModWindow.Info1"], item.File.displayName));
        if (res)
        {
            Install1(item.File);
        }
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
            Info.Show(Localizer.Instance["AddModWindow.Error1"]);
            return;
        }

        Install();
    }

    private void AddModWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;

        Tab.CloseAddMod();
    }

    public void Install()
    {
        App.CrossFade300.Start(null, GridVersion, CancellationToken.None);
        Load1();
    }

    public async void Install1(CurseForgeObj.Data.LatestFiles data)
    {
        _ = App.CrossFade300.Start(GridVersion, null, CancellationToken.None);
        var con = Last;
        con?.Download();
        var res = await GameBinding.DownloadMod(Obj, data);
        if (!res)
        {
            Info.Show(Localizer.Instance["AddModWindow.Error3"]);
            return;
        }
        await Task.Run(() => GameBinding.AddModInfo(Obj, data));
        con?.SetDownloadDone(true);
    }

    public void SetSelect(CurseForge1Control last)
    {
        Button2.IsEnabled = true;
        Last?.SetSelect(false);
        Last = last;
        Last.SetSelect(true);
    }

    private async void Load()
    {
        Info1.Show(Localizer.Instance["AddModWindow.Info2"]);
        var data = await GameBinding.GetModList(ComboBox2.SelectedItem as string,
            ComboBox1.SelectedIndex + 1, Input1.Text, (int)Input2.Value!, ComboBox3.SelectedIndex);
        Info1.Close();
        if (data == null)
        {
            Info.Show(Localizer.Instance["AddModWindow.Error2"]);
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
            if (Obj.Datas.ContainsKey(item.id))
            {
                control.SetDownloadDone(true);
            }
            else
            {
                control.SetDownloadDone(false);
            }
            a++;
        }

        ScrollViewer1.ScrollToHome();
    }

    private async void Load1()
    {
        List1.Clear();
        Info1.Show(Localizer.Instance["AddModWindow.Info3"]);
        var data = await GameBinding.GetPackFile(Last!.Data.id, (int)Input3.Value!);
        Info1.Close();

        if (data == null)
        {
            Info.Show(Localizer.Instance["AddModWindow.Error4"]);
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
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private async void AddModWindow_Opened(object? sender, EventArgs e)
    {
        DataGridFiles.MakeTran();
        Info1.Show(Localizer.Instance["AddModWindow.Info4"]);
        var list = await GameBinding.GetCurseForgeGameVersions();
        Info1.Close();
        if (list == null)
        {
            //Info.Show(Localizer.Instance["AddModWindow.Error5"]);
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
