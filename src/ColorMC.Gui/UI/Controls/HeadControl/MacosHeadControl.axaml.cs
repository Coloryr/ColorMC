using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.UI.Controls.HeadControl;

public partial class MacosHeadControl : UserControl
{
    /// <summary>
    /// ²Ëµ¥°´Å¥
    /// </summary>
    private class ButtonBack
    {
        private readonly IBrush _color;
        private readonly Button _button;

        public ButtonBack(Button button, IBrush basecolor)
        {
            _color = basecolor;
            _button = button;

            button.Background = _color;
        }

        public void SetColor(bool hide)
        {
            if (hide)
            {
                _button.Background = ThemeManager.GetColor(nameof(ThemeObj.ProgressBarBG));
            }
            else
            {
                _button.Background = _color;
            }
        }
    }

    private bool _isLost;

    public MacosHeadControl()
    {
        InitializeComponent();

        var select1 = new ButtonBack(ButtonMin, Brush.Parse("#febb2c"));
        ButtonMin.PointerEntered += (a, b) => { Img1.IsVisible = true; };
        ButtonMin.PointerExited += (a, b) => { Img1.IsVisible = false; };

        var select2 = new ButtonBack(ButtonMax, Brush.Parse("#29c73f"));
        ButtonMax.PointerEntered += (a, b) => { Img2.IsVisible = true; };
        ButtonMax.PointerExited += (a, b) => { Img2.IsVisible = false; };

        var select3 = new ButtonBack(ButtonClose, Brush.Parse("#fe5f59"));
        ButtonClose.PointerEntered += (a, b) => { Img3.IsVisible = true; };
        ButtonClose.PointerExited += (a, b) => { Img3.IsVisible = false; };

        PointerEntered += (a, b) =>
        {
            if (_isLost)
            {
                select1.SetColor(false);
                select2.SetColor(false);
                select3.SetColor(false);
            }
        };
        PointerExited += (a, b) =>
        {
            if (_isLost)
            {
                select1.SetColor(true);
                select2.SetColor(true);
                select3.SetColor(true);
            }
        };

        Dispatcher.UIThread.Post(() =>
        {
            if (VisualRoot is Window window)
            {
                window.Activated += (a, b) =>
                {
                    _isLost = false;

                    select1.SetColor(false);
                    select2.SetColor(false);
                    select3.SetColor(false);
                };
                window.Deactivated += (a, b) =>
                {
                    _isLost = true;

                    select1.SetColor(true);
                    select2.SetColor(true);
                    select3.SetColor(true);
                };
            }
        });
    }
}