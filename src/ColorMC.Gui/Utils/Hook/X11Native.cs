using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Objs;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using X11;

namespace ColorMC.Gui.Utils.Hook;

public class X11Native : INative
{
    public event Action<string>? TitleChange;
    public event Action<bool>? IsFocus;

    private IntPtr target;
    private IntPtr topTarget;
    private IntPtr display;

    public void AddHook(Process process, IntPtr handel)
    {
        target = handel;

        var pd = process.StartInfo.Environment["DISPLAY"];

        display = Xlib.XOpenDisplay(pd);
        if (display == IntPtr.Zero)
        {
            throw new Exception("DISPLAY open fail");
        }
    }

    public void AddHookTop(IntPtr top)
    {
        topTarget = top;
    }


    private bool GetWindow(out XWindowAttributes attributes)
    {
        return Xlib.XGetWindowAttributes(display, target, out attributes) != Status.Failure;
    }

    public void NoTranferTop()
    {
        if (GetWindow(out var attributes))
        {
            X11.XRectangle rect = new() { x = 0, y = 0, width = (short)attributes.width, height = (short)attributes.height };
            X11.XShapeCombineRectangles(display, target, X11.ShapeKind.ShapeInput, 0, 0, ref rect, 1, X11.ShapeOp.ShapeSet, 0);
        }
    }


    public void TransferTop()
    {
        if (GetWindow(out _))
        {
            X11.XRectangle rect = new() { x = 0, y = 0, width = 0, height = 0 };
            X11.XShapeCombineRectangles(display, target, X11.ShapeKind.ShapeInput, 0, 0, ref rect, 1, X11.ShapeOp.ShapeSet, 0);
        }
    }


    public void Close()
    {
        var wmDelete = Xlib.XInternAtom(display, "WM_DELETE_WINDOW", false);

        XAnyEvent evt = new()
        {
            type = (int)Event.ClientMessage,
            send_event = true,
            display = display,
            window = target,
            serial = (ulong)wmDelete
        };

        unsafe
        {
            Xlib.XSendEvent(display, target, false, (long)EventMask.NoEventMask, new IntPtr(&evt));
        }
        Xlib.XFlush(display);
    }

    public void DestroyWindow()
    {
        Xlib.XDestroyWindow(display, target);
    }

    public Bitmap? GetIcon()
    {
        var atomNetWmIcon = Xlib.XInternAtom(display, "_NET_WM_ICON", false);

        if (X11.XGetWindowProperty(display, target, atomNetWmIcon, 0, long.MaxValue, false, Atom.None,
            out var actualType, out int actualFormat, out long nItems, out long bytesAfter, out IntPtr prop) == 0 && prop != IntPtr.Zero)
        {
            try
            {
                if (actualFormat == 32)
                {
                    // The first two values are width and height
                    int width = Marshal.ReadInt32(prop);
                    int height = Marshal.ReadInt32(prop, 4);

                    // Prepare to read the ARGB values
                    var argbValues = new int[width * height];
                    Marshal.Copy(prop, argbValues, 2, argbValues.Length);

                    // Create SKBitmap from the ARGB values
                    var info = new SKImageInfo(width, height, SKColorType.Bgra8888);
                    var skBitmap = new SKBitmap(info);
                    unsafe
                    {
                        fixed (void* ptr = argbValues)
                        {
                            skBitmap.SetPixels(new IntPtr(ptr));
                        }
                    }
                    // Convert SKBitmap to Avalonia Bitmap
                    using var image = SKImage.FromBitmap(skBitmap);
                    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    var stream = data.AsStream();
                    var bitmap = new Bitmap(stream);
                    return bitmap;
                }
            }
            finally
            {
                Xlib.XFree(prop);
            }
        }

        return null;
    }

