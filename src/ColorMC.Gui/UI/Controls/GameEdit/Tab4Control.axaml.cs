using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ColorMC.Core.Utils;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab4Control : UserControl
{
    private readonly ObservableCollection<ModDisplayObj> List = new();
    private readonly List<ModDisplayObj> Items = new();
    private readonly Dictionary<string, ModObj> Dir1 = new();
    private AddModWindow? AddModWindow;

    private readonly List<string> FName = new()
    {
        Localizer.Instance["GameEditWindow.Tab4.Filter.Item1"],
        Localizer.Instance["GameEditWindow.Tab4.Filter.Item2"],
        Localizer.Instance["GameEditWindow.Tab4.Filter.Item3"]
    };

    private GameEditWindow Window;
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

        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;
        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;

        Button_A1.Click += Button_A1_Click;
        Button_R1.Click += Button_R1_Click;
        Button_I1.Click += Button_I1_Click;

        Button1.Click += Button1_Click;

        ComboBox1.Items = FName;
        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox1.SelectedIndex = 0;

        TextBox1.PropertyChanged += TextBox1_TextInput;

        LayoutUpdated += Tab5Control_LayoutUpdated;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Load1();
    }

    private void TextBox1_TextInput(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        var property = e.Property.Name;
        if (property == "Text")
        {
            Load1();
        }
    }

    private async void Button_I1_Click(object? sender, RoutedEventArgs e)
    {
        var file = await Window.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = "Mod",
            AllowMultiple = true,
            FileTypeFilter = new List<FilePickerFileType>()
            {
                new FilePickerFileType("Mod")
                {
                    Patterns = new List<string>()
                    {
                        "*.jar"
                    }
                }
            }
        });
        if (file?.Any() == true)
        {
            var list = new List<string>();
            foreach (var item in file)
            {
                list.Add(item.Path.OriginalString);
            }
            GameBinding.AddMods(Obj, list);
            Window.Info2.Show(Localizer.Instance["GameEditWindow.Tab4.Info2"]);
            Load();
        }
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

    private void Button_A1_Click(object? sender, RoutedEventArgs e)
    {
        if (AddModWindow == null)
        {
            AddModWindow = new();
            AddModWindow.SetTab4Control(Obj, this);
            AddModWindow.Show();
        }
        else
        {
            AddModWindow.Activate();
        }
    }

    public void CloseAddMod()
    {
        AddModWindow = null;
    }

    private void DataGrid1_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        var item = DataGrid1.SelectedItem as ModDisplayObj;
        if (item == null)
            return;

        DisE(item);
    }

    public async void Delete(ModDisplayObj item)
    {
        var res = await Window.Info.ShowWait(
            string.Format(Localizer.Instance["GameEditWindow.Tab4.Info4"], item.Name));
        if (!res)
        {
            return;
        }

        if (Dir1.Remove(item.Local, out var obj))
        {
            GameBinding.DeleteMod(obj);
            List.Remove(item);

            Window.Info2.Show(Localizer.Instance["GameEditWindow.Tab4.Info3"]);
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
    }

    private async void Load()
    {
        Window.Info1.Show(Localizer.Instance["GameEditWindow.Tab4.Info1"]);
        Dir1.Clear();
        Items.Clear();
        var res = await GameBinding.GetGameMods(Obj);
        Window.Info1.Close();
        if (res == null)
        {
            Window.Info.Show(Localizer.Instance["GameEditWindow.Tab4.Error1"]);
            return;
        }

        int count = 0;

        foreach (var item in res)
        {
            ModDisplayObj obj;
            if (item.Broken)
            {
                obj = new ModDisplayObj()
                {
                    Name = Localizer.Instance["GameEditWindow.Tab4.Text3"],
                    Local = item.Local,
                    Enable = item.Disable
                };
                count++;
            }
            else
            {
                obj = new ModDisplayObj()
                {
                    Name = item.name,
                    Version = item.version,
                    Local = item.Local,
                    Author = item.authorList.Make(),
                    Url = item.url,
                    Loader = item.Loader.GetName(),
                    Enable = item.Disable
                };
            }

            Items.Add(obj);
            Dir1.Add(obj.Local, item);
        }

        if (count != 0)
        {
            Window.Info.Show(string.Format(Localizer
                .Instance["GameEditWindow.Tab4.Text4"], count));
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
