using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Gui.UI.Animations;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class MainControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => App.Lang("Name");

    public readonly SelfPageSlideSide SidePageSlide300 = new(TimeSpan.FromMilliseconds(300));

    public string UseName { get; }

    public MainControl()
    {
        InitializeComponent();

        UseName = ToString() ?? "MainControl";

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

            if (Bounds.Width > 500 && model.TopSide2 != false)
            {
                model.TopSide2 = false;
                model.TopSide = true;
                model.TopSide1 = true;
            }
            else if (Bounds.Width <= 500 && model.TopSide2 != true)
            {
                model.TopSide2 = true;
                model.TopSide = false;
                model.TopSide1 = true;
            }
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
            Label1.Text = App.Lang("Gui.Info6");
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
                Label1.Text = App.Lang("Gui.Info42");
            }
            else if (item.Name.EndsWith(".zip") || item.Name.EndsWith(".mrpack"))
            {
                Grid2.IsVisible = true;
                Label1.Text = App.Lang("Gui.Info7");
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

            var item = files.ToList()[0];
            if (item == null)
                return;
            if (item is IStorageFolder forder && Directory.Exists(forder.GetPath()))
            {
                App.ShowAddGame(null, true, forder.GetPath());
            }
            else if (item.Name.EndsWith(".zip") || item.Name.EndsWith(".mrpack"))
            {
                App.ShowAddGame(null, false, item.GetPath());
            }
        }
    }

    private void SwitchView()
    {
        var model = (DataContext as MainModel)!;
        if (model.IsOneGame || model.IsGameError)
        {
            if (Content1.Child is not MainOneGameControl)
            {
                Content1.Child = new MainOneGameControl();
            }
        }
        else
        {
            if (model.IsNotGame && Content1.Child is not MainEmptyControl)
            {
                Content1.Child = new MainEmptyControl();
            }
            else if (Content1.Child is not MainGamesControl)
            {
                Content1.Child = new MainGamesControl();
            }
        }
    }

    public void WindowStateChange(WindowState state)
    {
        (DataContext as MainModel)!.Render = state != WindowState.Minimized;
    }

    public void Closed()
    {
        App.MainWindow = null;

        App.Close();
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        ChangeLive2DSize();

        if (BaseBinding.NewStart)
        {
            var con1 = new MainStartControl();
            Start.Child = con1;
            con1.Start(Start);
        }

        if (ColorMCGui.IsCrash)
        {
            var model = (DataContext as MainModel)!;
            model.Model.Show(App.Lang("Gui.Error48"));
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
            (DataContext as MainModel)!.LoadGameItem();
        });
    }

    public void MotdLoad()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as MainModel)!.LoadMotd();
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
        amodel.PropertyChanged += Amodel_PropertyChanged;
        DataContext = amodel;

        var config = GuiConfigUtils.Config.Live2D;
        amodel.Live2dWidth = (int)(Bounds.Width * ((float)config.Width / 100));
        amodel.Live2dHeight = (int)(Bounds.Height * ((float)config.Height / 100));
    }

    private void Amodel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "SwitchView")
        {
            SwitchView();
        }
    }
}
