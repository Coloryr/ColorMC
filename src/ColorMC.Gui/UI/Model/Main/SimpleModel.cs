﻿using Avalonia.Media.Imaging;
using AvaloniaEdit.Utils;
using ColorMC.Core.Config;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Silk.NET.SDL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public bool IsSimple { get; private set; }

    [RelayCommand]
    public void Launch()
    {
        if (Game == null)
        {
            return;
        }

        Game.Launch();
    }

    [RelayCommand]
    public void EditGame()
    {
        if (Game == null)
        {
            return;
        }

        WindowManager.ShowGameEdit(Game.Obj);
    }

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

    public void LoadSimpleGames()
    {
        GameList.Clear();
        foreach (var item in GameGroups)
        {
            GameList.AddRange(item.Items.Values);
        }
    }
}
