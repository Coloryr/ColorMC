using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.UI.Controls.HeadControl;

public partial class MacosHeadControl : UserControl
{
    /// <summary>
    /// 菜单按钮
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
            _button.Background = hide ? ThemeManager.GetColor(nameof(ThemeObj.ProgressBarBG)) : _color;
        }
    }

    private bool _isLost;

    public MacosHeadControl()
    {
        InitializeComponent();
        
        Buttons.PointerEntered += ButtonMinOnPointerEntered;
        Buttons.PointerExited += ButtonMinOnPointerExited;

        var select1 = new ButtonBack(ButtonMin, Brush.Parse("#febb2c"));
        var select2 = new ButtonBack(ButtonMax, Brush.Parse("#29c73f"));
        var select3 = new ButtonBack(ButtonClose, Brush.Parse("#fe5f59"));

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

    private void ButtonMinOnPointerExited(object? sender, PointerEventArgs e)
    {
        Image(false);
    }

    private void ButtonMinOnPointerEntered(object? sender, PointerEventArgs e)
    {
        Image(true);
    }

    private void Image(bool show)
    {
        Img1.IsVisible = Img2.IsVisible = Img3.IsVisible = show;
    }
}