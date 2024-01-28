using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Views.Svg;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.Hook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Event = Silk.NET.SDL.Event;
using EventType = Silk.NET.SDL.EventType;
using GameControllerAxis = Silk.NET.SDL.GameControllerAxis;

namespace ColorMC.Gui.UI.Controls;

public partial class GameWindowControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title { get; set; }

    public string UseName { get; set; }

    private readonly IntPtr _handle;
    private readonly INative _implementation;
    private readonly GameSettingObj _obj;
    private readonly TopView _control;
    private readonly Process _process;

    private bool isClose;
    private bool isExit;

    private double cursorX;
    private double cursorY;

    private double cursorNowX;
    private double cursorNowY;

    private IntPtr gameController;
    private Point point;
    private int joystickID;
    private int controlIndex;
    private string? configUUID;

    private bool isMouseMode;
    private bool oldMouseMode;
    private bool isEdit;
    private bool _cursorHidden = false;

    private readonly short[] lastAxisMax = new short[10];
    private readonly Dictionary<InputKeyObj, bool> lastKeyState = [];

    private readonly int DownRate = 800;

    private readonly string _name;

    public GameWindowControl()
    {
        InitializeComponent();

        _name = ToString() ?? "GameWindowControl";
    }

    public GameWindowControl(GameSettingObj obj, Process process, IntPtr handel) : this()
    {
        _obj = obj;
        _handle = handel;
        _process = process;

        process.Exited += Process_Exited;

        if (SystemInfo.Os == OsType.Windows)
        {
            _implementation = new Win32Native();
        }

        _implementation.AddHook(process, _handle);
        _implementation.NoBorder();
        var chandle = new WindowControlHandle(_implementation, handel);
        _control = new TopView(chandle);
        Panel1.Children.Add(_control);

        controlIndex = 0;
        configUUID = GuiConfigUtils.Config.Input.NowConfig;
    }

    public void Closed()
    {
        App.GameWindows.Remove(_obj.UUID);
    }

    private void Process_Exited(object? sender, EventArgs e)
    {
        isClose = true;
        Window?.Close();
    }

    private void Event(Event sdlEvent)
    {
        if (isEdit || !GuiConfigUtils.Config.Input.Enable
            || sdlEvent.Cbutton.Which != joystickID
            || string.IsNullOrWhiteSpace(configUUID)
            || !InputConfigUtils.Configs.TryGetValue(configUUID, out var config))
        {
            return;
        }

        var type = (EventType)sdlEvent.Type;
        if (type == EventType.Controlleraxismotion)
        {
            var axisEvent = sdlEvent.Caxis;
            var axisValue = axisEvent.Value;

            var axis = (GameControllerAxis)axisEvent.Axis;

            var axisFixValue = (float)axisValue / DownRate * (isMouseMode ? config.CursorRate : config.RotateRate);
            var deathSize = isMouseMode ? config.CursorDeath : config.RotateDeath;
            var check = isMouseMode ? config.CursorAxis : config.RotateAxis;
            //左摇杆
            if (check == 0)
            {
                if (axis == GameControllerAxis.Leftx)
                {
                    if (axisValue >= deathSize || axisValue <= -deathSize)
                    {
                        cursorNowX = axisFixValue;
                    }
                    else
                    {
                        cursorNowX = 0;
                    }
                }
                else if (axis == GameControllerAxis.Lefty)
                {
                    if (axisValue >= deathSize || axisValue <= -deathSize)
                    {
                        cursorNowY = axisFixValue;
                    }
                    else
                    {
                        cursorNowY = 0;
                    }
                }
            }
            //右摇杆
            else if (check == 1)
            {
                if (axis == GameControllerAxis.Rightx)
                {
                    if (axisValue >= deathSize || axisValue <= -deathSize)
                    {
                        cursorNowX = axisFixValue;
                    }
                    else
                    {
                        cursorNowX = 0;
                    }
                }
                else if (axis == GameControllerAxis.Righty)
                {
                    if (axisValue >= deathSize || axisValue <= -deathSize)
                    {
                        cursorNowY = axisFixValue;
                    }
                    else
                    {
                        cursorNowY = 0;
                    }
                }
            }

            bool skip = false;
            if (isMouseMode)
            {
                if (check == 0 && axis is GameControllerAxis.Leftx
                        or GameControllerAxis.Lefty)
                {
                    skip = true;
                }
                else if (check == 1 && axis is GameControllerAxis.Rightx
                        or GameControllerAxis.Righty)
                {
                    skip = true;
                }
            }

            if (!skip)
            {
                if (axisValue < config.ToBackValue)
                {
                    lastAxisMax[axisEvent.Axis] = 0;
                }
                else if (lastAxisMax[axisEvent.Axis] < axisValue)
                {
                    lastAxisMax[axisEvent.Axis] = axisValue;
                }

                var nowMaxValue = lastAxisMax[axisEvent.Axis];

                foreach (var item in config.AxisKeys.Values)
                {
                    //光标模式跳过光标摇杆
                    if (item.InputKey == axisEvent.Axis)
                    {
                        bool down;
                        if (!item.BackCancel)
                        {
                            if (axisValue <= 0 && item.Start <= 0 && item.End <= 0)
                            {
                                down = item.Start >= axisValue && item.End <= axisValue;
                            }
                            else
                            {
                                down = item.Start <= axisValue && item.End >= axisValue;
                            }
                        }
                        else
                        {
                            if (axisValue <= 0 && item.Start <= 0 && item.End <= 0)
                            {
                                down = item.Start >= nowMaxValue && item.End <= nowMaxValue;
                            }
                            else
                            {
                                down = item.Start <= nowMaxValue && item.End >= nowMaxValue;
                            }
                        }

                        CheckKeyAndSend(item, down);
                    }
                }
            }
        }
        else if (type == EventType.Controllerbuttondown)
        {
            var button = sdlEvent.Cbutton.Button;
            foreach (var item in config.Keys)
            {
                if (item.Key == button)
                {
                    CheckKeyAndSend(item.Value, true);
                }
            }
            if (config.ItemCycle)
            {
                if (button == config.ItemCycleLeft)
                {
                    _implementation.SendScoll(true);
                }
                else if (button == config.ItemCycleRight)
                {
                    _implementation.SendScoll(false);
                }
            }
        }
        else if (type == EventType.Controllerbuttonup)
        {
            var button = sdlEvent.Cbutton.Button;
            foreach (var item in config.Keys)
            {
                if (item.Key == button)
                {
                    CheckKeyAndSend(item.Value, false);
                }
            }
        }
    }

    private void PostTestMouse()
    {
        if (isMouseMode)
        {
            _implementation.TransferTop();
            Thread.Sleep(50);
            if (isMouseMode)
            {
                _implementation.NoTranferTop();
                _implementation.SendMouse(cursorX, cursorY, true);
            }
        }
    }

    public void Opened()
    {
        unsafe
        {
            gameController = new(InputControlUtils.Open(controlIndex));
        }

        if (gameController != IntPtr.Zero)
        {
            InputControlUtils.OnEvent += Event;
            joystickID = InputControlUtils.GetJoystickID(gameController);
        }

        _implementation.TitleChange += TitleChange;

        if (_implementation.GetWindowSize(out var width, out var height))
        {
            Window.SetSize(width + 2, height + 31);
        }
        if (_implementation.GetIcon() is { } icon)
        {
            Window.SetIcon(icon);
        }

        Window.SetTitle(_implementation.GetWindowTitle());

        var handle1 = _control.TopWindow.TryGetPlatformHandle();

        if (handle1 is { })
        {
            _implementation.AddHookTop(handle1.Handle);
            _implementation.TransferTop();
        }

        cursorX = width / 2;
        cursorY = height / 2;
        _control.SendMouse(cursorX, cursorY);
        _control.TopWindow.PointerMoved += OnPointerMoved;
        _control.TopWindow.PointerExited += TopWindow_PointerExited;
        _control.TopWindow.PointerEntered += TopWindow_PointerEntered;

        new Thread(() =>
        {
            while (!isExit)
            {
                isMouseMode = _implementation.GetMouseMode();
                if (isMouseMode != oldMouseMode)
                {
                    oldMouseMode = isMouseMode;
                    _control.ChangeCursorDisplay(isMouseMode);
                    if (isMouseMode)
                    {
                        var size = _control.Bounds;

                        if (size.Width != 0 && size.Height != 0)
                        {
                            cursorX = size.Width / 2;
                            cursorY = size.Height / 2;
                            _control.SendMouse(cursorX, cursorY);
                        }
                    }
                }

                if (cursorNowX != 0 || cursorNowY != 0)
                {
                    if (isMouseMode)
                    {
                        HideCursor();
                        var size = _control.Bounds;

                        cursorX += cursorNowX;
                        cursorY += cursorNowY;

                        if (cursorX < 0)
                        {
                            cursorX = 0;
                        }
                        else if (cursorX > size.Width)
                        {
                            cursorX = size.Width;
                        }

                        if (cursorY < 0)
                        {
                            cursorY = 0;
                        }
                        else if (cursorY > size.Height)
                        {
                            cursorY = size.Height;
                        }

                        _control.SendMouse(cursorX, cursorY);
                        _implementation.SendMouse(cursorX, cursorY, true);
                    }
                    else
                    {
                        _implementation.SendMouse(cursorNowX, cursorNowY, false);
                    }
                }

                Thread.Sleep(10);
            }
        }).Start();
    }

    private void TopWindow_PointerEntered(object? sender, PointerEventArgs e)
    {
        point = e.GetPosition(_control.TopWindow);
    }

    private void TopWindow_PointerExited(object? sender, PointerEventArgs e)
    {
        point = e.GetPosition(_control.TopWindow);
    }

    private void HideCursor()
    {
        if (_cursorHidden == false)
        {
            _implementation.NoTranferTop();
            _cursorHidden = true;
        }
    }

    private void ShowCursor()
    {
        if (_cursorHidden)
        {
            _implementation.TransferTop();
            _cursorHidden = false;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var pos = e.GetPosition(_control.TopWindow);
        if (pos.NearlyEquals(point))
        {
            return;
        }
        point = pos;
        if (_cursorHidden)
        {
            ShowCursor();
        }
    }

    private void CheckKeyAndSend(InputKeyObj obj, bool down)
    {
        if (lastKeyState.TryGetValue(obj, out var state))
        {
            if (state != down)
            {
                lastKeyState[obj] = down;
                _implementation.SendKey(obj, down, !_cursorHidden);
            }
        }
        else
        {
            lastKeyState.Add(obj, down);
            _implementation.SendKey(obj, down, !_cursorHidden);
        }
        if (down)
        {
            PostTestMouse();
        }
    }

    private void TitleChange(string title)
    {
        Window?.SetTitle(title);
    }

    public void WindowStateChange(WindowState state)
    {
        _implementation.SetWindowState(state);
    }

    private BaseModel _model;

    public void SetBaseModel(BaseModel model)
    {
        _model = model;
        SetChoise();
    }

    private void SetChoise()
    {
        _model.SetChoiseCall(_name, SelectConfig);
        _model.SetChoiseContent(_name, App.Lang("GameWindow.Text1"));
    }

    private void RemoveChoise()
    {
        _model.RemoveChoiseData(_name);
    }

    private void EditClose()
    {
        _implementation.TransferTop();
        _control.Edit(false);
        SetChoise();
        isEdit = false;
    }

    private void SelectConfig()
    {
        if (isEdit)
        {
            return;
        }
        isEdit = true;
        RemoveChoise();
        _control.Edit(true);
        _implementation.NoTranferTop();
        var model = new ControlSelectModel(controlIndex, configUUID, (model) =>
        {
            ChangeConfig(model);
            EditClose();
        }, () => 
        {
            EditClose();
        });
        _control.TopWindow.Content = new ControlSelectControl()
        { 
            DataContext = model
        };
    }

    private void ChangeConfig(ControlSelectModel model)
    {
        if (gameController != IntPtr.Zero)
        {
            InputControlUtils.Close(gameController);
            gameController = IntPtr.Zero;
            joystickID = 0;
        }

        controlIndex = model.ControlIndex;
        configUUID = model.GetUUID();

        unsafe
        {
            gameController = new(InputControlUtils.Open(controlIndex));
        }

        if (gameController != IntPtr.Zero)
        {
            joystickID = InputControlUtils.GetJoystickID(gameController);
        }
    }
    
    public Task<bool> Closing()
    {
        isExit = true;

        InputControlUtils.OnEvent -= Event;

        _implementation.Stop();
        _implementation.Close();

        Task.Run(() =>
        {
            Thread.Sleep(2000);
            try
            {
                if (isClose == false && !_process.HasExited)
                {
                    _process.Kill();
                }
            }
            catch
            {

            }
        });

        return Task.FromResult(false);
    }
}

internal class WindowControlHandle(INative native, IntPtr handle)
    : PlatformHandle(handle, "HWND"), INativeControlHostDestroyableControlHandle
{
    public void Destroy()
    {
        native.DestroyWindow();
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
                Opacity = 1
            };

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

    private void ShowNativeOverlay(bool show)
    {
        if (TopWindow == null || TopWindow.IsVisible == show)
            return;

        if (show && _isAttached)
            TopWindow.Show(VisualRoot as Window);
        else
            TopWindow.Hide();
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
        TopWindow?.Close();
        TopWindow = null;
    }
}