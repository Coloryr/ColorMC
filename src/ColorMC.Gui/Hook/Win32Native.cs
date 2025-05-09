using System;
using System.Runtime.InteropServices;
using Avalonia.Input;
using Avalonia.Win32.Input;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Objs.Config;

namespace ColorMC.Gui.Hook;

public class Win32Native(IntPtr ptr) : BaseNative(ptr)
{
    private IntPtr _winEventId;

    //public bool GetWindowSize(out int width, out int height)
    //{
    //    if (Win32.GetWindowRect(target, out var rect))
    //    {
    //        width = rect.Right - rect.Left;
    //        height = rect.Bottom - rect.Top;
    //        return true;
    //    }
    //    else
    //    {
    //        width = 0;
    //        height = 0;
    //        return false;
    //    }
    //}

    public override void Stop()
    {
        if (_winEventId != IntPtr.Zero)
        {
            Win32.UnhookWinEvent(_winEventId);
            _winEventId = 0;
        }
    }

    public override void SendMouse(double cursorX, double cursorY)
    {
        int x = (int)cursorX;
        int y = (int)cursorY;

        //if (message)
        //{
        //    IntPtr lParam = ((y << 16) | (x & 0xFFFF));
        //    Win32.SendMessage(target, Win32.WM_MOUSEMOVE, IntPtr.Zero, lParam);
        //}
        //else
        //{
        // 发送鼠标移动事件
        Win32.INPUT[] inputs =
        [
            new()
            {
                type = Win32.INPUT_MOUSE,
                u = new Win32.InputUnion
                {
                    mi = new Win32.MOUSEINPUT
                    {
                        dx = x,
                        dy = y,
                        mouseData = 0,
                        dwFlags = Win32.MOUSEEVENTF_MOVE,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            }
        ];
        Win32.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Win32.INPUT)));
        //}
    }

    public override void SendKey(InputKeyObj key, bool down)
    {
        if (JoystickInput.IsEditMode)
        {
            return;
        }

        int key1 = 0;
        if (key.KeyModifiers == KeyModifiers.Alt)
        {
            key1 = KeyInterop.VirtualKeyFromKey(Key.LeftAlt);
        }
        else if (key.KeyModifiers == KeyModifiers.Control)
        {
            key1 = KeyInterop.VirtualKeyFromKey(Key.LeftCtrl);
        }
        else if (key.KeyModifiers == KeyModifiers.Shift)
        {
            key1 = KeyInterop.VirtualKeyFromKey(Key.LeftShift);
        }

        if (key1 != 0)
        {
            SendKey(key1, down);
        }

        if (key.MouseButton != MouseButton.None)
        {
            key1 = key.MouseButton switch
            {
                MouseButton.Left => down ? Win32.WM_LBUTTONDOWN : Win32.WM_LBUTTONUP,
                MouseButton.Right => down ? Win32.WM_RBUTTONDOWN : Win32.WM_RBUTTONUP,
                MouseButton.Middle => down ? Win32.WM_MBUTTONDOWN : Win32.WM_MBUTTONDOWN,
                MouseButton.XButton1 or MouseButton.XButton2 =>
                    down ? Win32.WM_XBUTTONDOWN : Win32.WM_XBUTTONUP,
                _ => throw new Exception()
            };
            int key2 = key.MouseButton switch
            {
                MouseButton.XButton1 => Win32.MK_XBUTTON1,
                MouseButton.XButton2 => Win32.MK_XBUTTON2,
                _ => 0
            };
            Win32.SendMessage(Target, key1, key2, IntPtr.Zero);
        }
        else
        {
            key1 = KeyInterop.VirtualKeyFromKey(key.Key);

            SendKey(key1, down);
        }
    }

    private void SendKey(int key1, bool down)
    {
        Win32.INPUT[] inputs =
        [
            new()
            {
                type = Win32.INPUT_KEYBOARD,
                u = new Win32.InputUnion
                {
                    ki = new Win32.KEYBDINPUT
                    {
                        wVk = (ushort)key1,
                        wScan = 0,
                        dwFlags = down ? 0 : Win32.KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero,
                    }
                }
            }
        ];
        Win32.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Win32.INPUT)));
    }

    public override void SendScoll(bool up)
    {
        // 正数滚动向上，负数滚动向下，WHEEL_DELTA通常是120的倍数
        int wheelAmount = 120 * (up ? 1 : -1); // 滚动一次滚轮的距离

        Win32.INPUT[] inputs =
        [
            new()
            {
                type = Win32.INPUT_MOUSE,
                u = new Win32.InputUnion
                {
                    mi = new Win32.MOUSEINPUT
                    {
                        dx = 0,
                        dy = 0,
                        mouseData = (uint)wheelAmount,
                        dwFlags = Win32.MOUSEEVENTF_WHEEL,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            }
        ];

        Win32.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Win32.INPUT)));
    }
}
