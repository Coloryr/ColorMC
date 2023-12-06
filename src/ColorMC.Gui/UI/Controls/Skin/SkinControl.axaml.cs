using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Skin;
using ColorMC.Gui.UI.Windows;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Controls.Skin;

public partial class SkinControl : UserControl, IUserControl
{
    private FpsTimer _renderTimer;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => App.Lang("SkinWindow.Title");

    public string UseName { get; }

    public SkinControl()
    {
        InitializeComponent();

        UseName = ToString() ?? "SkinControl";

        Button2.Click += Button2_Click;

        App.SkinLoad += App_SkinLoad;
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

    public async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            await (DataContext as SkinModel)!.Load();
        }
    }

    public void WindowStateChange(WindowState state)
    {
        _renderTimer.Pause = state != WindowState.Minimized;
    }

    public void Opened()
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

    public void Closed()
    {
        App.SkinLoad -= App_SkinLoad;
        _renderTimer.Close();

        App.SkinWindow = null;
    }

    private void App_SkinLoad()
    {
        Skin.ChangeSkin();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        Skin.Reset();
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new SkinModel(model);
        amodel.PropertyChanged += Model_PropertyChanged;
        DataContext = amodel;
    }
}
