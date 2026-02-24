using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Config;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
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
    /// Java列表
    /// </summary>
    public ObservableCollection<string> JavaList { get; init; } = [];

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
    private bool _enableArg;

    [ObservableProperty]
    private string _java;

    partial void OnGameChanged(GameItemModel? value)
    {
        if (value == null)
        {
            GameName = LangUtils.Get("MainWindow.Text79");
            GameIcon = ImageManager.GameIcon;
            HaveGame = false;
            MinMem = MaxMem = GameWidth = GameHeight = null;
            return;
        }

        LoadJavaList();

        GameName = value.Name;
        GameIcon = ImageManager.GetGameIcon(value.Obj) ?? ImageManager.GameIcon;

        var conf = ConfigLoad.Config;
        MaxMem = value.Obj.JvmArg?.MaxMemory ?? conf.DefaultJvmArg.MaxMemory;
        MinMem = value.Obj.JvmArg?.MinMemory ?? conf.DefaultJvmArg.MinMemory;
        MaxWindow = value.Obj.Window?.FullScreen ?? conf.Window.FullScreen ?? false;
        GameWidth = value.Obj.Window?.Width ?? conf.Window.Width;
        GameHeight = value.Obj.Window?.Height ?? conf.Window.Height;
        Java = value.Obj.JvmLocal ?? "";
    }

    partial void OnJavaChanged(string value)
    {
        if (Game?.Obj is not { } obj)
        {
            return;
        }

        obj.JvmLocal = value;
        obj.Save();
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

    [RelayCommand]
    public void OpenGameSetting()
    {
        if (Game == null)
        {
            return;
        }

        WindowManager.ShowGameEdit(Game.Obj, GameEditWindowType.Arg);
    }

    /// <summary>
    /// 启动选中的游戏实例
    /// </summary>
    [RelayCommand]
    public void LaunchSimple()
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

    private void LoadJavaList()
    {
        JavaList.Clear();

        JavaList.Add("");

        foreach (var item in JvmPath.Jvms)
        {
            JavaList.Add(item.Value.Path);
        }

        Java = Game?.Obj.JvmLocal ?? "";
    }

    private void ColorMCCore_JavaChange(JavaChangeArg obj)
    {
        Dispatcher.UIThread.Post(LoadJavaList);
    }
}
