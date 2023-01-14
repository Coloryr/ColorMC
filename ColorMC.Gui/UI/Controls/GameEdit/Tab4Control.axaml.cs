using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using DynamicData;
using System.Linq;
using ColorMC.Gui.Language;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab4Control : UserControl
{
    private readonly ObservableCollection<ModDisplayObj> List = new();
    private readonly List<ModDisplayObj> Items = new();
    private readonly Dictionary<string, ModObj> Dir1 = new();

    private readonly List<string> FName = new() 
    { 
        Localizer.Instance["Tab4Control2.Filter.Item1"],
        Localizer.Instance["Tab4Control2.Filter.Item2"],
        Localizer.Instance["Tab4Control2.Filter.Item3"]
    };

    private GameEditWindow Window;
    private GameSettingObj Obj;

    public Tab4Control()
    {
        InitializeComponent();

        DataGrid1.Items = List;

        Button_A1.PointerLeave += Button_A1_PointerLeave;
        Button_A.PointerEnter += Button_A_PointerEnter;

        Button_R1.PointerLeave += Button_R1_PointerLeave;
        Button_R.PointerEnter += Button_R_PointerEnter;

        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;
        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;

        Button_A1.Click += Button_A1_Click;
        Button_R1.Click += Button_R1_Click;

        ComboBox1.Items = FName;
        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox1.SelectedIndex = 0;

        LayoutUpdated += Tab5Control_LayoutUpdated;
    }

    private void TextBox1_TextInput(object? sender, TextInputEventArgs e)
    {
        Load1();
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

            if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                new GameEditFlyout1(this, mod).ShowAt(this, true);
            }
        });
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private async void Button_A1_Click(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog openFile = new()
        {
            Title = "Mod",
            AllowMultiple = false,
            Filters = new()
            {
                new FileDialogFilter()
                {
                    Extensions = new()
                    {
                        "jar"
                    }
                }
            }
        };

        var file = await openFile.ShowAsync(Window);
        if (file?.Length > 0)
        {
            GameBinding.AddMods(Obj, file);
            Window.Info2.Show("添加完成");
            Load();
        }
    }

    private void DataGrid1_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        var item = DataGrid1.SelectedItem as ModDisplayObj;
        if (item == null)
            return;

        DisE(item);
    }

    public void Delete(ModDisplayObj item)
    {
        if (Dir1.Remove(item.Local, out var obj))
        {
            GameBinding.DeleteMod(obj);
            List.Remove(item);

            Window.Info2.Show("删除完成");
        }
    }

    public void DisE(ModDisplayObj item)
    {
        if (Dir1.TryGetValue(item.Local, out var obj))
        {
            GameBinding.ModEnDi(obj);
            item.Enable = obj.Disable;
        }
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
        Expander_A.MakePadingNull();
        Expander_R.MakePadingNull();
    }

    private async void Load()
    {
        Window.Info1.Show("正在加载Mod列表");
        Dir1.Clear();
        Items.Clear();
        var res = await GameBinding.GetGameMods(Obj);
        Window.Info1.Close();
        if (res == null)
        {
            Window.Info.Show("Mod列表加载失败");
            return;
        }

        foreach (var item in res)
        {
            var obj = new ModDisplayObj()
            {
                Name = item.name,
                Version = item.version,
                Local = item.Local,
                Author = item.authorList.Make(),
                Url = item.url,
                Enable = item.Disable
            };

            Items.Add(obj);
            Dir1.Add(obj.Local, item);
        }

        Load1();
    }

    private void Load1() 
    {
        string fil = TextBox1.Text;
        if (string.IsNullOrWhiteSpace(fil))
        {
            List.Clear();
            List.AddRange(Items);
        }
        else
        {
            switch (ComboBox1.SelectedIndex)
            {
                case 0:
                    var list = from item in Items
                               where item.Name.Contains(fil)
                               select item;
                    List.Clear();
                    List.AddRange(list);
                    break;
                case 1:
                    list = from item in Items
                               where item.Local.Contains(fil)
                               select item;
                    List.Clear();
                    List.AddRange(list);
                    break;
                case 2:
                    list = from item in Items
                           where item.Author.Contains(fil)
                           select item;
                    List.Clear();
                    List.AddRange(list);
                    break;
            }
        }
    }

    public void SetWindow(GameEditWindow window)
    {
        Window = window;
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
