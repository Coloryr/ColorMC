using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class GamesControl : UserControl
{
    private readonly Dictionary<string, GameControl> Items = new();

    private static readonly CrossFade CrossFade = new(TimeSpan.FromMilliseconds(300));

    private MainWindow Window;
    private List<GameSettingObj> List;
    private GameControl? Last;
    private bool Init;
    private bool Check;

    public GamesControl()
    {
        InitializeComponent();

        LayoutUpdated += GamesControl_LayoutUpdated;
        Expander_Head.PointerPressed += WrapPanel_Items_PointerPressed;
        WrapPanel_Items.DoubleTapped += WrapPanel_Items_DoubleTapped;
        Expander_Head.ContentTransition = CrossFade;
    }

    private void WrapPanel_Items_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (Last != null)
        {
            Window.Launch(false);
        }
    }

    private void WrapPanel_Items_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Check == false)
        {
            Last?.SetSelect(false);
            Last = null;
            Window.GameItemSelect(null);
        }

        Check = false;
    }

    private void GamesControl_LayoutUpdated(object? sender, EventArgs e)
    {
        if (Init)
            return;
        Expander_Head.MakeTran();
        Init = true;
    }

    public void SetWindow(MainWindow window)
    {
        Window = window;
    }

    public void SetItems(List<GameSettingObj> list)
    {
        List = list;
        Reload();
    }

    public void SetName(string name)
    {
        Expander_Head.Header = name;
    }

    public void Reload()
    {
        WrapPanel_Items.Children.Clear();
        Items.Clear();
        foreach (var item in List)
        {
            var game = new GameControl();
            game.PointerPressed += Game_PointerPressed;
            game.SetItem(item);
            Items.Add(item.Name, game);
            WrapPanel_Items.Children.Add(game);
        }
    }

    private void Game_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Check = true;
        var game = sender as GameControl;
        Last?.SetSelect(false);
        Last = game;
        Last?.SetSelect(true);
        Window.GameItemSelect(Last?.Obj);

        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            if (Last?.Obj != null)
            {
                new MyFlyout(Last?.Obj!).ShowAt(this, true);
            }
        }
    }
}
