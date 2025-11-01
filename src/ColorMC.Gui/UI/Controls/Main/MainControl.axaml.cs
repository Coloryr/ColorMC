using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.LaunchPath;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Main;

/// <summary>
/// 主窗口
/// </summary>
public partial class MainControl : BaseUserControl
{
    public const string DialogName = "MainCon";

    /// <summary>
    /// 单游戏实例
    /// </summary>
    private MainOneGameControl? _oneGame;
    /// <summary>
    /// 没有游戏实例
    /// </summary>
    private MainEmptyControl? _emptyGame;
    /// <summary>
    /// 游戏实例列表
    /// </summary>
    private MainGamesControl? _games;
    /// <summary>
    /// 简易主界面
    /// </summary>
    private SimpleControl? _simple;

    public MainControl() : base(WindowManager.GetUseName<MainControl>())
    {
        InitializeComponent();

        Title = "ColorMC";

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, DropAsync);
    }

    public override Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.F || e.KeyModifiers != KeyModifiers.Control
                           || DataContext is not MainModel model
                           || Content1.Child is not MainGamesControl con)
        {
            return Task.FromResult(false);
        }

        if (model.GameSearch)
        {
            model.SearchClose();
        }
        else
        {
            model.Search();
            con.Search.Focus();
        }

        return Task.FromResult(true);
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(BaseBinding.DrapType))
        {
            return;
        }
        if (e.DataTransfer.Contains(DataFormat.Text))
        {
            Grid2.IsVisible = true;
            Label1.Text = LanguageUtils.Get("UserWindow.Text8");
        }
        else if (e.DataTransfer.Contains(DataFormat.File))
        {
            var files = e.DataTransfer.TryGetFiles();
            if (files == null || files.Length > 1)
                return;

            var item = files.FirstOrDefault();
            switch (item)
            {
                case null:
                    return;
                case IStorageFolder forder when Directory.Exists(forder.GetPath()):
                    Grid2.IsVisible = true;
                    Label1.Text = LanguageUtils.Get("AddGameWindow.Text2");
                    break;
                default:
                    {
                        if (item.Name.EndsWith(Names.NameZipExt) || item.Name.EndsWith(Names.NameMrpackExt))
                        {
                            Grid2.IsVisible = true;
                            Label1.Text = LanguageUtils.Get("MainWindow.Text25");
                        }
                        else if (item.Name.EndsWith(GuiNames.NameColorMCExt))
                        {
                            Grid2.IsVisible = true;
                            Label1.Text = LanguageUtils.Get("MainWindow.Text38");
                        }

                        break;
                    }
            }
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
    }

    private async Task DropAsync(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(BaseBinding.DrapType))
        {
            return;
        }
        Grid2.IsVisible = false;
        if (e.DataTransfer.Contains(DataFormat.Text))
        {
            var str = e.DataTransfer.TryGetText();
            if (str == null)
            {
                return;
            }
            if (str.StartsWith(GuiNames.NameAuthlibKey))
            {
                WindowManager.ShowUser(url: str);
            }
            else if (str.StartsWith(GuiNames.NameColorMCCloudKey, StringComparison.CurrentCultureIgnoreCase))
            {
                BaseBinding.SetCloudKey(str);
            }
        }
        else if (e.DataTransfer.Contains(DataFormat.File))
        {
            var files = e.DataTransfer.TryGetFiles();
            if (files == null || files.Length > 1)
                return;

            var item = files.FirstOrDefault();
            switch (item)
            {
                case null:
                    return;
                case IStorageFolder forder when Directory.Exists(forder.GetPath()):
                    WindowManager.ShowAddGame(null, true, forder.GetPath());
                    break;
                default:
                    {
                        if (item.Name.EndsWith(Names.NameZipExt) || item.Name.EndsWith(Names.NameMrpackExt))
                        {
                            WindowManager.ShowAddGame(null, false, item.GetPath());
                        }
                        else if (item.Name.EndsWith(GuiNames.NameColorMCExt))
                        {
                            if (DataContext is not MainModel model)
                            {
                                return;
                            }
                            if (!InstancesPath.IsNotGame)
                            {
                                var res = await model.Model.ShowAsync(LanguageUtils.Get("MainWindow.Info45"));
                                if (res is not true)
                                {
                                    return;
                                }
                            }
                            BaseBinding.ReadBuildConfig(model.Model, item);
                        }

                        break;
                    }
            }
        }
    }

    private void SwitchView()
    {
        var model = (DataContext as MainModel)!;
        if (model.IsSimple)
        {
            _simple ??= new();
            Content1.Child = _simple;
            model.LoadSimpleGames();
        }
        else if (model.IsOneGame || model.IsGameError)
        {
            _oneGame ??= new();
            Content1.Child = _oneGame;
        }
        else if (model.IsNotGame && Content1.Child is not MainEmptyControl)
        {
            _emptyGame ??= new();
            Content1.Child = _emptyGame;
            model.LoadEmptyGame();
        }
        else
        {
            _games ??= new();
            Content1.Child = _games;
        }
    }

    public override void ControlStateChange(WindowState state)
    {
        if (DataContext is MainModel model)
        {
            model.Render = state != WindowState.Minimized;
        }
    }

    public override void Closed()
    {
        WindowManager.MainWindow = null;

        ColorMCGui.Exit();
    }

    public override async void Opened()
    {
        var config = GuiConfigUtils.Config.ServerCustom;
        if (BaseBinding.NewStart || config.CustomStart)
        {
            MainView.Opacity = 0;
            var con1 = new MainStartControl();
            Start.Child = con1;
            Start.IsVisible = true;
            await con1.Start(config.CustomStart ? config.DisplayType : DisplayType.LeftRight, config.StartText, ImageManager.GetStartIcon());
            await ThemeManager.CrossFade.Start(Start, MainView, CancellationToken.None);
            Start.IsVisible = false;
            Start.Child = null;
        }

        if (ColorMCGui.IsCrash)
        {
            var model = (DataContext as MainModel)!;
            model.Model.Show(LanguageUtils.Get("MainWindow.Error2"));
        }
    }

    public override async Task<bool> Closing()
    {
        var model = (DataContext as MainModel)!;
        if (model.IsLaunch)
        {
            var res = await model.Model.ShowAsync(LanguageUtils.Get("MainWindow.Info34"));
            if (res)
            {
                return false;
            }
            return true;
        }

        if (GameManager.IsGameRuning())
        {
            App.Hide();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 游戏实例退出
    /// </summary>
    /// <param name="uuid">游戏实例UUID</param>
    public void GameClose(string uuid)
    {
        (DataContext as MainModel)!.GameClose(uuid);
    }

    /// <summary>
    /// 初始化完成
    /// </summary>
    public void LoadDone()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as MainModel)?.LoadDone();
        });
    }

    /// <summary>
    /// 加载游戏列表
    /// </summary>
    public void LoadGameItem()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as MainModel)?.LoadGameItem();
        });
    }

    /// <summary>
    /// Motd加载
    /// </summary>
    public void MotdLoad()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as MainModel)?.LoadMotd();
        });
    }

    protected override TopModel GenModel(BaseModel model)
    {
        var amodel = new MainModel(model);
        amodel.PropertyChanged += Amodel_PropertyChanged;
        return amodel;
    }

    private void Amodel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == MainModel.SwitchView)
        {
            SwitchView();
        }
        else if (e.PropertyName == nameof(MainModel.HaveCard))
        {
            if (DataContext is MainModel model)
            {
                if (!model.HaveCard)
                {
                    if (!model.MinMode)
                    {
                        model.SideDisplay = false;
                    }
                    ContentOut.Margin = new(0, 0, 20, 0);
                }
                else
                {
                    if (!model.MinMode)
                    {
                        model.SideDisplay = true;
                        ContentOut.Margin = new(0, 0, 10, 0);
                    }
                    else
                    {
                        model.SideDisplay = false;
                        ContentOut.Margin = new(0, 0, 20, 0);
                    }
                }
            }
        }
        else if (e.PropertyName == TopModel.MinModeName)
        {
            if (DataContext is MainModel model)
            {
                if (model.MinMode)
                {
                    //TopRight.IsVisible = false;

                    Head.Children.Remove(HeadTop);
                    ContentTop.Children.Add(HeadTop);
                    HeadTop.Margin = new(0, 0, 0, 10);

                    //TopRight.Child = null;
                    //ContentTop.Children.Add(UserButton);
                    //UserButton.Margin = new(0, 0, 0, 10);

                    Right.Child = null;
                    ContentTop.Children.Add(RightSide);

                    model.SideDisplay = false;
                    ContentOut.Margin = new(0, 0, 20, 0);
                }
                else
                {
                    //TopRight.IsVisible = true;

                    ContentTop.Children.Remove(HeadTop);
                    Head.Children.Add(HeadTop);
                    HeadTop.Margin = new(0, 0, 10, 0);

                    //ContentTop.Children.Remove(UserButton);
                    //TopRight.Child = UserButton;
                    //UserButton.Margin = new(0);

                    ContentTop.Children.Remove(RightSide);
                    Right.Child = RightSide;

                    if (!model.HaveCard)
                    {
                        model.SideDisplay = false;
                        ContentOut.Margin = new(0, 0, 20, 0);
                    }
                    else
                    {
                        model.SideDisplay = true;
                        ContentOut.Margin = new(0, 0, 10, 0);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 主界面隐藏
    /// </summary>
    public void Hide()
    {
        if (DataContext is MainModel model)
        {
            model.Render = false;
        }
    }

    /// <summary>
    /// 主界面还原
    /// </summary>
    public void Show()
    {
        if (DataContext is MainModel model)
        {
            model.Render = true;
        }
    }

    /// <summary>
    /// 显示今日幸运方块
    /// </summary>
    public void ReloadBlock()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (DataContext is MainModel model)
            {
                model.LoadBlock();
            }
        });
    }
}
