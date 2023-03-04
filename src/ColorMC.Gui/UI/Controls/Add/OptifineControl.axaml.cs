using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.LaunchPath;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class OptifineControl : UserControl
{
    private GameSettingObj Obj;
    public readonly ObservableCollection<OptifineDisplayObj> List = new();
    public OptifineControl()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;

        DataGrid1.Items = List;
    }

    public void SetGame(GameSettingObj obj) 
    {
        Obj = obj;
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as AddWindow)!;

        window.OptifineClsoe();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private async void Button1_Click(object? sender, RoutedEventArgs e)
    {
        if (DataGrid1.SelectedItem is not OptifineDisplayObj item)
            return;

        var window = (VisualRoot as AddWindow)!;

        var res = await window.Info.ShowWait(string.Format("是否要安装 {0}", item.Version));
        if (!res)
            return;
        window.Info1.Show("正在下载");
        var res1 = await GameBinding.DownloadOptifine(Obj, item);
        window.Info1.Close();
        if (res1.Item1 == false)
        {
            window.Info.Show(res1.Item2!);
        }
        else 
        {
            window.Info2.Show("已下载");
            window.OptifineClsoe();
        }
    }

    public async void Load()
    {
        var window = (VisualRoot as AddWindow)!;

        window.Info1.Show("正在加载Optifine列表");

        var list = await GameBinding.GetOptifine();
        if (list == null)
        {
            window.Info1.Close();
            window.Info.Show("Optifine版本加载失败");
            return;
        }

        List.AddRange(list);
    }
}
