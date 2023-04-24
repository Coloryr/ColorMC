using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class GamesControl : UserControl
{
    private readonly Dictionary<string, GameControl> Items = new();

    private MainControl Window;
    private List<GameSettingObj> List;
    private bool Init;
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
        Window.GameItemSelect(null);
    }

    private void GamesControl_LayoutUpdated(object? sender, EventArgs e)
    {
        if (Init)
            return;
        Expander_Head.MakeTran();
        Init = true;
    }

    public void SetWindow(MainControl window)
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
            game.PointerReleased += Game_PointerReleased;
            game.PointerMoved += Game_PointerMoved;
            game.DoubleTapped += Game_DoubleTapped;
            game.SetItem(item);
            Items.Add(item.UUID, game);
            WrapPanel_Items.Children.Add(game);
        }
    }

    private Point point;

    private async void Game_PointerMoved(object? sender, PointerEventArgs e)
    {
        var game = (sender as GameControl)!;
        var pro = e.GetCurrentPoint(this);
        if (pro.Properties.IsLeftButtonPressed)
        {
            var pos = pro.Position;
            if (Math.Sqrt(Math.Pow(Math.Abs(pos.X - point.X), 2) + Math.Pow(Math.Abs(pos.Y - point.Y), 2)) > 30)
            {
                if (sender is UserControl con)
                {
                    con.RenderTransform = null;
                }

                List<IStorageFolder> files = new();
                if (Window.Window is Window win)
                {
                    var item = await win.StorageProvider
                        .TryGetFolderFromPathAsync(game.Obj.GetBasePath());
                    files.Add(item);
                }
                var dragData = new DataObject();
                dragData.Set(BaseBinding.DrapType, game);
                dragData.Set(DataFormats.Files, files);

                Dispatcher.UIThread.Post(() =>
                {
                    DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move | DragDropEffects.Link | DragDropEffects.Copy);
                });

                e.Handled = true;
            }
        }
    }

    private void Game_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is UserControl con)
        {
            con.RenderTransform = null;
        }
    }

    private void Game_DoubleTapped(object? sender, TappedEventArgs e)
    {
        e.Handled = true;
        if (Window.Obj != null)
        {
            Window.Launch(false);
        }
    }

    private void GameControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;

        if (Window.Obj != sender)
        {
            var game = (sender as GameControl)!;
            Window.GameItemSelect(game);
        }

        if (sender is UserControl con)
        {
            con.RenderTransform = new ScaleTransform(0.95, 0.95);
        }

        var pro = e.GetCurrentPoint(this);
        point = pro.Position;

        if (pro.Properties.IsRightButtonPressed
            && Window.Obj?.Obj != null)
        {
            _ = new MainFlyout(Window, Window.Obj!);
        }
    }

    public GameControl? Find(string? uuid)
    {
        if (string.IsNullOrWhiteSpace(uuid))
            return null;
        if (Items.TryGetValue(uuid, out var item))
        {
            return item;
        }

        return null;
    }
}
