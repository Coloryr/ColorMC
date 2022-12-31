using Avalonia.Controls;
using ColorMC.Core.Objs;
using System.Collections.Generic;
using Avalonia.Input;
using SixLabors.Fonts;

namespace ColorMC.Gui.UI.Views.Main;

public partial class GamesControl : UserControl
{
    private MainWindow Window;
    private List<GameSettingObj> List;
    private GameControl? Last;
    private Dictionary<string, GameControl> Items = new();

    public GamesControl()
    {
        InitializeComponent();
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
        var game = sender as GameControl;
        Last?.SetSelect(false);
        Last = game;
        Last?.SetSelect(true);
        Window.ItemSelect(Last?.Obj); 
        
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            if (Last?.Obj != null)
            {
                new MyFlyout(Last?.Obj!).ShowAt(this, true);
            }
        }
    }
}
