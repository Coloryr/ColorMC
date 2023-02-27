using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class GamesControl : UserControl
{
    private readonly Dictionary<string, GameControl> Items = new();

    private MainWindow Window;
    private List<GameSettingObj> List;
    private GameControl? Last;
    private bool Init;
    private bool Check;
    public string Group { get; private set; }

    public GamesControl()
    {
        InitializeComponent();

        LayoutUpdated += GamesControl_LayoutUpdated;
        Expander_Head.PointerPressed += WrapPanel_Items_PointerPressed;
        WrapPanel_Items.DoubleTapped += WrapPanel_Items_DoubleTapped;
        Expander_Head.ContentTransition = App.CrossFade300;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Source is Control)
        {
            if (e.Data.Get(App.DrapType) is not GameControl c)
                return;
            if (Items.ContainsValue(c))
                return;

            Grid1.IsVisible = true;
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Grid1.IsVisible = false;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        Grid1.IsVisible = false;
        if (e.Data.Get(App.DrapType) is not GameControl c)
            return;
        if (Items.ContainsValue(c))
            return;

        GameBinding.MoveGameGroup(c.Obj, Group);
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

    public void SetName(string name, string display)
    {
        Group = name;
        Expander_Head.Header = display;
    }

    public void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var item in WrapPanel_Items.Children)
            {
                if (item is not GameControl c)
                    return;
                c.Close();
            }
        });
    }

    public void Reload()
    {
        Close();
        WrapPanel_Items.Children.Clear();
        Items.Clear();
        foreach (var item in List)
        {
            var game = new GameControl();
            game.PointerPressed += Game_PointerPressed;
            game.SetItem(item);
            Items.Add(item.UUID, game);
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
        Window.GameItemSelect(Last);

        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            if (Last?.Obj != null)
            {
                new MainFlyout(Window, game!).ShowAt(this, true);
            }
        }
    }
}
