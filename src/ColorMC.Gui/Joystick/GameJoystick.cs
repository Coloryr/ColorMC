using System.Collections.Generic;
using System.Threading;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.Hook;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.Utils;
using Event = Silk.NET.SDL.Event;
using EventType = Silk.NET.SDL.EventType;
using GameControllerAxis = Silk.NET.SDL.GameControllerAxis;

namespace ColorMC.Gui.Joystick;

public class GameJoystick
{
    /// <summary>
    /// 现有的游戏手柄操作储存
    /// </summary>
    public static readonly Dictionary<string, GameJoystick> NowGameJoystick = [];

    /// <summary>
    /// 底层操作接口
    /// </summary>
    private readonly INative _implementation;
    /// <summary>
    /// 最大的遥感值
    /// </summary>
    private readonly short[] _lastAxisMax = new short[10];
    /// <summary>
    /// 保存的按键值
    /// </summary>
    private readonly Dictionary<InputKeyObj, bool> _lastKeyState = [];
    /// <summary>
    /// 游戏实例
    /// </summary>
    private readonly GameSettingObj _obj;
    /// <summary>
    /// 当前指针的位置
    /// </summary>
    private double _cursorNowX, _cursorNowY;
    /// <summary>
    /// 选择的手柄
    /// </summary>
    private nint _gameController;
    /// <summary>
    /// 手柄ID
    /// </summary>
    private int _joystickID, _controlIndex;
    /// <summary>
    /// 启用的手柄配置
    /// </summary>
    private string? _configUUID;
    /// <summary>
    /// 游戏是否退出
    /// </summary>
    private bool _isExit;
    /// <summary>
    /// 是否为鼠标指针模式
    /// </summary>
    private bool _mouseMode = true;

    /// <summary>
    /// 设置鼠标显示
    /// </summary>
    /// <param name="uuid">游戏UUID</param>
    /// <param name="show">是否显示鼠标指针</param>
    public static void SetMouse(string uuid, bool show)
    {
        if (NowGameJoystick.TryGetValue(uuid, out var value))
        {
            value._mouseMode = !show;
        }
    }

    /// <summary>
    /// 开始处理手柄操作
    /// </summary>
    /// <param name="obj">游戏储存</param>
    /// <param name="handel">游戏操作句柄</param>
    public static void Start(GameSettingObj obj, IGameHandel handel)
    {
        if (NowGameJoystick.Remove(obj.UUID, out var value))
        {
            value.Stop();
        }
        NowGameJoystick.Add(obj.UUID, new(obj, handel));
    }

    /// <summary>
    /// 游戏手柄操作
    /// </summary>
    /// <param name="obj">游戏储存</param>
    /// <param name="handel">游戏操作句柄</param>
    private GameJoystick(GameSettingObj obj, IGameHandel handel)
    {
        //目前只支持windows
        if (SystemInfo.Os == OsType.Windows)
        {
            _implementation = new Win32Native();
        }
        else
        {
            return;
        }

        ColorMCCore.GameExit += GameExit;

        _implementation!.AddHook(handel.Handel);

        _obj = obj;
        _controlIndex = 0;
        _configUUID = GuiConfigUtils.Config.Input.NowConfig;

        _gameController = JoystickInput.Open(_controlIndex);

        if (_gameController != nint.Zero)
        {
            JoystickInput.OnEvent += Event;
            _joystickID = JoystickInput.GetJoystickID(_gameController);
        }

        //开始发送鼠标指针
        new Thread(() =>
        {
            while (!_isExit)
            {
                if (_cursorNowX != 0 || _cursorNowY != 0)
                {
                    _implementation.SendMouse(_cursorNowX, _cursorNowY, false);
                }

                Thread.Sleep(10);
            }
        }).Start();
    }

    /// <summary>
    /// 游戏退出处理
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    private void GameExit(GameSettingObj obj, LoginObj arg2, int arg3)
    {
        if (obj.UUID == _obj.UUID)
        {
            Stop();
        }
    }

    /// <summary>
    /// 停止游戏手柄处理
    /// </summary>
    private void Stop()
    {
        if (_implementation != null)
        {
            _isExit = true;
            ColorMCCore.GameExit -= GameExit;
            _implementation.Stop();
        }
    }

    /// <summary>
    /// 添加按键状态
    /// </summary>
    /// <param name="obj">按键</param>
    /// <param name="down">是否按下</param>
    private void CheckKeyAndSend(InputKeyObj obj, bool down)
    {
        if (_lastKeyState.TryGetValue(obj, out var state))
        {
            if (state != down)
            {
                _lastKeyState[obj] = down;
                _implementation.SendKey(obj, down, _mouseMode);
            }
        }
        else
        {
            _lastKeyState.Add(obj, down);
            _implementation.SendKey(obj, down, _mouseMode);
        }
    }

    /// <summary>
    /// 处理SDL事件
    /// </summary>
    /// <param name="sdlEvent"></param>
    private void Event(Event sdlEvent)
    {
        if (!GuiConfigUtils.Config.Input.Enable
            || sdlEvent.Cbutton.Which != _joystickID
            || string.IsNullOrWhiteSpace(_configUUID)
            || !JoystickConfig.Configs.TryGetValue(_configUUID, out var config))
        {
            return;
        }

        var type = (EventType)sdlEvent.Type;
        //摇杆事件
        if (type == EventType.Controlleraxismotion)
        {
            var axisEvent = sdlEvent.Caxis;
            var axisValue = axisEvent.Value;

            var axis = (GameControllerAxis)axisEvent.Axis;

            var axisFixValue = axisValue / config.DownRate * (_mouseMode ? config.CursorRate : config.RotateRate);
            var deathSize = _mouseMode ? config.CursorDeath : config.RotateDeath;
            var choiseAxis = _mouseMode ? config.CursorAxis : config.RotateAxis;
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
            if (_mouseMode)
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
        //手柄按键按下
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
        //手柄按键松开
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

    /// <summary>
    /// 设置手柄映射配置
    /// </summary>
    /// <param name="model">配置</param>
    public void ChangeConfig(JoystickSettingModel model)
    {
        if (_isExit)
        {
            return;
        }

        _configUUID = model.GetUUID();

        //切换手柄
        if (_controlIndex != model.ControlIndex)
        {
            //关闭旧的手柄
            if (_gameController != nint.Zero)
            {
                JoystickInput.Close(_gameController);
                _gameController = nint.Zero;
                _joystickID = 0;
            }


            _controlIndex = model.ControlIndex;

            //切换新的手柄
            _gameController = JoystickInput.Open(_controlIndex);

            if (_gameController != nint.Zero)
            {
                _joystickID = JoystickInput.GetJoystickID(_gameController);
            }
        }
    }

    /// <summary>
    /// 创建显示模型
    /// </summary>
    /// <returns>显示用模型</returns>
    public JoystickSettingModel MakeModel()
    {
        return new JoystickSettingModel(_controlIndex, _configUUID);
    }
}
