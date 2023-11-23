using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Animations;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class MainControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => App.Lang("MainWindow.Title");

    public readonly SelfPageSlideSide SidePageSlide300 = new(TimeSpan.FromMilliseconds(300));

    public string UseName { get; }

    public MainControl()
    {
        InitializeComponent();

        UseName = ToString() ?? "MainControl";

        ScrollViewer1.PointerPressed += ScrollViewer1_PointerPressed;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);

        SizeChanged += MainControl_SizeChanged;
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

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "GroupEnable")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if ((DataContext as MainModel)!.GroupEnable)
                {
                    App.CrossFade100.Start(null, StackPanel1);
                }
                else
                {
                    App.CrossFade100.Start(StackPanel1, null);
                }
            });
        }
    }

    private void ScrollViewer1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(ScrollViewer1).Properties.IsLeftButtonPressed)
        {
            (DataContext as MainModel)!.Select(null);
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
            Label1.Content = App.Lang("Gui.Info6");
        }
        else if (e.Data.Contains(DataFormats.Files))
        {
            Grid2.IsVisible = true;
            Label1.Content = App.Lang("Gui.Info7");
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
                App.ShowUser(false, str);
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
        (DataContext as MainModel)!.Render = state != WindowState.Minimized;

        if (SystemInfo.Os == OsType.Windows)
        {
            if (state == WindowState.Maximized)
            {
                Margin = new Avalonia.Thickness(10);
            }
            else
            {
                Margin = new Avalonia.Thickness(0);
            }
        }
    }

    public void Closed()
    {
        ColorMCCore.GameLaunch = null;
        ColorMCCore.GameRequest = null;
        ColorMCCore.OfflineLaunch = null;

        App.MainWindow = null;

        App.Close();
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        ChangeLive2DSize();

        if (BaseBinding.NewStart)
        {
            Start.Start();
        }
    }

    public async Task<bool> Closing()
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
        if (App.FrpProcess != null)
        {
            var res = await model.Model.ShowWait(App.Lang("NetFrpWindow.Tab3.Info2"));
            if (res)
            {
                App.FrpProcess.Kill(true);
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
        (DataContext as MainModel)!.GameClose(uuid);
    }

    public void LoadDone()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as MainModel)!.LoadDone();
        });
    }

    public void LoadMain()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as MainModel)!.Load();
        });
    }

    public void MotdLoad()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as MainModel)!.MotdLoad();
        });
    }

    public void IsDelete()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as MainModel)!.IsDelete();
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

    public void ShowMessage(string message)
    {
        (DataContext as MainModel)!.ShowMessage(message);
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new MainModel(model);
        amodel.PropertyChanged += Model_PropertyChanged;
        DataContext = amodel;

        var config = GuiConfigUtils.Config.Live2D;
        amodel.Live2dWidth = (int)(Bounds.Width * ((float)config.Width / 100));
        amodel.Live2dHeight = (int)(Bounds.Height * ((float)config.Height / 100));
    }
}
