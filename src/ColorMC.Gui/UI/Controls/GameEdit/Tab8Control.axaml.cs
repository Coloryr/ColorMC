using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab8Control : UserControl
{
    private readonly List<ResourcePackControl> List = new();
    private GameSettingObj Obj;
    private ResourcePackControl? Last;

    public Tab8Control()
    {
        InitializeComponent();

        Button_A1.PointerExited += Button_A1_PointerLeave;
        Button_A.PointerEntered += Button_A_PointerEnter;

        Button_I1.PointerExited += Button_I1_PointerLeave;
        Button_I.PointerEntered += Button_I_PointerEnter;

        Button_R1.PointerExited += Button_R1_PointerLeave;
        Button_R.PointerEntered += Button_R_PointerEnter;

        Button_R1.Click += Button_R1_Click;
        Button_A1.Click += Button_A1_Click;
        Button_I1.Click += Button_I1_Click;

        LayoutUpdated += Tab8Control_LayoutUpdated;
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Button_A1_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowAdd(Obj, FileType.Resourcepack);
    }

    private async void Button_I1_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as GameEditWindow)!;
        var file = await BaseBinding.OpFile(window,
            App.GetLanguage("GameEditWindow.Tab8.Info2"), 
            new string[] { "*.zip" },
            App.GetLanguage("GameEditWindow.Tab8.Info7"));
        if (file.Any())
        {
            var res = await GameBinding.AddResourcepack(Obj, file[0].GetPath());
            if (!res)
            {
                window.Info2.Show(App.GetLanguage("GameEditWindow.Tab4.Info2"));
                return;
            }
            window.Info2.Show(App.GetLanguage("GameEditWindow.Tab4.Info2"));
            Load();
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

    public async void Delete(ResourcepackDisplayObj obj)
    {
        var window = (VisualRoot as GameEditWindow)!;
        var res = await window.Info.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab8.Info1"), obj.Local));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteResourcepack(obj.Pack);
        window.Info2.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        Load();
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }

    public void SetSelect(ResourcePackControl item)
    {
        Last?.SetSelect(false);
        Last = item;
        Last.SetSelect(true);
    }

    private async void Load()
    {
        var window = (VisualRoot as GameEditWindow)!;
        window.Info1.Show(App.GetLanguage("GameEditWindow.Tab8.Info5"));
        List.Clear();
        ListBox_Items.Children.Clear();

        var res = await GameBinding.GetResourcepacks(Obj);
        window.Info1.Close();
        foreach (var item in res)
        {
            var con = new ResourcePackControl();
            con.SetWindow(this);
            con.Load(item);
            ListBox_Items.Children.Add(con);
            List.Add(con);
        }
    }

    private void Tab8Control_LayoutUpdated(object? sender, EventArgs e)
    {
        Expander_I.MakePadingNull();
        Expander_A.MakePadingNull();
        Expander_R.MakePadingNull();
    }

    public void Update()
    {
        if (Obj == null)
            return;

        Load();
    }
}
