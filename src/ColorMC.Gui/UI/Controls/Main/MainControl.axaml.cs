using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class MainControl : UserControl, IUserControl
{
    private readonly MainModel _model;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title => App.GetLanguage("MainWindow.Title");

    public BaseModel Model => _model;

    public MainControl()
    {
        InitializeComponent();

        _model = new(this);
        _model.PropertyChanged += Model_PropertyChanged;
        DataContext = _model;

        Grid3.PointerPressed += Grid3_PointerPressed;
        ScrollViewer1.PointerPressed += ScrollViewer1_PointerPressed;

        Image1.PointerPressed += Image1_PointerPressed;
        Image1.PointerEntered += Image1_PointerEntered;
        Image1.PointerExited += Image1_PointerExited;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);

        KeyDown += MainControl_KeyDown;

        SizeChanged += MainControl_SizeChanged;

        var config = GuiConfigUtils.Config.Live2D;
        _model.Live2dWidth = (int)(Bounds.Width * ((float)config.Width / 100));
        _model.Live2dHeight = (int)(Bounds.Height * ((float)config.Height / 100));
    }

    private void MainControl_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var config = GuiConfigUtils.Config.Live2D;
        _model.Live2dWidth = (int)(Bounds.Width * ((float)config.Width / 100));
        _model.Live2dHeight = (int)(Bounds.Height * ((float)config.Height / 100));
    }

    private void Image1_PointerExited(object? sender, PointerEventArgs e)
    {
        Border2.IsVisible = false;
    }

    private void Image1_PointerEntered(object? sender, PointerEventArgs e)
    {
        Border2.IsVisible = true;
    }

    private void Image1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            App.ShowUser();
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "GroupEnable")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_model.GroupEnable)
                {
                    App.CrossFade100.Start(null, StackPanel1);
                }
                else
                {
                    App.CrossFade100.Start(StackPanel1, null);
                }
            });
        }
        else if (e.PropertyName == "SideDisplay")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_model.SideDisplay)
                {
                    App.CrossFade100.Start(null, Grid1);
                }
                else
                {
                    App.CrossFade100.Start(Grid1, null);
                }
            });
        }
    }

    private void MainControl_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {

        }
    }

    private void ScrollViewer1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(ScrollViewer1).Properties.IsLeftButtonPressed)
        {
            _model.Select(null);
        }
    }

    private void Grid3_PointerPressed(object? sender, PointerEventArgs e)
    {
        App.ShowAddGame();
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
            Label1.Content = App.GetLanguage("Gui.Info6");
        }
        else if (e.Data.Contains(DataFormats.Files))
        {
            Grid2.IsVisible = true;
            Label1.Content = App.GetLanguage("Gui.Info7");
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
            if (str?.StartsWith("authlib-injector:yggdrasil-server:") == true)
            {
                App.ShowUser(str);
            }
        }
        else if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files == null || files.Count() > 1)
                return;

            var item = files.ToList()[0].GetPath();
            if (item == null)
                return;
            if (item.EndsWith(".zip") || item.EndsWith(".mrpack"))
            {
                App.ShowAddGame(item);
            }
        }
    }

    public void WindowStateChange(WindowState state)
    {
        _model.Render = state != WindowState.Minimized;
    }

    public void Closed()
    {
        ColorMCCore.GameLaunch = null;
        ColorMCCore.GameDownload = null;
        ColorMCCore.OfflineLaunch = null;

        App.MainWindow = null;

        App.Close();
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        _model.Open();
    }

    private void Item_DoubleTapped(object? sender, TappedEventArgs e)
    {
        _model.Launch();
    }

    public async Task<bool> Closing()
    {
        var windows = App.FindRoot(VisualRoot);
        if (_model.IsLaunch)
        {
            var res = await windows.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info34"));
            if (res)
            {
                return false;
            }
            return true;
        }

        if (BaseBinding.IsGameRuning())
        {
            App.Hide();
            return true;
        }

        return false;
    }

    public void GameClose(string uuid)
    {
        _model.GameClose(uuid);
    }

    public void LoadMain()
    {
        Dispatcher.UIThread.Post(_model.Load);
    }

    public void MotdLoad()
    {
        Dispatcher.UIThread.Post(_model.MotdLoad);
    }

    public void IsDelete()
    {
        Dispatcher.UIThread.Post(_model.IsDelete);
    }

    public void ChangeModel()
    {
        _model.ChangeModel();
    }

    public void DeleteModel()
    {
        _model.DeleteModel();
    }

    public void ChangeLive2DSize()
    {
        var config = GuiConfigUtils.Config.Live2D;
        _model.Live2dWidth = (int)(Bounds.Width * ((float)config.Width / 100));
        _model.Live2dHeight = (int)(Bounds.Height * ((float)config.Height / 100));
    }

    public void ShowMessage(string message)
    {
        _model.ShowMessage(message);
    }

    public void MirrorChange()
    {
        _model.Mirror();
    }
}
