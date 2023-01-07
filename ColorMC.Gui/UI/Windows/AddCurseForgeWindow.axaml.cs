using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Gui.Language;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.CurseForge;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace ColorMC.Gui.UI.Windows;

public partial class AddCurseForgeWindow : Window
{
    private readonly List<CurseForgeControl> List = new();
    private readonly ObservableCollection<FileDisplayObj> List1 = new();
    private CurseForgeControl? Last;

    private readonly CrossFade transition = new(TimeSpan.FromMilliseconds(300));

    public AddCurseForgeWindow()
    {
        InitializeComponent();

        this.MakeItNoChrome();
        FontFamily = Program.FontFamily;

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

        Opened += AddCurseForgeWindow_Opened;
        Closed += AddCurseForgeWindow_Closed;

        Update();
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
            string.Format(Localizer.Instance["AddCurseForgeWindow.Info1"], item.File.displayName));
        if (res)
        {
            Install1(item.File);
        }
    }

    private void ButtonCancel_Click(object? sender, RoutedEventArgs e)
    {
        transition.Start(GridVersion, null, CancellationToken.None);
    }

    private void ButtonSearch_Click(object? sender, RoutedEventArgs e)
    {
        Load1();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        if (Last == null)
        {
            Info.Show(Localizer.Instance["AddCurseForgeWindow.Error1"]);
            return;
        }

        Install(Last.Data);
    }

    private void AddCurseForgeWindow_Closed(object? sender, EventArgs e)
    {
        App.AddCurseForgeWindow = null;
    }

    public void Install(CurseForgeObj.Data data)
    {
        transition.Start(null, GridVersion, CancellationToken.None);
        Load1();
    }

    public void Install1(CurseForgeObj.Data.LatestFiles data)
    {
        App.ShowAddGame();
        App.AddGameWindow?.Install(data);
        Close();
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
        Info1.Show(Localizer.Instance["AddCurseForgeWindow.Info2"]);
        var data = await GameBinding.GetPackList(ComboBox2.SelectedItem as string, ComboBox1.SelectedIndex + 1, Input1.Text, int.Parse(Input2.Text), ComboBox3.SelectedIndex);
        Info1.Close();
        if (data == null)
        {
            Info.Show(Localizer.Instance["AddCurseForgeWindow.Error2"]);
            return;
        }

        List.Clear();
        ListBox_Items.Children.Clear();
        foreach (var item in data.data)
        {
            CurseForgeControl control = new();
            control.SetWindow(this);
            control.Load(item);
            List.Add(control);
            ListBox_Items.Children.Add(control);
        }
    }

    private async void Load1()
    {
        List1.Clear();
        Info1.Show(Localizer.Instance["AddCurseForgeWindow.Info3"]);
        var data = await GameBinding.GetPackFile(Last!.Data.id, int.Parse(Input3.Text));
        Info1.Close();

        if (data == null)
        {
            Info.Show(Localizer.Instance["AddCurseForgeWindow.Error3"]);
            return;
        }

        foreach (var item in data.data)
        {
            List1.Add(new()
            {
                Name = item.displayName,
                Size = $"{item.fileLength / 1000 / 1000:0.00}",
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

    private async void AddCurseForgeWindow_Opened(object? sender, EventArgs e)
    {
        DataGridFiles.MakeTran();
        //return;
        Info1.Show(Localizer.Instance["AddCurseForgeWindow.Info4"]);
        var list = await GameBinding.GetCurseForgeGameVersions();
        Info1.Close();
        if (list == null)
        {
            Info.Show(Localizer.Instance["AddCurseForgeWindow.Error4"]);
            return;
        }

        ComboBox2.Items = list;
        Load();
    }

    public void Update()
    {
        App.Update(this, Image_Back, Rectangle1);
    }
}
