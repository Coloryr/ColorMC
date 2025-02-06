using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class MainControl : BaseUserControl
{
    public const string DialogName = "MainCon";

    private MainOneGameControl? _oneGame;
    private MainEmptyControl? _emptyGame;
    private MainGamesControl? _games;

    public MainControl() : base(WindowManager.GetUseName<MainControl>())
    {
        InitializeComponent();

        Title = "ColorMC";

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);

        SizeChanged += MainControl_SizeChanged;
        HelloText.PointerPressed += HelloText_PointerPressed;
        BaseBinding.LoadDone += LoadDone;
    }

    private void HelloText_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as MainModel)?.HelloClick();
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
            else if (item.Name.EndsWith(".zip") || item.Name.EndsWith(".mrpack"))
            {
                Grid2.IsVisible = true;
                Label1.Text = App.Lang("MainWindow.Text25");
            }
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
    }

    private void Drop(object? sender, DragEventArgs e)
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
            if (str.StartsWith("authlib-injector:yggdrasil-server:"))
            {
                WindowManager.ShowUser(false, url: str);
            }
            else if (str.StartsWith("cloudkey:") || str.StartsWith("cloudKey:"))
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
            else if (item.Name.EndsWith(".zip") || item.Name.EndsWith(".mrpack"))
            {
                WindowManager.ShowAddGame(null, false, item.GetPath());
            }
        }
    }

    private void SwitchView()
    {
        var model = (DataContext as MainModel)!;
        if (model.IsOneGame || model.IsGameError)
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

    public override void WindowStateChange(WindowState state)
    {
        if (DataContext is MainModel model)
        {
            model.Render = state != WindowState.Minimized;
        }
    }

    public override void Closed()
    {
        WindowManager.MainWindow = null;

        App.Exit();
    }

    public override async void Opened()
    {
        ChangeLive2DSize();

        if (BaseBinding.NewStart)
        {
            MainView.Opacity = 0;
            var con1 = new MainStartControl();
            Start.Child = con1;
            Start.IsVisible = true;
            await con1.Start();
            await ThemeManager.CrossFade.Start(Start, MainView, CancellationToken.None);
            Start.IsVisible = false;
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
            var res = await model.Model.ShowWait(App.Lang("MainWindow.Info34"));
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

    public void GameClose(string uuid)
    {
        (DataContext as MainModel)!.GameClose(uuid);
    }

    public void LoadDone()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as MainModel)?.LoadDone();
        });
    }

    public void LoadMain()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as MainModel)?.LoadGameItem();
        });
    }

    public void MotdLoad()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as MainModel)?.LoadMotd();
        });
    }

    public void IsDelete()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as MainModel)?.IsDelete();
        });
    }

    public void ChangeModel()
    {
        (DataContext as MainModel)!.ChangeModel();
    }

    public void DeleteModel()
    {
        (DataContext as MainModel)!.DeleteModel();
    }

    public void ChangeLive2DSize()
    {
        var config = GuiConfigUtils.Config.Live2D;
        var model = (DataContext as MainModel)!;
        model.Live2dWidth = (int)(Bounds.Width * ((float)config.Width / 100));
        model.Live2dHeight = (int)(Bounds.Height * ((float)config.Height / 100));
        model.L2dPos = (HorizontalAlignment)((config.Pos % 3) + 1);
        model.L2dPos1 = (VerticalAlignment)((config.Pos / 3) + 1);
    }

    public void ChangeLive2DMode()
    {
        var config = GuiConfigUtils.Config.Live2D;
        var model = (DataContext as MainModel)!;

        model.LowFps = config.LowFps;
    }

    public void ShowMessage(string message)
    {
        (DataContext as MainModel)!.ShowMessage(message);
    }

    public override TopModel GenModel(BaseModel model)
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
                    TopRight.IsVisible = false;

                    Head.Children.Remove(HeadTop);
                    ContentTop.Children.Add(HeadTop);
                    HeadTop.Margin = new(0, 0, 0, 10);

                    TopRight.Child = null;
                    ContentTop.Children.Add(UserButton);
                    UserButton.Margin = new(0, 0, 0, 10);

                    Right.Child = null;
                    ContentTop.Children.Add(RightSide);

                    model.SideDisplay = false;
                    ContentOut.Margin = new(0, 0, 20, 0);
                }
                else
                {
                    TopRight.IsVisible = true;

                    ContentTop.Children.Remove(HeadTop);
                    Head.Children.Add(HeadTop);
                    HeadTop.Margin = new(0, 0, 10, 0);

                    ContentTop.Children.Remove(UserButton);
                    TopRight.Child = UserButton;
                    UserButton.Margin = new(0);

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

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }

    public void IconChange(string uuid)
    {
        if (DataContext is MainModel model)
        {
            model.IconChange(uuid);
        }
    }
}
