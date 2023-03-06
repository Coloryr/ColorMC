using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class OptifineControl : UserControl
{
    private GameSettingObj Obj;
    public readonly List<OptifineDisplayObj> List2 = new();
    public readonly ObservableCollection<OptifineDisplayObj> List = new();
    public readonly ObservableCollection<string> List1 = new();
    public OptifineControl()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;

        ComboBox6.SelectionChanged += ComboBox6_SelectionChanged;

        DataGrid1.Items = List;
        ComboBox6.Items = List1;
        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;
    }

    private void ComboBox6_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Load1();
    }

    private void DataGrid1_DoubleTapped(object? sender, TappedEventArgs e)
    {
        Button3_Click(null, null);
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }

    private async void Button3_Click(object? sender, RoutedEventArgs e)
    {
        if (DataGrid1.SelectedItem is not OptifineDisplayObj item)
            return;

        var window = (VisualRoot as AddWindow)!;

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
            window.OptifineClsoe();
        }
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as AddWindow)!;

        window.OptifineClsoe();
    }

    public async void Load()
    {
        var window = (VisualRoot as AddWindow)!;
        List.Clear();
        List1.Clear();
        List2.Clear();
        window.Info1.Show(App.GetLanguage("AddGameWindow.Control1.Info4"));
        var list = await GameBinding.GetOptifine();
        window.Info1.Close();
        if (list == null)
        {
            window.Info.Show(App.GetLanguage("AddGameWindow.Control1.Error1"));
            return;
        }

        List2.AddRange(list);

        List1.Add("");
        List1.AddRange(from item in list
                       group item by item.MC into newgroup
                       select newgroup.Key);

        Load1();
    }

    public void Load1()
    {
        List.Clear();
        if (ComboBox6.SelectedItem is not string item
            || string.IsNullOrWhiteSpace(item))
        {
            List.AddRange(List2);
        }
        else
        {
            List.AddRange(from item1 in List2
                          where item1.MC == item
                          select item1);
        }
    }
}
