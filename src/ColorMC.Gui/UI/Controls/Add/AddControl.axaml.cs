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

    public string Title => string.Format(App.GetLanguage("AddWindow.Title"), Obj.Name);

    public AddControl()
    {
        InitializeComponent();
    }

    public AddControl(GameSettingObj obj) : this()
    {
        Obj = obj;

        DataGridFiles.DoubleTapped += DataGridFiles_DoubleTapped;

        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;
        DataGrid2.DoubleTapped += DataGrid2_DoubleTapped;

        Grid1.PointerPressed += Grid1_PointerPressed;
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
                    App.CrossFade300.Start(null, Grid2);
                }
                else
                {
                    App.CrossFade300.Start(Grid2, null);
                }
            });
        }
        else if (e.PropertyName == "ModDownloadDisplay")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (model.ModDownloadDisplay == true)
                {
                    App.CrossFade300.Start(null, Grid4);
                }
                else
                {
                    App.CrossFade300.Start(Grid4, null);
                }
            });
        }
        else if (e.PropertyName == "VersionDisplay")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (model.VersionDisplay == true)
                {
                    App.CrossFade300.Start(null, Grid1);
                }
                else
                {
                    App.CrossFade300.Start(Grid1, null);
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

    private void Grid1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            (DataContext as AddControlModel)!.VersionClose();
            e.Handled = true;
        }
    }
    private void DataGrid2_DoubleTapped(object? sender, TappedEventArgs e)
    {
        var item = (DataContext as AddControlModel)!.Mod;
        if (item != null)
        {
            item.Download = !item.Download;
        }
    }

    private async void DataGrid1_DoubleTapped(object? sender, TappedEventArgs e)
    {
        await (DataContext as AddControlModel)!.DownloadOptifine();
    }

    private async void DataGridFiles_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        await (DataContext as AddControlModel)!.GoFile();
    }

    public void Closed()
    {
        App.AddWindows.Remove(Obj.UUID);

        var model = (DataContext as AddControlModel)!;

        if (model.Set)
        {
            model.Set = false;
        }
    }

    public void GoFile(SourceType type, string pid)
    {
        (DataContext as AddControlModel)!.GoFile(type, pid);
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        DataGridFiles.SetFontColor();
        DataGrid1.SetFontColor();
        DataGrid2.SetFontColor();

        (DataContext as AddControlModel)!.Display = true;
    }

    public Task GoSet()
    {
        return (DataContext as AddControlModel)!.GoSet();
    }

    public void GoTo(FileType type)
    {
        (DataContext as AddControlModel)?.GoTo(type);
    }
}
