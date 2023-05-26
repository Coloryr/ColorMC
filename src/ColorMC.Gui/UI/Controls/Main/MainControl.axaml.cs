using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class MainControl : UserControl, IUserControl
{
    private readonly MainModel model;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public MainControl()
    {
        InitializeComponent();

        model = new(this);
        model.PropertyChanged += Model_PropertyChanged;
        DataContext = model;

        Grid3.PointerPressed += Grid3_PointerPressed;
        ScrollViewer1.PointerPressed += ScrollViewer1_PointerPressed;

        Image1.PointerPressed += Image1_PointerPressed;
        Image1.PointerEntered += Image1_PointerEntered;
        Image1.PointerExited += Image1_PointerExited;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);

        KeyDown += MainControl_KeyDown;
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
                if (model.GroupEnable)
                {
                    App.CrossFade100.Start(null, StackPanel1, CancellationToken.None);
                }
                else
                {
                    App.CrossFade100.Start(StackPanel1, null, CancellationToken.None);
                }
            });
        }
        else if (e.PropertyName == "SideDisplay")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (model.SideDisplay)
                {
                    App.CrossFade100.Start(null, Grid1, CancellationToken.None);
                }
                else
                {
                    App.CrossFade100.Start(Grid1, null, CancellationToken.None);
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
            model.Select(null);
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

            var item = files.First().GetPath();
            if (item == null)
                return;
            if (item.EndsWith(".zip") || item.EndsWith(".mrpack"))
            {
                App.ShowAddGame(item);
            }
        }
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
        Window.SetTitle(App.GetLanguage("MainWindow.Title"));

        model.Open();
    }

    private void Item_DoubleTapped(object? sender, TappedEventArgs e)
    {
        model.Launch();
    }

    public async Task<bool> Closing()
    {
        var windows = App.FindRoot(VisualRoot);
        if (model.launch)
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
        model.GameClose(uuid);
    }

    public void LoadMain()
    {
        Dispatcher.UIThread.Post(model.Load);
    }

    public void MotdLoad()
    {
        Dispatcher.UIThread.Post(model.MotdLoad);
    }

    public void IsDelete()
    {
        Dispatcher.UIThread.Post(model.IsDelete);
    }
}
