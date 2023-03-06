using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab4Control : UserControl
{
    private readonly ObservableCollection<ModDisplayObj> List = new();
    private readonly List<ModDisplayObj> Items = new();
    private bool isSet;

    private GameSettingObj Obj;

    public Tab4Control()
    {
        InitializeComponent();

        DataGrid1.Items = List;

        Button_A1.PointerExited += Button_A1_PointerLeave;
        Button_A.PointerEntered += Button_A_PointerEnter;

        Button_I1.PointerExited += Button_I1_PointerLeave;
        Button_I.PointerEntered += Button_I_PointerEnter;

        Button_R1.PointerExited += Button_R1_PointerLeave;
        Button_R.PointerEntered += Button_R_PointerEnter;

        Button_C1.PointerExited += Button_C1_PointerLeave;
        Button_C.PointerEntered += Button_C_PointerEnter;

        Button_B1.PointerExited += Button_B1_PointerLeave;
        Button_B.PointerEntered += Button_B_PointerEnter;

        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;
        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;

        Button_A1.Click += Button_A1_Click;
        Button_R1.Click += Button_R1_Click;
        Button_I1.Click += Button_I1_Click;
        Button_C1.Click += Button_C1_Click;
        Button_B1.Click += Button_B1_Click;

        Button1.Click += Button1_Click;

        ComboBox1.Items = BaseBinding.GetFilterName();
        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox1.SelectedIndex = 0;

        TextBox1.PropertyChanged += TextBox1_TextInput;

        LayoutUpdated += Tab5Control_LayoutUpdated;
    }

    private async void Button_B1_Click(object? sender, RoutedEventArgs e)
    {
        if (isSet) 
            return;

        isSet = true;
        await App.ShowAddSet(Obj);
        isSet = false;
    }

    private async void Button_C1_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as GameEditWindow)!;
        window.Info1.Show("正在检测Mod版本更新");
        var res = await GameBinding.CheckModUpdate(Obj, Items);
        window.Info1.Close();
        if (res.Count > 0)
        {
            var res1 = await window.Info.ShowWait(string.Format(
                "检测到 {0} 个Mod有新版本，是否要更新", res.Count));
            if (res1)
            {
                window.Info1.Show("正在更新Mod");
                await GameBinding.StartUpdate(res);
                window.Info1.Close();

                Load();
            }
        }
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpPath(Obj.GetModsPath());
    }

    private void TextBox1_TextInput(object? sender,
        AvaloniaPropertyChangedEventArgs e)
    {
        var property = e.Property.Name;
        if (property == "Text")
        {
            Load1();
        }
    }

    private async void Button_I1_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as GameEditWindow)!;
        var file = await BaseBinding.AddFile(window, Obj, FileType.Mod);

        if (file == null)
            return;

        if (file == false)
        {
            window.Info1.Show(App.GetLanguage("GameEditWindow.Tab4.Error2"));
            return;
        }

        window.Info2.Show(App.GetLanguage("GameEditWindow.Tab4.Info2"));
        Load();
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Load1();
    }

    private void DataGrid1_CellPointerPressed(object? sender,
        DataGridCellPointerPressedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var mod = DataGrid1.SelectedItem as ModDisplayObj;
            if (mod == null)
                return;

            var items = DataGrid1.SelectedItems;

            if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                new GameEditFlyout1(this, items).ShowAt(this, true);
            }
        });
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Button_A1_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowAdd(Obj, FileType.Mod);
    }

    private void DataGrid1_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        var item = DataGrid1.SelectedItem as ModDisplayObj;
        if (item == null)
            return;

        DisE(item);
    }

    public async void Delete(IEnumerable<ModDisplayObj> items)
    {
        var window = (VisualRoot as GameEditWindow)!;
        var res = await window.Info.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab4.Info9"), items.Count()));
        if (!res)
        {
            return;
        }

        items.ToList().ForEach(item =>
        {
            GameBinding.DeleteMod(item.Obj);
            List.Remove(item);
        });

        window.Info2.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
    }

    public async void Delete(ModDisplayObj item)
    {
        var window = (VisualRoot as GameEditWindow)!;
        var res = await window.Info.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab4.Info4"), item.Name));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteMod(item.Obj);
        List.Remove(item);

        window.Info2.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
    }

    public void DisE(ModDisplayObj item)
    {
        GameBinding.ModEnDi(item.Obj);
        item.Enable = item.Obj.Disable;
    }

    private void Button_B1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_B.IsExpanded = false;
    }

    private void Button_B_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_B.IsExpanded = true;
    }

    private void Button_C1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_C.IsExpanded = false;
    }

    private void Button_C_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_C.IsExpanded = true;
    }

    private void Button_I1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_I.IsExpanded = false;
    }

    private void Button_I_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_I.IsExpanded = true;
    }

    private void Button_A1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_A.IsExpanded = false;
    }

    private void Button_A_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_A.IsExpanded = true;
    }
    private void Button_R1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_R.IsExpanded = false;
    }

    private void Button_R_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_R.IsExpanded = true;
    }

    private void Tab5Control_LayoutUpdated(object? sender, EventArgs e)
    {
        DataGrid1.MakeTran();
        Expander_I.MakePadingNull();
        Expander_A.MakePadingNull();
        Expander_R.MakePadingNull();
        Expander_C.MakePadingNull();
        Expander_B.MakePadingNull();
    }

    private async void Load()
    {
        var window = (VisualRoot as GameEditWindow)!;
        window.Info1.Show(App.GetLanguage("GameEditWindow.Tab4.Info1"));
        Items.Clear();
        var res = await GameBinding.GetGameMods(Obj);
        window.Info1.Close();
        if (res == null)
        {
            window.Info.Show(App.GetLanguage("GameEditWindow.Tab4.Error1"));
            return;
        }

        int count = 0;

        Items.AddRange(res);

        if (count != 0)
        {
            window.Info.Show(string.Format(App
                .GetLanguage("GameEditWindow.Tab4.Info6"), count));
        }

        Load1();
    }

    private void Load1()
    {
        if (string.IsNullOrWhiteSpace(TextBox1.Text))
        {
            List.Clear();
            List.AddRange(Items);
        }
        else
        {
            string fil = TextBox1.Text.ToLower();
            switch (ComboBox1.SelectedIndex)
            {
                case 0:
                    var list = from item in Items
                               where item.Name.ToLower().Contains(fil)
                               select item;
                    List.Clear();
                    List.AddRange(list);
                    break;
                case 1:
                    list = from item in Items
                           where item.Local.ToLower().Contains(fil)
                           select item;
                    List.Clear();
                    List.AddRange(list);
                    break;
                case 2:
                    list = from item in Items
                           where item.Author.ToLower().Contains(fil)
                           select item;
                    List.Clear();
                    List.AddRange(list);
                    break;
            }
        }
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }

    public void Update()
    {
        if (Obj == null)
            return;

        Load();
    }
}
