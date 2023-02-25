using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab5Control : UserControl
{
    private readonly List<WorldControl> List = new();
    private GameSettingObj Obj;
    private WorldControl? Last;

    public Tab5Control()
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

        Button1.Click += Button1_Click;

        LayoutUpdated += Tab5Control_LayoutUpdated;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpPath(Obj.GetSavesPath());
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Button_A1_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowAdd(Obj, FileType.World);
    }

    private async void Button_I1_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as GameEditWindow)!;
        var file = await BaseBinding.OpFile(window!,
            App.GetLanguage("GameEditWindow.Tab5.Info2"),
            new string[] { "*.zip" },
            App.GetLanguage("GameEditWindow.Tab5.Info8"));
        if (file.Any())
        {
            var res = await GameBinding.AddWorld(Obj, file[0].GetPath());
            if (!res)
            {
                window!.Info2.Show(App.GetLanguage("GameEditWindow.Tab4.Info2"));
                return;
            }
            window!.Info2.Show(App.GetLanguage("GameEditWindow.Tab4.Info2"));
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

    public async void Export(WorldDisplayObj obj)
    {
        var Window = (VisualRoot as GameEditWindow)!;
        var file = await BaseBinding.OpSave(Window,
            App.GetLanguage("GameEditWindow.Tab5.Info2"), ".zip", "world.zip");
        if (file == null)
            return;

        Window.Info1.Show(App.GetLanguage("GameEditWindow.Tab5.Info4"));
        bool error = false;
        try
        {
            await GameBinding.ExportWorld(obj.World, file.GetPath());
        }
        catch (Exception e)
        {
            Logs.Error(App.GetLanguage("GameEditWindow.Tab5.Error1"), e);
            error = true;
        }
        Window.Info1.Close();
        if (error)
        {
            Window.Info.Show(App.GetLanguage("GameEditWindow.Tab5.Error1"));
        }
        else
        {
            Window.Info2.Show(App.GetLanguage("GameEditWindow.Tab5.Info3"));
        }
    }

    public async void Delete(WorldDisplayObj obj)
    {
        var Window = (VisualRoot as GameEditWindow)!;
        var res = await Window!.Info.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab5.Info1"), obj.Name));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteWorld(obj.World);
        Window.Info2.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        Load();
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }

    public void SetSelect(WorldControl item)
    {
        Last?.SetSelect(false);
        Last = item;
        Last.SetSelect(true);
    }

    private async void Load()
    {
        var Window = (VisualRoot as GameEditWindow)!;
        Window.Info1.Show(App.GetLanguage("GameEditWindow.Tab5.Info7"));
        List.Clear();
        ListBox_Items.Children.Clear();

        var res = await GameBinding.GetWorlds(Obj!);
        Window.Info1.Close();
        foreach (var item in res)
        {
            var con = new WorldControl();
            con.Load(item);
            ListBox_Items.Children.Add(con);
            List.Add(con);
        }
    }

    private void Tab5Control_LayoutUpdated(object? sender, EventArgs e)
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
