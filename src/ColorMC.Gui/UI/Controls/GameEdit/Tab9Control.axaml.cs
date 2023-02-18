using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab9Control : UserControl
{
    private readonly List<ScreenshotControl> List = new();
    private GameSettingObj Obj;
    private ScreenshotControl? Last;

    public Tab9Control()
    {
        InitializeComponent();

        Button_C1.PointerExited += Button_C1_PointerLeave;
        Button_C.PointerEntered += Button_C_PointerEnter;

        Button_R1.PointerExited += Button_R1_PointerLeave;
        Button_R.PointerEntered += Button_R_PointerEnter;

        Button_R1.Click += Button_R1_Click;
        Button_C1.Click += Button_C1_Click;

        LayoutUpdated += Tab9Control_LayoutUpdated;
    }

    private async void Button_C1_Click(object? sender, RoutedEventArgs e)
    {
        var Window = (VisualRoot as GameEditWindow)!;
        var res = await Window.Info.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab9.Info2"), Obj.Name));
        if (!res)
        {
            return;
        }

        GameBinding.ClearScreenshots(Obj);
        Window.Info2.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        Load();
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Button_C1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_C.IsExpanded = false;
    }

    private void Button_C_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_C.IsExpanded = true;
    }

    private void Button_R1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_R.IsExpanded = false;
    }

    private void Button_R_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_R.IsExpanded = true;
    }

    private void Tab9Control_LayoutUpdated(object? sender, EventArgs e)
    {
        Expander_R.MakePadingNull();
        Expander_C.MakePadingNull();
    }

    public async void Delete(ScreenshotDisplayObj obj)
    {
        var Window = (VisualRoot as GameEditWindow)!;
        var res = await Window.Info.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab9.Info1"), obj.Local));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteScreenshot(obj.Local);
        Window.Info2.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        Load();
    }

    private async void Load()
    {
        var window = (VisualRoot as GameEditWindow)!;
        window.Info1.Show(App.GetLanguage("GameEditWindow.Tab9.Info3"));
        List.Clear();
        WrapPanel1.Children.Clear();

        var res = await GameBinding.GetScreenshots(Obj);
        window.Info1.Close();
        foreach (var item in res)
        {
            var con = new ScreenshotControl();
            con.SetWindow(this);
            con.Load(item);
            WrapPanel1.Children.Add(con);
            List.Add(con);
        }
    }

    public void SetSelect(ScreenshotControl item)
    {
        Last?.SetSelect(false);
        Last = item;
        Last.SetSelect(true);
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