    public bool GetMouseMode()
    {
        if (Xlib.XQueryPointer(display, target, out IntPtr root_return, out IntPtr child_return, 
            out int root_x, out int root_y, out int win_x, out int win_y, out uint mask_return))
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public bool GetWindowSize(out int width, out int height)
    {
        if (GetWindow(out var attributes))
        {
            width = (int)attributes.width;
            height = (int)attributes.height;
            return true;
        }
        else
        {
            width = 0;
            height = 0;
            return false;
        }
    }

    public string GetWindowTitle()
    {
        Xlib.XFetchName(display, target, out var window_name);

        return window_name ?? "";
    }

    public void NoBorder()
    {
        // _NET_WM_WINDOW_TYPE
        var xaPropType = Xlib.XInternAtom(display, "_NET_WM_WINDOW_TYPE", false);
        // _NET_WM_WINDOW_TYPE_DIALOG
        var xaDialogAtom = Xlib.XInternAtom(display, "_NET_WM_WINDOW_TYPE_DIALOG", false);

        IntPtr data = (IntPtr)xaDialogAtom;
        Xlib.XChangeProperty(display, target, xaPropType, xaPropType, 32, X11.PropModeReplace, data, 1);
    }

    private void SendMouse(byte key1, bool down)
    {
        var xKeyEvent = new XKeyEvent
        {
            type = down ? (int)Event.ButtonPress : (int)Event.ButtonRelease,
            serial = IntPtr.Zero,
            send_event = true,
            display = display,
            window = target,
            root = Xlib.XRootWindow(display, 0),
            subwindow = IntPtr.Zero,
            time = 0,
            x = 0,
            y = 0,
            x_root = 0,
            y_root = 0,
            state = 0,
            keycode = key1,
            same_screen = true
        };

        unsafe
        {
            Xlib.XSendEvent(display, target, true, (long)(down ? EventMask.ButtonPressMask : EventMask.ButtonReleaseMask), new IntPtr(&xKeyEvent));
        }
        Xlib.XFlush(display);
    }

    private void SendKey(byte key1, bool down)
    {
        var xKeyEvent = new XKeyEvent
        {
            type = down ? (int)Event.KeyPress : (int)Event.KeyRelease,
            serial = IntPtr.Zero,
            send_event = true,
            display = display,
            window = target,
            root = Xlib.XRootWindow(display, 0),
            subwindow = IntPtr.Zero,
            time = 0,
            x = 0,
            y = 0,
            x_root = 0,
            y_root = 0,
            state = 0,
            keycode = key1,
            same_screen = true
        };

        unsafe
        {
            Xlib.XSendEvent(display, target, true, (long)(down ? EventMask.KeyPressMask : EventMask.KeyReleaseMask), new IntPtr(&xKeyEvent));
        }
        Xlib.XFlush(display);
    }

    public void SendKey(InputKeyObj key, bool down, bool message)
    {
        if (InputControlUtils.IsEditMode)
        {
            return;
        }

        X11Key key1 = 0;
        if (key.KeyModifiers == KeyModifiers.Alt)
        {
            X11KeyTransform.X11KeyFromKey(Key.LeftAlt);
        }
        else if (key.KeyModifiers == KeyModifiers.Control)
        {
            key1 = X11KeyTransform.X11KeyFromKey(Key.LeftCtrl);
        }
        else if (key.KeyModifiers == KeyModifiers.Shift)
        {
            key1 = X11KeyTransform.X11KeyFromKey(Key.LeftShift);
        }

        if (key1 != 0)
        {
            var keycode = Xlib.XKeysymToKeycode(display, (KeySym)key1);
            SendKey((byte)keycode, down);
        }

        if (key.MouseButton != MouseButton.None)
        {
            var key2 = key.MouseButton switch
            {
                MouseButton.Left => Button.LEFT,
                MouseButton.Right => Button.RIGHT,
                MouseButton.Middle => Button.MIDDLE,
                MouseButton.XButton1 => Button.X1,
                MouseButton.XButton2 => Button.X2,
                _ => throw new Exception()
            };
            SendMouse((byte)key2, down);
        }
        else
        {
            key1 = X11KeyTransform.X11KeyFromKey(key.Key);

            SendKey((byte)key1, down);
        }
    }

    public void SendMouse(double cursorX, double cursorY, bool message)
    {
        int x = (int)cursorX;
        int y = (int)cursorY;
        XMotionEvent motionEvent = new()
        {
            type = 6, // MotionNotify
            serial = 0,
            send_event = true,
            display = display,
            window = target,
            root = Xlib.XRootWindow(display, 0),
            subwindow = 0,
            time = 0, // You can also use Xlib's CurrentTime
            x = x,
            y = y,
            x_root = x,
            y_root = y,
            state = 0,
            is_hint = 0,
            same_screen = true,
        };

        unsafe
        {
            Xlib.XSendEvent(display, target, true, (long)EventMask.PointerMotionMask, new IntPtr(&motionEvent));
        }
    }

    public void SendScoll(bool up)
    {
        SendMouse((byte)(up ? Button.FOUR : Button.FIVE), true);
        SendMouse((byte)(up ? Button.FOUR : Button.FIVE), false);
    }

    private void ChangeState(IntPtr stateAtom, bool set)
    {
        var wmState = Xlib.XInternAtom(display, "_NET_WM_STATE", false);

        var xev = new XWindowStateEvent
        {
            type = (int)Event.ClientMessage,
            serial = IntPtr.Zero,
            send_event = true,
            display = display,
            window = target,
            message_type = (nint)wmState,
            format = 32,
            data1 = (set ? 1 : 0), // _NET_WM_STATE_ADD : _NET_WM_STATE_REMOVE
            data2 = stateAtom,
            data3 = IntPtr.Zero,
            data4 = IntPtr.Zero,
            data5 = IntPtr.Zero
        };

        unsafe
        {
            Xlib.XSendEvent(display, target, false, (long)(EventMask.SubstructureRedirectMask | EventMask.SubstructureNotifyMask), new IntPtr(&xev));
        }
    }
    
    public void SetWindowState(Avalonia.Controls.WindowState state)
    {
        var stateAtom = (IntPtr)Xlib.XInternAtom(display, "_NET_WM_STATE_MAXIMIZED_HORZ", false);
        var stateAtom1 = (IntPtr)Xlib.XInternAtom(display, "_NET_WM_STATE_MAXIMIZED_VERT", false);
        var stateAtom2 = (IntPtr)Xlib.XInternAtom(display, "_NET_WM_STATE_HIDDEN", false);

        if (state == Avalonia.Controls.WindowState.Normal)
        {
            ChangeState(stateAtom, false);
            ChangeState(stateAtom1, false);
            ChangeState(stateAtom2, false);
        }
        else if (state == Avalonia.Controls.WindowState.Minimized)
        {
            ChangeState(stateAtom, false);
            ChangeState(stateAtom1, false);
            
            ChangeState(stateAtom2, true);
        }
        else if (state == Avalonia.Controls.WindowState.Maximized)
        {
            ChangeState(stateAtom2, false);

            ChangeState(stateAtom, true);
            ChangeState(stateAtom1, true);
        }

        Xlib.XFlush(display);
    }

    public void Stop()
    {
        if (display != IntPtr.Zero)
        {
            Xlib.XCloseDisplay(display);
            display = IntPtr.Zero;
        }
    }
}

