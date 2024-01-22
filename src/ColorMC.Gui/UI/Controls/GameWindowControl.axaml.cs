using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Layout;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Windows;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ColorMC.Gui.UI.Views.Svg;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Linq;
using ColorMC.Gui.Utils.Hook;

namespace ColorMC.Gui.UI.Controls;

public partial class GameWindowControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title { get; set; }

    public string UseName { get; set; }

    private readonly IntPtr _handle;
    private readonly INative _implementation;
    private readonly GameSettingObj _obj;
    private TopView _control;

    private double cursorX;
    private double cursorY;

    public GameWindowControl()
    {
        InitializeComponent();
    }

    public GameWindowControl(GameSettingObj obj, Process process, IntPtr handel) : this()
    {
        _obj = obj;
        _handle = handel;

        process.Exited += Process_Exited;

        if (SystemInfo.Os == OsType.Windows)
        {
            _implementation = new Win32Native();
        }
    }

    public void Closed() 
    {
        App.GameWindows.Remove(_obj.UUID);
    }

    private void Process_Exited(object? sender, EventArgs e)
    {
        Window?.Close();
    }

    public void Opened()
    {
        if (_implementation.GetWindowSize(_handle, out var width, out var height))
        {
            Window.SetSize(width, height);
        }
        _implementation.TitleChange += TitleChange;
        Window.SetTitle(_implementation.GetWindowTitle(_handle));
        if (_implementation.GetIcon(_handle) is { } icon)
        {
            Window.SetIcon(icon);
        }
        var handle = _implementation.CreateControl(_handle);
        _control = new TopView(handle);
        Panel1.Children.Add(_control);
        var handle1 = _control.TopWindow.TryGetPlatformHandle();
        if (handle1 is { })
        {
            _implementation.TransferEvent(handle1.Handle);
        }
    }

    private void TitleChange(string title)
    {
        Window.SetTitle(title);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
    }

    public void WindowStateChange(WindowState state) 
    {
        _implementation.SetWindowState(_handle, state);
    }

    public void SetBaseModel(BaseModel model)
    {
        
    }

    public class EmbedControl(IPlatformHandle handle) : NativeControlHost
    {
        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            return handle ?? base.CreateNativeControlCore(parent);
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            if (control is INativeControlHostDestroyableControlHandle)
            {
                base.DestroyNativeControlCore(control);
            }
        }
    }

    public Task<bool> Closing()
    {
        _implementation.Close(_handle);

        return Task.FromResult(false);
    }
}

public class TopView : NativeControlHost
{
    private readonly IPlatformHandle _input;

    public IPlatformHandle hndl;

    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<TopView>();

    public Window TopWindow;
    private IDisposable _disposables;
    private bool _isAttached;
    private IDisposable _isEffectivelyVisible;

    private readonly TranslateTransform transform = new();

    private readonly SvgControl svg;

    private void GameWindowControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        var pos = e.GetPosition(this);
        //cursorX = pos.X;
        //cursorY = pos.Y;

        transform.X = pos.X - 15;
        transform.Y = pos.Y - 15;
    }

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

    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    private void InitializeNativeOverlay()
    {
        if (!this.IsAttachedToVisualTree()) return;

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
                Opacity = 1,
            };

            TopWindow.PointerMoved += GameWindowControl_PointerMoved;

            _disposables = new CompositeDisposable()
                {
                    TopWindow.Bind(ContentControl.ContentProperty, this.GetObservable(ContentProperty)),
                    this.GetObservable(ContentProperty).Skip(1).Subscribe(_=> UpdateOverlayPosition()),
                    this.GetObservable(BoundsProperty).Skip(1).Subscribe(_ => UpdateOverlayPosition()),
                    Observable.FromEventPattern(VisualRoot, nameof(Window.PositionChanged))
                    .Subscribe(_ => UpdateOverlayPosition())
                };


        }

        ShowNativeOverlay(IsEffectivelyVisible);
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        return _input;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        base.DestroyNativeControlCore(control);
    }

    private void ShowNativeOverlay(bool show)
    {
        if (TopWindow == null || TopWindow.IsVisible == show)
            return;

        if (show && _isAttached)
            TopWindow.Show(VisualRoot as Window);
        else
            TopWindow.Hide();
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
        TopWindow?.Close();
        TopWindow = null;
    }
}

internal class Win32WindowControlHandle(IntPtr handle) 
    : PlatformHandle(handle, "HWND"), INativeControlHostDestroyableControlHandle
{
    public void Destroy()
    {
        Win32.DestroyWindow(Handle);
    }
}