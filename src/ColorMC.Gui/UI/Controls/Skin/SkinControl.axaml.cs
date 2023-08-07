using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model.Skin;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.Skin;

public partial class SkinControl : UserControl, IUserControl
{
    private readonly SkinModel _model;

    private FpsTimer _renderTimer;

    private float _xdiff = 0;
    private float _ydiff = 0;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title => App.GetLanguage("SkinWindow.Title");

    public SkinControl()
    {
        InitializeComponent();

        _model = new(this);
        DataContext = _model;

        Skin.SetModel(_model);

        Button2.Click += Button2_Click;

        App.SkinLoad += App_SkinLoad;

        SkinTop.PointerMoved += SkinTop_PointerMoved;
        SkinTop.PointerPressed += SkinTop_PointerPressed;
        SkinTop.PointerWheelChanged += SkinTop_PointerWheelChanged;
    }

    public async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            await _model.Load();
        }
    }

    private void SkinTop_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.Y > 0)
        {
            Skin.AddDis(0.05f);
        }
        else
        {
            Skin.AddDis(-0.05f);
        }
    }

    private void SkinTop_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pro = e.GetCurrentPoint(this);
        _xdiff = (float)pro.Position.X;
        _ydiff = -(float)pro.Position.Y;
    }

    private void SkinTop_PointerMoved(object? sender, PointerEventArgs e)
    {
        var pro = e.GetCurrentPoint(this);
        if (pro.Properties.IsLeftButtonPressed)
        {
            float y = (float)pro.Position.X - _xdiff;
            float x = (float)pro.Position.Y + _ydiff;

            _xdiff = (float)pro.Position.X;
            _ydiff = -(float)pro.Position.Y;

            Skin.Rot(x, y);
        }
        else if (pro.Properties.IsRightButtonPressed)
        {
            float x = (float)pro.Position.X - _xdiff;
            float y = (float)pro.Position.Y + _ydiff;

            _xdiff = (float)pro.Position.X;
            _ydiff = -(float)pro.Position.Y;

            Skin.Pos(x / ((float)Bounds.Width / 8), -y / ((float)Bounds.Height / 8));
        }
    }

    public void Opened()
    {
        Window.SetTitle(Title);
        _renderTimer = new(Skin)
        {
            FpsTick = (fps) =>
            {
                Dispatcher.UIThread.Post(() => _model.Fps = fps);
            }
        };
    }

    public void Update()
    {
        if (_model.IsLoad)
        {
            Skin.RequestNextFrameRendering();
        }
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
}
