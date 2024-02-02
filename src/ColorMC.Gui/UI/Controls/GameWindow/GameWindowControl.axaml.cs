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

namespace ColorMC.Gui.UI.Controls.GameWindow;

public partial class GameWindowControl : UserControl, IUserControl
{
    internal class WindowControlHandle(INative native, IntPtr handle)
    : PlatformHandle(handle, "HWND"), INativeControlHostDestroyableControlHandle
    {
        public void Destroy()
        {
            native.DestroyWindow();
        }
    }

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title { get; set; }

    public string UseName { get; set; }

    private readonly IntPtr _handle;
    private readonly INative _implementation;
    private readonly GameSettingObj _obj;
    private readonly TopView _control;
    private readonly Process _process;

    private readonly short[] _lastAxisMax = new short[10];
    private readonly Dictionary<InputKeyObj, bool> _lastKeyState = [];
    private readonly string _name;

    private BaseModel _model;
    private double _cursorX, _cursorY, _cursorNowX, _cursorNowY;
    private IntPtr _gameController;
    private Point _lastPoint;
    private int _joystickID, _controlIndex;
    private string? _configUUID;
    private bool _isClose, _isExit, _isMouseMode, _oldMouseMode, _isEdit, _cursorHidden;

    private const int DownRate = 800;

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
        _control = new TopView(new WindowControlHandle(_implementation, handel));
        Panel1.Children.Add(_control);

