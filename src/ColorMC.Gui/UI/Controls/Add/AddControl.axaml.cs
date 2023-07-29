using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Objs;
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

    public UserControl Con => this;

    public string Title => string.Format(App.GetLanguage("AddWindow.Title"), Obj.Name);

    private readonly AddControlModel _model;

    public AddControl() : this(new() { Empty = true })
    {

    }

    public AddControl(GameSettingObj obj)
    {
        Obj = obj;

        InitializeComponent();

        _model = new AddControlModel(this, obj);
        _model.PropertyChanged += Model_PropertyChanged;

        DataContext = _model;

        DataGridFiles.DoubleTapped += DataGridFiles_DoubleTapped;

        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;
        DataGrid2.DoubleTapped += DataGrid2_DoubleTapped;

        Grid1.PointerPressed += Grid1_PointerPressed;
    }

    public void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            _model.Reload();
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "OptifineDisplay")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_model.OptifineDisplay == true)
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
                if (_model.ModDownloadDisplay == true)
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
                if (_model.VersionDisplay == true)
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
    }

    private void Grid1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            _model.VersionClose();
            e.Handled = true;
        }
    }
    private void DataGrid2_DoubleTapped(object? sender, TappedEventArgs e)
    {
        var item = _model.Mod;
        if (item != null)
        {
            item.Download = !item.Download;
        }
    }

    private async void DataGrid1_DoubleTapped(object? sender, TappedEventArgs e)
    {
        await _model.DownloadOptifine();
    }

    private async void DataGridFiles_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        await _model.GoFile();
    }

    public void Closed()
    {
        _model.DisplayList.Clear();

        App.AddWindows.Remove(Obj.UUID);

        if (_model.Set)
            _model.Set = false;
    }

    public void GoFile(SourceType type, string pid)
    {
        _model.GoFile(type, pid);
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        DataGridFiles.SetFontColor();
        DataGrid1.SetFontColor();
        DataGrid2.SetFontColor();

        _model.Display = true;
    }

    public Task GoSet()
    {
        return _model.GoSet();
    }

    public void GoTo(FileType type)
    {
        _model.GoTo(type);
    }
}
