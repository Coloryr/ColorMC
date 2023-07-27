using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
using System.ComponentModel;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddModPackControl : UserControl, IUserControl, IAddWindow
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title => App.GetLanguage("AddModPackWindow.Title");

    private readonly AddModPackControlModel _model;

    public AddModPackControl()
    {
        InitializeComponent();

        _model = new(this);
        _model.PropertyChanged += Model_PropertyChanged;
        DataContext = _model;

        DataGridFiles.DoubleTapped += DataGridFiles_DoubleTapped;

        Grid1.PointerPressed += Grid1_PointerPressed;

        Input1.KeyDown += Input1_KeyDown;
    }

    public void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            _model.Reload1();
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "DisplayList")
        {
            Dispatcher.UIThread.Post(ScrollViewer1.ScrollToHome);
        }
        else if (e.PropertyName == "Display")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_model.Display)
                {
                    App.CrossFade300.Start(null, Grid1, CancellationToken.None);
                }
                else
                {
                    App.CrossFade300.Start(Grid1, null, CancellationToken.None);
                }
            });
        }
    }

    private async void Grid1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            await _model.Download();
            e.Handled = true;
        }
    }

    private void Input1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _model.Reload();
        }
    }

    private async void DataGridFiles_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        await _model.Download();
    }

    public void Closed()
    {
        App.AddModPackWindow = null;
    }

    public void SetSelect(FileItemModel last)
    {
        _model.SetSelect(last);
    }

    public void Install(FileItemModel item)
    {
        _model.Install();
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        DataGridFiles.SetFontColor();

        _model.Source = 0;
    }

    public void Back()
    {
        if (_model.Page <= 0)
            return;

        _model.Page -= 1;
    }

    public void Next()
    {
        _model.Page += 1;
    }
}