        _controlIndex = 0;
        _configUUID = GuiConfigUtils.Config.Input.NowConfig;
    }

    public void Closed()
    {
        App.GameWindows.Remove(_obj.UUID);
    }

    private void Process_Exited(object? sender, EventArgs e)
    {
        _isClose = true;
        Window?.Close();
    }

    private void Event(Event sdlEvent)
    {
        if (_isEdit || !GuiConfigUtils.Config.Input.Enable
            || sdlEvent.Cbutton.Which != _joystickID
            || string.IsNullOrWhiteSpace(_configUUID)
            || !InputConfigUtils.Configs.TryGetValue(_configUUID, out var config))
        {
            return;
        }

        var type = (EventType)sdlEvent.Type;
        if (type == EventType.Controlleraxismotion)
        {
            var axisEvent = sdlEvent.Caxis;
            var axisValue = axisEvent.Value;

            var axis = (GameControllerAxis)axisEvent.Axis;

            var axisFixValue = (float)axisValue / DownRate * (_isMouseMode ? config.CursorRate : config.RotateRate);
            var deathSize = _isMouseMode ? config.CursorDeath : config.RotateDeath;
            var check = _isMouseMode ? config.CursorAxis : config.RotateAxis;
            //左摇杆
            if (check == 0)
            {
                if (axis == GameControllerAxis.Leftx)
                {
                    if (axisValue >= deathSize || axisValue <= -deathSize)
                    {
                        _cursorNowX = axisFixValue;
                    }
                    else
                    {
                        _cursorNowX = 0;
                    }
                }
                else if (axis == GameControllerAxis.Lefty)
                {
                    if (axisValue >= deathSize || axisValue <= -deathSize)
                    {
                        _cursorNowY = axisFixValue;
                    }
                    else
                    {
                        _cursorNowY = 0;
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
                        _cursorNowX = axisFixValue;
                    }
                    else
                    {
                        _cursorNowX = 0;
                    }
                }
                else if (axis == GameControllerAxis.Righty)
                {
                    if (axisValue >= deathSize || axisValue <= -deathSize)
                    {
                        _cursorNowY = axisFixValue;
                    }
                    else
                    {
                        _cursorNowY = 0;
                    }
                }
            }

            bool skip = false;
            if (_isMouseMode)
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
                    _lastAxisMax[axisEvent.Axis] = 0;
                }
                else if (_lastAxisMax[axisEvent.Axis] < axisValue)
                {
                    _lastAxisMax[axisEvent.Axis] = axisValue;
                }

                var nowMaxValue = _lastAxisMax[axisEvent.Axis];

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
        if (_isMouseMode)
        {
            _implementation.TransferTop();
            Thread.Sleep(50);
            if (_isMouseMode)
            {
                _implementation.NoTranferTop();
                _implementation.SendMouse(_cursorX, _cursorY, true);
            }
        }
    }

    public void Opened()
    {
        unsafe
        {
            _gameController = new(InputControlUtils.Open(_controlIndex));
        }

        if (_gameController != IntPtr.Zero)
        {
            InputControlUtils.OnEvent += Event;
            _joystickID = InputControlUtils.GetJoystickID(_gameController);
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

        _cursorX = width / 2;
        _cursorY = height / 2;
        _control.SendMouse(_cursorX, _cursorY);
        _control.TopWindow.PointerMoved += OnPointerMoved;
        _control.TopWindow.PointerExited += TopWindow_PointerExited;
        _control.TopWindow.PointerEntered += TopWindow_PointerEntered;

        new Thread(() =>
        {
            while (!_isExit)
            {
                _isMouseMode = _implementation.GetMouseMode();
                if (_isMouseMode != _oldMouseMode)
                {
                    _oldMouseMode = _isMouseMode;
                    _control.ChangeCursorDisplay(_isMouseMode);
                    if (_isMouseMode)
                    {
                        var size = _control.Bounds;

                        if (size.Width != 0 && size.Height != 0)
                        {
                            _cursorX = size.Width / 2;
                            _cursorY = size.Height / 2;
                            _control.SendMouse(_cursorX, _cursorY);
                        }
                    }
                }

                if (_cursorNowX != 0 || _cursorNowY != 0)
                {
                    if (_isMouseMode)
                    {
                        HideCursor();
                        var size = _control.Bounds;

                        _cursorX += _cursorNowX;
                        _cursorY += _cursorNowY;

                        if (_cursorX < 0)
                        {
                            _cursorX = 0;
                        }
                        else if (_cursorX > size.Width)
                        {
                            _cursorX = size.Width;
                        }

                        if (_cursorY < 0)
                        {
                            _cursorY = 0;
                        }
                        else if (_cursorY > size.Height)
                        {
                            _cursorY = size.Height;
                        }

                        _control.SendMouse(_cursorX, _cursorY);
                        _implementation.SendMouse(_cursorX, _cursorY, true);
                    }
                    else
                    {
                        _implementation.SendMouse(_cursorNowX, _cursorNowY, false);
                    }
                }

                Thread.Sleep(10);
            }
        }).Start();
    }

    private void TopWindow_PointerEntered(object? sender, PointerEventArgs e)
    {
        _lastPoint = e.GetPosition(_control.TopWindow);
    }

    private void TopWindow_PointerExited(object? sender, PointerEventArgs e)
    {
        _lastPoint = e.GetPosition(_control.TopWindow);
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
        if (pos.NearlyEquals(_lastPoint))
        {
            return;
        }
        _lastPoint = pos;
        if (_cursorHidden)
        {
            ShowCursor();
        }
    }

    private void CheckKeyAndSend(InputKeyObj obj, bool down)
    {
        if (_lastKeyState.TryGetValue(obj, out var state))
        {
            if (state != down)
            {
                _lastKeyState[obj] = down;
                _implementation.SendKey(obj, down, !_cursorHidden);
            }
        }
        else
        {
            _lastKeyState.Add(obj, down);
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
        _isEdit = false;
    }

    private void SelectConfig()
    {
        if (_isEdit)
        {
            return;
        }
        _isEdit = true;
        RemoveChoise();
        _control.Edit(true);
        _implementation.NoTranferTop();
        var model = new ControlSelectModel(_controlIndex, _configUUID, (model) =>
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
        if (_gameController != IntPtr.Zero)
        {
            InputControlUtils.Close(_gameController);
            _gameController = IntPtr.Zero;
            _joystickID = 0;
        }

        _controlIndex = model.ControlIndex;
        _configUUID = model.GetUUID();

        unsafe
        {
            _gameController = new(InputControlUtils.Open(_controlIndex));
        }

        if (_gameController != IntPtr.Zero)
        {
            _joystickID = InputControlUtils.GetJoystickID(_gameController);
        }
    }

    public Task<bool> Closing()
    {
        _isExit = true;

        InputControlUtils.OnEvent -= Event;

        _implementation.Stop();
        _implementation.Close();

        Task.Run(() =>
        {
            Thread.Sleep(2000);
            try
            {
                if (_isClose == false && !_process.HasExited)
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