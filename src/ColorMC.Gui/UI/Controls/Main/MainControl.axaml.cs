using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core;
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
        AddHandler(DragDrop.DropEvent, Drop);

        SizeChanged += MainControl_SizeChanged;
        BaseBinding.LoadDone += LoadDone;
    }

    public override Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F && e.KeyModifiers == KeyModifiers.Control)
        {
            if (DataContext is MainModel model
                && Content1.Child is MainGamesControl con)
            {
                if (model.GameSearch)
                {
                    model.SearchClose();
                }
                else
                {
                    model.Search();
                    con.Search.Focus();
                }
            }
        }

        return Task.FromResult(false);
    }

    private void MainControl_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var config = GuiConfigUtils.Config.Live2D;
        if (DataContext is MainModel model)
        {
            model.Live2dWidth = (int)(Bounds.Width * ((float)config.Width / 100));
            model.Live2dHeight = (int)(Bounds.Height * ((float)config.Height / 100));
        }
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(BaseBinding.DrapType))
        {
            return;
        }
        if (e.Data.Contains(DataFormats.Text))
        {
            Grid2.IsVisible = true;
            Label1.Text = App.Lang("UserWindow.Text8");
        }
        else if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files == null || files.Count() > 1)
                return;

            var item = files.ToList()[0];
            if (item == null)
                return;
            if (item is IStorageFolder forder && Directory.Exists(forder.GetPath()))
            {
                Grid2.IsVisible = true;
                Label1.Text = App.Lang("AddGameWindow.Text2");
            }
            else if (item.Name.EndsWith(Names.NameZipExt) || item.Name.EndsWith(Names.NameMrpackExt))
            {
                Grid2.IsVisible = true;
                Label1.Text = App.Lang("MainWindow.Text25");
            }
            else if (item.Name.EndsWith(GuiNames.NameColorMCExt))
            {
                Grid2.IsVisible = true;
                Label1.Text = App.Lang("MainWindow.Text38");
            }
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
    }

    private async void Drop(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(BaseBinding.DrapType))
        {
            return;
        }
        Grid2.IsVisible = false;
        if (e.Data.Contains(DataFormats.Text))
        {
            var str = e.Data.GetText();
            if (str == null)
            {
                return;
            }
            if (str.StartsWith(GuiNames.NameAuthlibKey))
            {
                WindowManager.ShowUser(false, url: str);
            }
            else if (str.StartsWith(GuiNames.NameColorMCCloudKey, StringComparison.CurrentCultureIgnoreCase))
            {
                BaseBinding.SetCloudKey(str);
            }
        }
        else if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files == null || files.Count() > 1)
                return;

            var item = files.ToList()[0];
            if (item == null)
                return;
            if (item is IStorageFolder forder && Directory.Exists(forder.GetPath()))
            {
                WindowManager.ShowAddGame(null, true, forder.GetPath());
            }
            else if (item.Name.EndsWith(Names.NameZipExt) || item.Name.EndsWith(Names.NameMrpackExt))
            {
                WindowManager.ShowAddGame(null, false, item.GetPath());
            }
            else if (item.Name.EndsWith(GuiNames.NameColorMCExt))
            {
                if (DataContext is not MainModel model)
                {
                    return;
                }
                if (!GameBinding.IsNotGame)
                {
                    var res = await model.Model.ShowAsync(App.Lang("MainWindow.Info45"));
                    if (res is not true)
                    {
                        return;
                    }
                }
                BaseBinding.ReadBuildConfig(model.Model, item);
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
        ChangeLive2DSize();

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
            model.Model.Show(App.Lang("MainWindow.Error2"));
        }
    }

    public override async Task<bool> Closing()
    {
        var model = (DataContext as MainModel)!;
        if (model.IsLaunch)
        {
            var res = await model.Model.ShowAsync(App.Lang("MainWindow.Info34"));
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

    /// <summary>
    /// 更换L2D模型
    /// </summary>
    public void ChangeModel()
    {
        (DataContext as MainModel)!.ChangeModel();
    }

    /// <summary>
    /// 删除L2D模型
    /// </summary>
    public void DeleteModel()
    {
        (DataContext as MainModel)!.DeleteModel();
    }

    /// <summary>
    /// 修改L2D渲染大小
    /// </summary>
    public void ChangeLive2DSize()
    {
        var config = GuiConfigUtils.Config.Live2D;
        var model = (DataContext as MainModel)!;
        model.Live2dWidth = (int)(Bounds.Width * ((float)config.Width / 100));
        model.Live2dHeight = (int)(Bounds.Height * ((float)config.Height / 100));
        model.L2dPos = (HorizontalAlignment)((config.Pos % 3) + 1);
        model.L2dPos1 = (VerticalAlignment)((config.Pos / 3) + 1);
    }

    /// <summary>
    /// 切换L2D模型
    /// </summary>
    public void ChangeLive2DMode()
    {
        var config = GuiConfigUtils.Config.Live2D;
        var model = (DataContext as MainModel)!;

        model.LowFps = config.LowFps;
    }

    /// <summary>
    /// L2D模型显示文字
    /// </summary>
    /// <param name="message">文本</param>
    public void ShowMessage(string message)
    {
        (DataContext as MainModel)!.L2dShowMessage(message);
    }

    protected override TopModel GenModel(BaseModel model)
    {
        var amodel = new MainModel(model);
        amodel.PropertyChanged += Amodel_PropertyChanged;

        var config = GuiConfigUtils.Config.Live2D;
        amodel.Live2dWidth = (int)(Bounds.Width * ((float)config.Width / 100));
        amodel.Live2dHeight = (int)(Bounds.Height * ((float)config.Height / 100));

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
                }
                else
                {
                    if (!model.MinMode)
                    {
                        model.SideDisplay = true;
                    }
                    else
                    {
                        model.SideDisplay = false;
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
                    Scroll.Children.Remove(GameViews);
                    Stack.Children.Add(GameViews);
                    Cards.Content = null;
                    Stack.Children.Insert(0, RightSide);
                    Side.Children.Remove(UserButton);
                    Stack.Children.Insert(0, UserButton);
                    Side.Children.Remove(Buttons);
                    Stack.Children.Insert(0, Buttons);
                    Side.IsVisible = false;
                    Stack.Margin = new(0, 0, 10, 0);
                }
                else
                {
                    Stack.Children.Remove(GameViews);
                    Scroll.Children.Add(GameViews);
                    Stack.Children.Remove(RightSide);
                    Cards.Content = RightSide;
                    Stack.Children.Remove(UserButton);
                    Side.Children.Insert(0, UserButton);
                    Stack.Children.Remove(Buttons);
                    Side.Children.Insert(0, Buttons);
                    Side.IsVisible = true;
                    Stack.Margin = new(0, 0, 0, 0);
                }
            }
        }
    }

    /// <summary>
    /// 游戏实例图标修改
    /// </summary>
    /// <param name="uuid">游戏实例UUID</param>
    public void IconChange(string uuid)
    {
        if (DataContext is MainModel model)
        {
            model.IconChange(uuid);
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
}
