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

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddModPackControl : BaseUserControl
{
    public AddModPackControl()
    {
        InitializeComponent();

        Title = App.Lang("AddModPackWindow.Title");
        UseName = ToString() ?? "AddModPackControl";

        ModPackFiles.PointerPressed += ModPackFiles_PointerPressed;
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

    public override TopModel GenModel(BaseModel model)
    {
        var amodel = new AddModPackControlModel(model);
        amodel.PropertyChanged += Model_PropertyChanged;
        return amodel;
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
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
                    App.CrossFade300.Start(ScrollViewer1, null);
                }
                else
                {
                    App.CrossFade300.Start(ModPackFiles, null);
                    App.CrossFade300.Start(null, ScrollViewer1);
                }
            });
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
}
