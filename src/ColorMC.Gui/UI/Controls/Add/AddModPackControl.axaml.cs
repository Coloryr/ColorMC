using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddModPackControl : BaseUserControl
{
    public AddModPackControl()
    {
        InitializeComponent();

        Title = App.Lang("AddModPackWindow.Title");
        UseName = ToString() ?? "AddModPackControl";

        PackFiles.DoubleTapped += PackFiles_DoubleTapped;

        ModPackFiles.PointerPressed += ModPackFiles_PointerPressed;

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
        ScrollViewer1.ScrollChanged += ScrollViewer1_ScrollChanged;
    }

    public override Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            (DataContext as AddModPackControlModel)!.Reload1();

            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public override void Closed()
    {
        WindowManager.AddModPackWindow = null;
    }

    public override void Opened()
    {
        Window.SetTitle(Title);

        (DataContext as AddModPackControlModel)!.Source = 0;
    }

    public override void SetModel(BaseModel model)
    {
        var amodel = new AddModPackControlModel(model);
        amodel.PropertyChanged += Model_PropertyChanged;
        DataContext = amodel;
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }

    private void ScrollViewer1_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (e.ExtentDelta == e.OffsetDelta || e.ExtentDelta.Y < 0)
        {
            return;
        }
        if (DataContext is AddModPackControlModel model)
        {
            if (model.EmptyDisplay)
            {
                return;
            }
        }
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is AddModPackControlModel model)
        {
            model.Wheel(e.Delta.Y);
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

    private async void PackFiles_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        await (DataContext as AddModPackControlModel)!.Download();
    }
}
