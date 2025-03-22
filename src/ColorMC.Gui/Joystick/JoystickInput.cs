using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Silk.NET.SDL;
using Thread = System.Threading.Thread;

namespace ColorMC.Gui.Joystick;

/// <summary>
/// 手柄输入操作
/// </summary>
public static class JoystickInput
{
    /// <summary>
    /// 是否在从SDL读取事件
    /// </summary>
    private static bool _isRun;
    /// <summary>
    /// SDL句柄
    /// </summary>
    private static Sdl _sdl;
    /// <summary>
    /// 是否在编辑模式
    /// </summary>
    public static bool IsEditMode { get; set; }
    /// <summary>
    /// 发生SDL事件时
    /// </summary>
    public static event Action<Event>? OnEvent;

    /// <summary>
    /// 手柄控制初始化
    /// </summary>
    public static void Init(Sdl sdl)
    {
        _sdl = sdl;
        App.OnClose += App_OnClose;
        _isRun = true;
        new Thread(() =>
        {
            var sdlEvent = new Event();
            while (_isRun)
            {
                _sdl.WaitEvent(ref sdlEvent);
                OnEvent?.Invoke(sdlEvent);
            }
        })
        {
            Name = "ColorMC Joystick Read"
        }.Start();
    }

    private static void App_OnClose()
    {
        _isRun = false;
    }

    /// <summary>
    /// 获取手柄数量
    /// </summary>
    public static int Count => _sdl.NumJoysticks();

    /// <summary>
    /// 获取手柄名字列表
    /// </summary>
    /// <returns></returns>
    public static List<string> GetNames()
    {
        var list = new List<string>();
        for (int i = 0; i < Count; i++)
        {
            if (_sdl.IsGameController(i) == SdlBool.True)
            {
                unsafe
                {
                    var gameController = _sdl.GameControllerOpen(i);
                    if (gameController != null)
                    {
                        list.Add(BytePointerToString(_sdl.GameControllerName(gameController)));
                        _sdl.GameControllerClose(gameController);
                    }
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 指针转字符串 
    /// </summary>
    /// <param name="bytePointer"></param>
    /// <returns></returns>
    private static unsafe string BytePointerToString(byte* bytePointer)
    {
        return Marshal.PtrToStringAuto(new nint(bytePointer)) ?? "";
    }

    /// <summary>
    /// 打开手柄
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static nint Open(int index)
    {
        unsafe
        {
            return new nint(_sdl.GameControllerOpen(index));
        }
    }

    /// <summary>
    /// 关闭手柄
    /// </summary>
    /// <param name="index"></param>
    public static void Close(nint index)
    {
        unsafe
        {
            _sdl.GameControllerClose((GameController*)index);
        }
    }

    /// <summary>
    /// 获取手柄编号
    /// </summary>
    /// <param name="ptr">手柄</param>
    /// <returns>编号</returns>
    public static unsafe int GetJoystickInstanceID(nint ptr)
    {
        var instanceID = _sdl.GameControllerGetJoystick((GameController*)ptr);
        return _sdl.JoystickInstanceID(instanceID);
    }
}
