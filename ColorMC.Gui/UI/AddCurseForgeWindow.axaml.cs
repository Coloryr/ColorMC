using Avalonia.Controls;
using ColorMC.Gui.UIBinding;
using System;
using Avalonia.Interactivity;
using ColorMC.Gui.UI.Views.CurseForge;
using System.Collections.Generic;
using ColorMC.Core.Objs.CurseForge;

namespace ColorMC.Gui.UI;

public partial class AddCurseForgeWindow : Window
{
    private List<CurseForgeControl> List = new();
    private CurseForgeControl? Last;
    public AddCurseForgeWindow()
    {
        InitializeComponent();

        this.MakeItNoChrome();
        FontFamily = Program.FontFamily;

        if (App.BackBitmap != null)
        {
            Image_Back.Source = App.BackBitmap;
        }

        ComboBox1.Items = GameBinding.GetCurseForgeTypes();
        ComboBox3.Items = GameBinding.GetSortOrder();

        ComboBox1.SelectedIndex = 1;
        ComboBox3.SelectedIndex = 1;

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;

        Opened += AddCurseForgeWindow_Opened;
        Closed += AddCurseForgeWindow_Closed;
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        if (Last == null)
        {
            Info.Show("你还没有选择整合包");
            return;
        }

        Install(Last.Data);
        Close();
    }

    private void AddCurseForgeWindow_Closed(object? sender, EventArgs e)
    {
        App.AddCurseForgeWindow = null;
    }

    public async void Install(CurseForgeObj.Data data)
    {
        var res = await Info.ShowWait("是否安装整合包：" + data.name);
        if (res)
        {
            App.ShowAddGame();
            App.AddGameWindow.Install(data);
            Close();
        }
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
        Info1.Show("正在搜索中");
        var data = await GameBinding.GetPackList(ComboBox2.SelectedItem as string, ComboBox1.SelectedIndex + 1, Input1.Text, int.Parse(Input2.Text), ComboBox3.SelectedIndex);
        Info1.Close();
        if (data == null)
        {
            Info.Show("整合包数据加载失败");
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

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private async void AddCurseForgeWindow_Opened(object? sender, EventArgs e)
    {
        Info1.Show("正在加载中");
        var list = await GameBinding.GetCurseForgeGameVersions();
        Info1.Close();
        if (list == null)
        {
            Info.Show("游戏版本加载失败");
            return;
        }

        ComboBox2.Items = list;
        Load();
    }
}
