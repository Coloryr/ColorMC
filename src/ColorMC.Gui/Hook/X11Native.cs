using System;
using System.Collections.Generic;
using Avalonia.Input;
using ColorMC.Gui.Objs.Config;

namespace ColorMC.Gui.Hook;

public class X11Native : BaseNative
{
    private static readonly Dictionary<MouseButton, uint> MouseKeys = new()
    {
        { MouseButton.Left, 1 },
        { MouseButton.Right, 3 },
        { MouseButton.Middle, 2 },
        { MouseButton.XButton1, 6 },
        { MouseButton.XButton2, 7 },
    };

    private IntPtr display;

    public X11Native() : base(0)
    {
        display = X11Hook.XOpenDisplay(null!);
        if (display == IntPtr.Zero)
        {
            throw new Exception("无法打开X显示");
        }
    }

    public override void Stop()
    {
        if (display != IntPtr.Zero)
        {
            X11Hook.XCloseDisplay(display);
            display = IntPtr.Zero;
        }
    }

    public override void SendMouse(double cursorX, double cursorY)
    {
        X11Hook.XTestFakeRelativeMotionEvent(display, (int)cursorX, (int)cursorY, 0);
        X11Hook.XFlush(display);
    }

    private uint? GetModifierKeyCode(KeyModifiers modifier)
    {
        return modifier switch
        {
            KeyModifiers.Control => X11Hook.XKeysymToKeycode(display, (uint)X11Hook.XKeySym.XK_Control_L),
            KeyModifiers.Shift => X11Hook.XKeysymToKeycode(display, (uint)X11Hook.XKeySym.XK_Shift_L),
            KeyModifiers.Alt => X11Hook.XKeysymToKeycode(display, (uint)X11Hook.XKeySym.XK_Alt_L),
            KeyModifiers.Meta => X11Hook.XKeysymToKeycode(display, (uint)X11Hook.XKeySym.XK_Meta_L),
            _ => null
        };
    }

    private static uint? AvaloniaKeyToXKeysym(Key key)
    {
        foreach (var item in X11Hook.KeyFromX11Key)
        {
            if (item.Value == key)
            {
                return (uint)item.Key;
            }
        }

        return null;
    }

    public override void SendKey(InputKeyObj key, bool down)
    {
        if (key.MouseButton != MouseButton.None)
        {
            X11Hook.XTestFakeButtonEvent(display, MouseKeys[key.MouseButton], down, 0);
            X11Hook.XFlush(display);
        }
        else
        {
            var temp = GetModifierKeyCode(key.KeyModifiers);
            if (temp != null)
            {
                X11Hook.XTestFakeButtonEvent(display, temp.Value, down, 0);
            }
            temp = AvaloniaKeyToXKeysym(key.Key);
            if (temp != null)
            {
                X11Hook.XTestFakeButtonEvent(display, temp.Value, down, 0);
            }
        }
        X11Hook.XFlush(display);
    }

    public override void SendScoll(bool up)
    {
        uint button = up ? 4u : 5u;
        X11Hook.XTestFakeButtonEvent(display, button, true, 0);
        X11Hook.XTestFakeButtonEvent(display, button, false, 0);
        X11Hook.XFlush(display);
    }
}
