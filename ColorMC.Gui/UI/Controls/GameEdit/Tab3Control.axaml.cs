using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Language;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab3Control : UserControl
{
    private readonly ObservableCollection<ModDisplayObj> List = new();
    private readonly Dictionary<string, ModObj> Dir1 = new();
    private GameEditWindow Window;
    private GameSettingObj Obj;

    public Tab3Control()
    {
        InitializeComponent();

        DataGrid1.Items = List;

        Button_E1.PointerLeave += Button_E1_PointerLeave;
        Button_E.PointerEnter += Button_E_PointerEnter;

        Button_A1.PointerLeave += Button_A1_PointerLeave;
        Button_A.PointerEnter += Button_A_PointerEnter;

        Button_D1.PointerLeave += Button_D1_PointerLeave;
        Button_D.PointerEnter += Button_D_PointerEnter;

        Button_R1.PointerLeave += Button_R1_PointerLeave;
        Button_R.PointerEnter += Button_R_PointerEnter;

        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;

        Button_E1.Click += Button_E1_Click;

        LayoutUpdated += Tab5Control_LayoutUpdated;
    }

    private void Button_E1_Click(object? sender, RoutedEventArgs e)
    {
        DataGrid1_DoubleTapped(sender, e);
    }

    private void DataGrid1_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        var item = DataGrid1.SelectedItem as ModDisplayObj;
        if (item == null)
            return;

        if (Dir1.TryGetValue(item.Local, out var obj))
        {
            GameBinding.ModEnDi(obj);
            item.Enable = obj.Disable;
        }
    }

    private void Button_E1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_E.IsExpanded = false;
    }

    private void Button_E_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_E.IsExpanded = true;
    }

    private void Button_A1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_A.IsExpanded = false;
    }

    private void Button_A_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_A.IsExpanded = true;
    }

    private void Button_D1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_D.IsExpanded = false;
    }

    private void Button_D_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_D.IsExpanded = true;
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
        Expander_E.MakePadingNull();
        Expander_A.MakePadingNull();
        Expander_R.MakePadingNull();
        Expander_D.MakePadingNull();
    }

    private async void Load() 
    {
        Window.Info1.Show("正在加载Mod列表");
        Dir1.Clear();
        List.Clear();
        var res= await GameBinding.GetGameMods(Obj);
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

            List.Add(obj);
            Dir1.Add(obj.Local, item);
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
