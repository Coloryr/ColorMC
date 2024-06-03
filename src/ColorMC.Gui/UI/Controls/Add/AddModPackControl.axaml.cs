using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddModPackControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => App.Lang("AddModPackWindow.Title");

    public string UseName { get; }

    public AddModPackControl()
    {
        InitializeComponent();

        UseName = ToString() ?? "AddModPackControl";

        PackFiles.DoubleTapped += PackFiles_DoubleTapped;

        ModPackFiles.PointerPressed += ModPackFiles_PointerPressed;

        Input1.KeyDown += Input1_KeyDown;

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
        ScrollViewer1.ScrollChanged += ScrollViewer1_ScrollChanged;
    }

    private void ScrollViewer1_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (e.ExtentDelta == e.OffsetDelta)
        {
            return;
        }
        if (DataContext is AddModPackControlModel model)
        {
            model.DisplayFilter = false;
        }
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is AddModPackControlModel model)
        {
            model.Wheel(e.Delta.Y);
        }
    }

    public void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            (DataContext as AddModPackControlModel)!.Reload1();
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
                if ((DataContext as AddModPackControlModel)!.Display)
                {
                    App.CrossFade300.Start(null, ModPackFiles);
                }
                else
                {
                    App.CrossFade300.Start(ModPackFiles, null);
                }
            });
        }
        else if (e.PropertyName == "WindowClose")
        {
            Window.Close();
        }
    }

    private async void ModPackFiles_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            await (DataContext as AddModPackControlModel)!.Download();
            e.Handled = true;
        }
    }

    private void Input1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            (DataContext as AddModPackControlModel)!.Reload();
        }
    }

    private async void PackFiles_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        await (DataContext as AddModPackControlModel)!.Download();
    }

    public void Closed()
    {
        App.AddModPackWindow = null;
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        (DataContext as AddModPackControlModel)!.Source = 0;
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new AddModPackControlModel(model);
        amodel.PropertyChanged += Model_PropertyChanged;
        DataContext = amodel;
    }
}
