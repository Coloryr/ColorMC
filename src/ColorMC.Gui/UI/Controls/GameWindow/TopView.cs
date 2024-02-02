using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ColorMC.Gui.UI.Views.Svg;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace ColorMC.Gui.UI.Controls.GameWindow;

internal class TopView : NativeControlHost
{
    private readonly IPlatformHandle _input;

    public IPlatformHandle hndl;

    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<TopView>();

    public Window TopWindow;
    private CompositeDisposable? _disposables;
    private bool _isAttached;
    private IDisposable _isEffectivelyVisible;

    private readonly TranslateTransform transform = new();
    private readonly SvgControl svg;

    public TopView(IPlatformHandle input)
    {
        _input = input;

        svg = new(baseUri: null)
        {
            Width = 30,
            Height = 30,
            RenderTransform = transform,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left,
            Path = "avares://ColorMC.Gui/Resource/Icon/Input/cursor.svg"
        };

        Content = svg;

        ContentProperty.Changed.AddClassHandler<TopView>((s, e) => s.InitializeNativeOverlay());
        IsVisibleProperty.Changed.AddClassHandler<TopView>((s, e) => s.ShowNativeOverlay(s.IsVisible));
    }

    public void ChangeCursorDisplay(bool enable)
    {
        Dispatcher.UIThread.Post(() =>
        {
            svg.IsVisible = enable;
        });
    }

    public void Edit(bool enable)
    {
        svg.IsVisible = !enable;
        if (!enable)
        {
            TopWindow.Content = svg;
        }
        else
        {
            TopWindow.Content = null;
        }
    }

    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    private void InitializeNativeOverlay()
    {
        if (!this.IsAttachedToVisualTree())
        {
            return;
        }

        if (TopWindow == null && Content != null)
        {
            var rect = Bounds;

            TopWindow = new Window()
            {
                SystemDecorations = SystemDecorations.None,
                TransparencyLevelHint = [WindowTransparencyLevel.Transparent],
                Background = Brushes.Transparent,
                SizeToContent = SizeToContent.WidthAndHeight,
                CanResize = false,
                ShowInTaskbar = false,
                ZIndex = int.MaxValue,
                Opacity = 1
            };

            _disposables =
            [
                TopWindow.Bind(ContentControl.ContentProperty, this.GetObservable(ContentProperty)),
                this.GetObservable(ContentProperty).Skip(1)
                    .Subscribe(_=> UpdateOverlayPosition()),
                this.GetObservable(BoundsProperty).Skip(1)
                    .Subscribe(_ => UpdateOverlayPosition()),
                Observable.FromEventPattern(VisualRoot!, nameof(Window.PositionChanged))
                    .Subscribe(_ => UpdateOverlayPosition())
            ];
        }

        ShowNativeOverlay(IsEffectivelyVisible);
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        return _input;
    }

    private void ShowNativeOverlay(bool show)
    {
        if (TopWindow == null || TopWindow.IsVisible == show)
            return;

        if (VisualRoot is Window window)
        {
            if (show && _isAttached)
                TopWindow.Show(window);
            else
                TopWindow.Hide();
        }
    }

    public void SendMouse(double cursorX, double cursorY)
    {
        Dispatcher.UIThread.Post(() =>
        {
            transform.X = cursorX - 15;
            transform.Y = cursorY - 15;
        });
    }

    private void UpdateOverlayPosition()
    {
        if (TopWindow == null) return;
        bool forceSetWidth = false, forceSetHeight = false;
        var topLeft = new Point();
        var child = TopWindow.Presenter?.Child;
        if (child?.IsArrangeValid == true)
        {
            switch (child.HorizontalAlignment)
            {
                case HorizontalAlignment.Right:
                    topLeft = topLeft.WithX(Bounds.Width - TopWindow.Bounds.Width);
                    break;

                case HorizontalAlignment.Center:
                    topLeft = topLeft.WithX((Bounds.Width - TopWindow.Bounds.Width) / 2);
                    break;

                case HorizontalAlignment.Stretch:
                    forceSetWidth = true;
                    break;
            }

            switch (child.VerticalAlignment)
            {
                case VerticalAlignment.Bottom:
                    topLeft = topLeft.WithY(Bounds.Height - TopWindow.Bounds.Height);
                    break;

                case VerticalAlignment.Center:
                    topLeft = topLeft.WithY((Bounds.Height - TopWindow.Bounds.Height) / 2);
                    break;

                case VerticalAlignment.Stretch:
                    forceSetHeight = true;
                    break;
            }
        }

        if (forceSetWidth && forceSetHeight)
            TopWindow.SizeToContent = SizeToContent.Manual;
        else if (forceSetHeight)
            TopWindow.SizeToContent = SizeToContent.Width;
        else if (forceSetWidth)
            TopWindow.SizeToContent = SizeToContent.Height;
        else
            TopWindow.SizeToContent = SizeToContent.Manual;

        TopWindow.Width = forceSetWidth ? Bounds.Width : double.NaN;
        TopWindow.Height = forceSetHeight ? Bounds.Height : double.NaN;

        TopWindow.MaxWidth = Bounds.Width;
        TopWindow.MaxHeight = Bounds.Height;

        var newPosition = this.PointToScreen(topLeft);

        if (newPosition != TopWindow.Position)
        {
            TopWindow.Position = newPosition;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        InitializeNativeOverlay();
        _isEffectivelyVisible = this.GetVisualAncestors().OfType<Control>()
                .Select(v => v.GetObservable(IsVisibleProperty))
                .CombineLatest(v => !v.Any(o => !o))
                .DistinctUntilChanged()
                .Subscribe(v => IsVisible = v);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _isEffectivelyVisible?.Dispose();
        ShowNativeOverlay(false);
        _isAttached = false;
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);

        _disposables?.Dispose();
        _disposables = null;
        TopWindow.Close();
    }
}
