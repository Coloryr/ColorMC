using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddControl : UserControl, IUserControl
{
    public GameSettingObj Obj { get; }

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => string.Format(App.Lang("AddWindow.Title"), Obj.Name);

    public AddControl()
    {
        InitializeComponent();
    }

    public AddControl(GameSettingObj obj) : this()
    {
        Obj = obj;

        VersionFiles.DoubleTapped += VersionFiles_DoubleTapped;
        OptifineFiles.DoubleTapped += OptifineFiles_DoubleTapped;
        ModDownloadFiles.DoubleTapped += ModDownloadFiles_DoubleTapped;

        VersionDisplay.PointerPressed += VersionDisplay_PointerPressed;
        OptifineDisplay.PointerPressed += OptifineDisplay_PointerPressed;
        ModDownloadDisplay.PointerPressed += ModDownloadDisplay_PointerPressed;
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new AddControlModel(model, Obj);
        amodel.PropertyChanged += Model_PropertyChanged;

        DataContext = amodel;
    }

    public void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            (DataContext as AddControlModel)!.Reload();
        }
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
                }
                else
                {
                    App.CrossFade300.Start(OptifineDisplay, null);
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
                }
                else
                {
                    App.CrossFade300.Start(ModDownloadDisplay, null);
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
                }
                else
                {
                    App.CrossFade300.Start(VersionDisplay, null);
                }
            });
        }
        else if (e.PropertyName == "DisplayList")
        {
            ScrollViewer1.ScrollToHome();
        }
        else if (e.PropertyName == "WindowClose")
        {
            Window.Close();
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

    public void Closed()
    {
        App.AddWindows.Remove(Obj.UUID);
    }

    public void GoFile(SourceType type, string pid)
    {
        (DataContext as AddControlModel)!.GoFile(type, pid);
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        VersionFiles.SetFontColor();
        OptifineFiles.SetFontColor();
        ModDownloadFiles.SetFontColor();

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
}
