using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Skin;

namespace ColorMC.Gui.UI.Controls.Skin;

public partial class SkinControl : BaseUserControl
{
    private FpsTimer _renderTimer;

    public SkinControl()
    {
        InitializeComponent();

        Title = App.Lang("SkinWindow.Title");
        UseName = ToString() ?? "SkinControl";

        Button2.Click += Button2_Click;

        App.SkinLoad += App_SkinLoad;
    }

    public override async Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            await (DataContext as SkinModel)!.Load();

            return true;
        }

        return false;
    }

    public override void WindowStateChange(WindowState state)
    {
        _renderTimer.Pause = state != WindowState.Minimized;
    }

    public override void Opened()
    {
        Window.SetTitle(Title);
        _renderTimer = new(Skin)
        {
            FpsTick = (fps) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var model = (DataContext as SkinModel)!;
                    model.Fps = fps;
                });
            }
        };
    }

    public override void Closed()
    {
        App.SkinLoad -= App_SkinLoad;
        _renderTimer.Close();

        WindowManager.SkinWindow = null;
    }

    public override void SetModel(BaseModel model)
    {
        var amodel = new SkinModel(model);
        amodel.PropertyChanged += Model_PropertyChanged;
        DataContext = amodel;
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "SkinLoadDone")
        {
            if ((DataContext as SkinModel)!.HaveSkin)
            {
                _renderTimer.Pause = false;
            }
            else
            {
                _renderTimer.Pause = true;
            }
        }
    }

    private void App_SkinLoad()
    {
        Skin.ChangeSkin();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        Skin.Reset();
    }
}
