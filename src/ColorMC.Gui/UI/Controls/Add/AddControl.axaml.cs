using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddControl : BaseUserControl
{
    private readonly GameSettingObj _obj;

    public AddControl()
    {
        InitializeComponent();
    }

    public AddControl(GameSettingObj obj) : this()
    {
        _obj = obj;

        Title = string.Format(App.Lang("AddWindow.Title"), obj.Name);
        UseName = (ToString() ?? "GameSettingObj") + ":" + obj.UUID;

        VersionFiles.DoubleTapped += VersionFiles_DoubleTapped;
        OptifineFiles.DoubleTapped += OptifineFiles_DoubleTapped;
        ModDownloadFiles.DoubleTapped += ModDownloadFiles_DoubleTapped;

        VersionDisplay.PointerPressed += VersionDisplay_PointerPressed;
        OptifineDisplay.PointerPressed += OptifineDisplay_PointerPressed;
        ModDownloadDisplay.PointerPressed += ModDownloadDisplay_PointerPressed;
    }

    public override TopModel GenModel(BaseModel model)
    {
        var amodel = new AddControlModel(model, _obj);
        amodel.PropertyChanged += Model_PropertyChanged;
        return amodel;
    }

    public override Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            (DataContext as AddControlModel)!.Reload();

            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
    public override Bitmap GetIcon()
    {
        var icon = ImageManager.GetGameIcon(_obj);
        return icon ?? ImageManager.GameIcon;
    }

    public override void Closed()
    {
        WindowManager.AddWindows.Remove(_obj.UUID);
    }

    public override void Opened()
    {
        Window.SetTitle(Title);

        (DataContext as AddControlModel)!.Display = true;
    }

    public async Task GoSet()
    {
        await (DataContext as AddControlModel)!.GoSet();
    }

    public void GoTo(FileType type)
    {
        (DataContext as AddControlModel)?.GoTo(type);
    }

    public void GoFile(SourceType type, string pid)
    {
        (DataContext as AddControlModel)!.GoFile(type, pid);
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var model = (DataContext as AddControlModel)!;
        if (e.PropertyName == "OptifineDisplay")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (model.OptifineDisplay == true)
                {
                    App.CrossFade300.Start(null, OptifineDisplay);
                    App.CrossFade300.Start(ScrollViewer1, null);
                }
                else
                {
                    App.CrossFade300.Start(OptifineDisplay, null);
                    App.CrossFade300.Start(null, ScrollViewer1);
                }
            });
        }
        else if (e.PropertyName == "ModDownloadDisplay")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (model.ModDownloadDisplay == true)
                {
                    App.CrossFade300.Start(null, ModDownloadDisplay);
                    App.CrossFade300.Start(ScrollViewer1, null);
                }
                else
                {
                    App.CrossFade300.Start(ModDownloadDisplay, null);
                    App.CrossFade300.Start(null, ScrollViewer1);
                }
            });
        }
        else if (e.PropertyName == "VersionDisplay")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (model.VersionDisplay == true)
                {
                    App.CrossFade300.Start(null, VersionDisplay);
                    App.CrossFade300.Start(ScrollViewer1, null);
                }
                else
                {
                    App.CrossFade300.Start(VersionDisplay, null);
                    App.CrossFade300.Start(null, ScrollViewer1);
                }
            });
        }
        else if (e.PropertyName == "ScrollToHome")
        {
            ScrollViewer1.ScrollToHome();
        }
    }

    private void VersionDisplay_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            (DataContext as AddControlModel)!.VersionDisplay = false;
            e.Handled = true;
        }
    }
    private void OptifineDisplay_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            (DataContext as AddControlModel)!.OptifineDisplay = false;
            e.Handled = true;
        }
    }
    private void ModDownloadDisplay_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            (DataContext as AddControlModel)!.ModDownloadDisplay = false;
            e.Handled = true;
        }
    }
    private void ModDownloadFiles_DoubleTapped(object? sender, TappedEventArgs e)
    {
        var item = (DataContext as AddControlModel)!.Mod;
        if (item != null)
        {
            item.Download = !item.Download;
        }
    }

    private async void OptifineFiles_DoubleTapped(object? sender, TappedEventArgs e)
    {
        await (DataContext as AddControlModel)!.DownloadOptifine();
    }

    private async void VersionFiles_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        await (DataContext as AddControlModel)!.GoFile();
    }
}
