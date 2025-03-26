using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using AvaloniaEdit.Utils;
using ColorMC.Core.Config;
using ColorMC.Core.LaunchPath;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Main;

/// <summary>
/// 简易模式
/// </summary>
public partial class MainModel
{
    /// <summary>
    /// 游戏列表
    /// </summary>
    public ObservableCollection<GameItemModel> GameList { get; init; } = [];

    [ObservableProperty]
    private bool _maxWindow;

    [ObservableProperty]
    private uint? _gameWidth;
    [ObservableProperty]
    private uint? _gameHeight;
    [ObservableProperty]
    private uint? _minMem;
    [ObservableProperty]
    private uint? _maxMem;

    [ObservableProperty]
    private string _gameName;

    [ObservableProperty]
    private Bitmap _gameIcon;

    [ObservableProperty]
    private bool _haveGame;

    [ObservableProperty]
    private bool _simpleMode;

    private bool _isload;

    partial void OnSimpleModeChanged(bool value)
    {
        if (_isload)
        {
            return;
        }

        var config = GuiConfigUtils.Config;
        config.Simple = value;
        GuiConfigUtils.Save();

        LoadGameItem();
    }

    partial void OnMaxWindowChanged(bool value)
    {
        if (Game?.Obj is not { } obj)
        {
            return;
        }

        obj.Window ??= new();
        obj.Window.FullScreen = value;
        obj.Save();
    }

    partial void OnGameWidthChanged(uint? value)
    {
        if (Game?.Obj is not { } obj)
        {
            return;
        }

        obj.Window ??= new();
        obj.Window.Width = value;
        obj.Save();
    }

    partial void OnGameHeightChanged(uint? value)
    {
        if (Game?.Obj is not { } obj)
        {
            return;
        }

        obj.Window ??= new();
        obj.Window.Height = value;
        obj.Save();
    }

    partial void OnMinMemChanged(uint? value)
    {
        if (Game?.Obj is not { } obj)
        {
            return;
        }

        obj.JvmArg ??= new();
        obj.JvmArg.MinMemory = value;
        obj.Save();
    }
    partial void OnMaxMemChanged(uint? value)
    {
        if (Game?.Obj is not { } obj)
        {
            return;
        }

        obj.JvmArg ??= new();
        obj.JvmArg.MaxMemory = value;
        obj.Save();
    }

    /// <summary>
    /// 启动选中的游戏实例
    /// </summary>
    [RelayCommand]
    public void Launch()
    {
        if (Game == null)
        {
            return;
        }

        Game.Launch();
    }
    /// <summary>
    /// 编辑游戏实例
    /// </summary>
    [RelayCommand]
    public void EditGame()
    {
        if (Game == null)
        {
            return;
        }

        WindowManager.ShowGameEdit(Game.Obj);
    }

    /// <summary>
    /// 加载游戏实例
    /// </summary>
    private void LoadSimple()
    {
        GameName = Game?.Name ?? App.Lang("MainWindow.Info44");
        GameIcon = Game?.Pic ?? ImageManager.GameIcon;

        if (Game == null)
        {
            HaveGame = false;
            MinMem = MaxMem = GameWidth = GameHeight = null;
            return;
        }

        var conf = ConfigUtils.Config;
        MaxMem = Game.Obj.JvmArg?.MaxMemory ?? conf.DefaultJvmArg.MaxMemory;
        MinMem = Game.Obj.JvmArg?.MinMemory ?? conf.DefaultJvmArg.MinMemory;
        MaxWindow = Game.Obj.Window?.FullScreen ?? conf.Window.FullScreen ?? false;
        GameWidth = Game.Obj.Window?.Width ?? conf.Window.Width;
        GameHeight = Game.Obj.Window?.Height ?? conf.Window.Height;
    }
    /// <summary>
    /// 加载游戏实例
    /// </summary>
    public void LoadSimpleGames()
    {
        var temp = Game;
        GameList.Clear();
        if (IsOneGame)
        {
            GameList.Add(OneGame!);
            Game = OneGame;
        }
        else
        {
            foreach (var item in GameGroups)
            {
                GameList.AddRange(item.Items.Values);
            }
        }
        if (temp != null && GameList.Contains(temp))
        {
            Game = temp;
        }
    }
}
