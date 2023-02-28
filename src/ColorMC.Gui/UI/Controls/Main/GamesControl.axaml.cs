using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
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
    private bool check = false;
    public string Group { get; private set; }

    public GamesControl()
    {
        InitializeComponent();

        LayoutUpdated += GamesControl_LayoutUpdated;
        Expander_Head.PointerPressed += Expander_Head_PointerPressed;
        Expander_Head.ContentTransition = App.CrossFade300;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Source is Control)
        {
            if (e.Data.Get(BaseBinding.DrapType) is not GameControl c)
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
        if (e.Data.Get(BaseBinding.DrapType) is not GameControl c)
            return;
        if (Items.ContainsValue(c))
            return;

        GameBinding.MoveGameGroup(c.Obj, Group);
    }

    private void Expander_Head_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (check)
        {
            check = false;
            return;
        }

        Last?.SetSelect(false);
        Last = null;
        Window.GameItemSelect(null);
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

    public void Reload()
    {
        WrapPanel_Items.Children.Clear();
        Items.Clear();
        foreach (var item in List)
        {
            var game = new GameControl();
            game.PointerPressed += GameControl_PointerPressed;
            game.PointerMoved += GameControl_PointerMoved;
            game.DoubleTapped += Game_DoubleTapped;
            game.SetItem(item);
            Items.Add(item.UUID, game);
            WrapPanel_Items.Children.Add(game);
        }
    }

    private void Game_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (Last != null)
        {
            Window.Launch(false);
        }
    }

    private void GameControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        var game = (sender as GameControl)!;
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var dragData = new DataObject();
            dragData.Set(BaseBinding.DrapType, this);
            dragData.Set(DataFormats.FileNames, new string[] { game.Obj.GetBasePath() });

            Dispatcher.UIThread.Post(() =>
            {
                DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move | DragDropEffects.Link | DragDropEffects.Copy);
            });
        }
    }

    private void GameControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        check = true;
        if (Last != sender)
        {
            var game = (sender as GameControl)!;
            Last?.SetSelect(false);
            Last = game;
            game.SetSelect(true);
            Window.GameItemSelect(game);
        }

        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            if (Last?.Obj != null)
            {
                new MainFlyout(Window, Last!).ShowAt(this, true);
            }
        }
    }
}
