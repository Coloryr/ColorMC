using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.Hook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Event = Silk.NET.SDL.Event;
using EventType = Silk.NET.SDL.EventType;
using GameControllerAxis = Silk.NET.SDL.GameControllerAxis;

namespace ColorMC.Gui.UI.Controls.GameWindow;

public partial class GameWindowControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title { get; set; }

    public string UseName { get; set; }

    public bool MouseMode { get; set; } = true;

    private readonly INative _implementation;
    private readonly GameSettingObj _obj;
    private readonly Process _process;

    private readonly short[] _lastAxisMax = new short[10];
    private readonly Dictionary<InputKeyObj, bool> _lastKeyState = [];

    private double _cursorNowX, _cursorNowY;
    private IntPtr _gameController;
    private int _joystickID, _controlIndex;
    private string? _configUUID;
    private bool _isMouseMode = true, _isExit, _isEdit;

    private const int DownRate = 800;

    public GameWindowControl()
    {
        InitializeComponent();
    }

    public GameWindowControl(GameSettingObj obj, Process process) : this()
    {
        _obj = obj;
        _process = process;

        process.Exited += Process_Exited;

        if (SystemInfo.Os == OsType.Windows)
        {
            _implementation = new Win32Native();
        }

        _implementation.AddHook(process);

        _controlIndex = 0;
        _configUUID = GuiConfigUtils.Config.Input.NowConfig;
    }

    public void Closed()
    {
        App.GameWindows.Remove(_obj.UUID);

        _implementation.Stop();
    }

    private void Process_Exited(object? sender, EventArgs e)
    {
        _isExit = true;
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
            var choiseAxis = _isMouseMode ? config.CursorAxis : config.RotateAxis;
            //左摇杆
            if (choiseAxis == 0)
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
            else if (choiseAxis == 1)
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
                if (choiseAxis == 0 && axis is GameControllerAxis.Leftx
                        or GameControllerAxis.Lefty)
                {
                    skip = true;
                }
                else if (choiseAxis == 1 && axis is GameControllerAxis.Rightx
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

    public void Opened()
    {
        unsafe
        {
            _gameController = new(InputControl.Open(_controlIndex));
        }

        if (_gameController != IntPtr.Zero)
        {
            InputControl.OnEvent += Event;
            _joystickID = InputControl.GetJoystickID(_gameController);
        }

        _implementation.GetWindowSize(out var width, out var height);

        new Thread(() =>
        {
            while (!_isExit)
            {
                _isMouseMode = MouseMode;

                if (_cursorNowX != 0 || _cursorNowY != 0)
                {
                    _implementation.SendMouse(_cursorNowX, _cursorNowY, false);
                }

                Thread.Sleep(10);
            }
        }).Start();
    }

    private void CheckKeyAndSend(InputKeyObj obj, bool down)
    {
        if (_lastKeyState.TryGetValue(obj, out var state))
        {
            if (state != down)
            {
                _lastKeyState[obj] = down;
                _implementation.SendKey(obj, down, _isMouseMode);
            }
        }
        else
        {
            _lastKeyState.Add(obj, down);
            _implementation.SendKey(obj, down, _isMouseMode);
        }
    }

    public void SetBaseModel(BaseModel model)
    {
        DataContext = new ControlSelectModel(_controlIndex, _configUUID, ChangeConfig);
    }

    private void ChangeConfig(ControlSelectModel model)
    {
        if (_gameController != IntPtr.Zero)
        {
            InputControl.Close(_gameController);
            _gameController = IntPtr.Zero;
            _joystickID = 0;
        }

        _controlIndex = model.ControlIndex;
        _configUUID = model.GetUUID();

        unsafe
        {
            _gameController = new(InputControl.Open(_controlIndex));
        }

        if (_gameController != IntPtr.Zero)
        {
            _joystickID = InputControl.GetJoystickID(_gameController);
        }
    }

    public Task<bool> Closing()
    {
        Window.Hide();

        return Task.FromResult(true);
    }
}