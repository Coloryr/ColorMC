using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Win32.Input;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using SkiaSharp;
using System;
using System.Runtime.InteropServices;
using System.Text;
namespace ColorMC.Gui.Utils.Hook;

internal class Win32WindowControlHandle(INative native, IntPtr handle)
    : PlatformHandle(handle, "HWND"), INativeControlHostDestroyableControlHandle
{
    public void Destroy()
    {
        native.DestroyWindow();
    }
}

public class Win32Native : INative
{
    private static Win32.WinEventDelegate _winEventDelegate;

    public event Action<string>? TitleChange;
    public event Action<bool>? IsFocus;

    private IntPtr target;
    private IntPtr topTarget;

    private IntPtr _winEventId;
    private IntPtr hHook;

    public Win32Native()
    {
        _winEventDelegate = new(WinEventProc);
    }

    public void AddHook(uint id, IntPtr handel)
    {
        target = handel;

        uint processId;
        uint threadId = Win32.GetWindowThreadProcessId(target, out processId);

        _winEventId = Win32.SetWinEventHook(Win32.EVENT_OBJECT_NAMECHANGE, Win32.EVENT_OBJECT_NAMECHANGE,
            IntPtr.Zero, _winEventDelegate, id, 0, Win32.WINEVENT_OUTOFCONTEXT);

        hHook = Win32.SetWindowsHookEx(Win32.WH_CALLWNDPROC, CallWndProc, IntPtr.Zero, threadId);
    }

    public void AddHookTop(IntPtr top)
    {
        topTarget = top;
    }

    public void TransferTop()
    {
        if (Win32.IsWindow(topTarget))
        {
            int extendedStyle = Win32.GetWindowLong(topTarget, Win32.GWL_EXSTYLE);
            Win32.SetWindowLong(topTarget, Win32.GWL_EXSTYLE, extendedStyle | Win32.WS_EX_LAYERED | Win32.WS_EX_TRANSPARENT);
        }
    }
    public void NoTranferTop()
    {
        if (Win32.IsWindow(topTarget))
        {
            int extendedStyle = Win32.GetWindowLong(topTarget, Win32.GWL_EXSTYLE);
            extendedStyle &= ~(Win32.WS_EX_LAYERED | Win32.WS_EX_TRANSPARENT);
            Win32.SetWindowLong(topTarget, Win32.GWL_EXSTYLE, extendedStyle);
        }
    }

    private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject,
        int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        if (eventType == Win32.EVENT_OBJECT_NAMECHANGE)
        {
            var title = GetWindowTitle();
            TitleChange?.Invoke(title);
        }
    }

    // 钩子回调函数
    private IntPtr CallWndProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            Win32.CWPSTRUCT msg = (Win32.CWPSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32.CWPSTRUCT));

            // 在这里处理您感兴趣的消息
            Logs.Info($"Message: {msg.message} HWND: {msg.hwnd}");
        }
        return Win32.CallNextHookEx(hHook, nCode, wParam, lParam);
    }

    public bool GetWindowSize(out int width, out int height)
    {
        if (Win32.GetWindowRect(target, out var rect))
        {
            width = rect.Right - rect.Left;
            height = rect.Bottom - rect.Top;
            return true;
        }
        else
        {
            width = 0;
            height = 0;
            return false;
        }
    }

    public void NoBorder()
    {
        int wndStyle = Win32.GetWindowLong(target, Win32.GWL_STYLE);
        wndStyle &= ~(Win32.WS_CAPTION | Win32.WS_THICKFRAME | Win32.WS_MINIMIZE
            | Win32.WS_MAXIMIZE | Win32.WS_SYSMENU);
        Win32.SetWindowLong(target, Win32.GWL_STYLE, wndStyle);
        Win32.SetWindowPos(target, IntPtr.Zero, 0, 0, 0, 0, Win32.SWP_FRAMECHANGED
            | Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOZORDER);
    }

    public IPlatformHandle CreateControl()
    {
        NoBorder();
        return new Win32WindowControlHandle(this, target);
    }

    public void SetWindowState(WindowState state)
    {
        if (state == WindowState.Minimized)
        {
            Win32.ShowWindow(target, Win32.SW_MINIMIZE);
        }
        else if (state == WindowState.Maximized)
        {
            Win32.ShowWindow(target, Win32.SW_MAXIMIZE);
        }
        else if (state == WindowState.Normal)
        {
            Win32.ShowWindow(target, Win32.SW_RESTORE);
        }
    }

    public string GetWindowTitle()
    {
        int length = Win32.GetWindowTextLength(target);
        var sb = new StringBuilder(length + 1);
        Win32.GetWindowText(target, sb, sb.Capacity);
        return sb.ToString();
    }

    private IntPtr GetWindowIcon(int iconSize = Win32.ICON_SMALL)
    {
        IntPtr hIcon = Win32.SendMessage(target, Win32.WM_GETICON, iconSize, 0);
        if (hIcon == IntPtr.Zero)
        {
            hIcon = Win32.SendMessage(target, Win32.WM_GETICON, Win32.ICON_BIG, 0);
        }
        if (hIcon == IntPtr.Zero)
        {
            hIcon = Win32.SendMessage(target, Win32.WM_GETICON, Win32.ICON_SMALL2, 0);
        }
        return hIcon;
    }

    public Bitmap? GetIcon()
    {
        var ptr = GetWindowIcon();
        if (ptr == IntPtr.Zero)
        {
            return null;
        }
        if (Win32.GetIconInfo(ptr, out var iconInfo))
        {
            try
            {
                if (Win32.GetObject(iconInfo.hbmColor, Marshal.SizeOf(typeof(Win32.BITMAP)), out var bmpColor) == 0)
                {
                    return null;
                }
                if (Win32.GetObject(iconInfo.hbmMask, Marshal.SizeOf(typeof(Win32.BITMAP)), out var bmpMask) == 0)
                {
                    return null;
                }

                var bmi = new Win32.BITMAPINFO();
                bmi.biSize = Marshal.SizeOf(bmi);
                bmi.biWidth = bmpColor.bmWidth; // Icon width
                bmi.biHeight = -bmpColor.bmHeight; // Icon height (negative to flip the image)
                bmi.biPlanes = bmpColor.bmPlanes;
                bmi.biBitCount = bmpColor.bmBitsPixel;
                bmi.biCompression = 0; // BI_RGB

                IntPtr hdc = Win32.CreateCompatibleDC(IntPtr.Zero);
                byte[] pixelData = new byte[bmpColor.bmWidth * bmpColor.bmHeight * 4];
                Win32.GetDIBits(hdc, iconInfo.hbmColor, 0, (uint)bmpColor.bmHeight, pixelData, ref bmi, 0);

                using var skBitmap = new SKBitmap(bmpColor.bmWidth, bmpColor.bmHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
                Marshal.Copy(pixelData, 0, skBitmap.GetPixels(), pixelData.Length);

                // Convert SKBitmap to Avalonia Bitmap
                using var image = SKImage.FromBitmap(skBitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                var stream = data.AsStream();
                var bitmap = new Bitmap(stream);
                return bitmap;
            }
            finally
            {
                if (iconInfo.hbmColor != IntPtr.Zero)
                    Win32.DeleteObject(iconInfo.hbmColor);
                if (iconInfo.hbmMask != IntPtr.Zero)
                    Win32.DeleteObject(iconInfo.hbmMask);
                Win32.DestroyIcon(ptr);
            }
        }

        return null;
    }

    public void Close()
    {
        Win32.SendMessage(target, Win32.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
    }

    public void DestroyWindow()
    {
        Win32.DestroyWindow(target);
    }

    public void Stop()
    {
        if (_winEventId != IntPtr.Zero)
        {
            Win32.UnhookWinEvent(_winEventId);
            _winEventId = 0;
        }
    }

    public void SendMouse(double cursorX, double cursorY, bool message)
    {
        int x = (int)cursorX;
        int y = (int)cursorY;

        if (message)
        {
            IntPtr lParam = ((y << 16) | (x & 0xFFFF));
            Win32.SendMessage(target, Win32.WM_MOUSEMOVE, IntPtr.Zero, lParam);
        }
        else
        {
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
        }
    }

    private static bool IsCursorShown()
    {
        var cursorInfo = new Win32.CURSORINFO();
        cursorInfo.cbSize = Marshal.SizeOf(cursorInfo);
        if (Win32.GetCursorInfo(out cursorInfo))
        {
            return (cursorInfo.flags & Win32.CURSOR_SHOWING) != 0;
        }
        return false; // 如果无法获取光标信息，则默认返回false
    }

    private static bool IsCursorClippedToWindow(IntPtr hwnd)
    {
        if (Win32.GetClipCursor(out var clipRect))
        {
            if (Win32.GetWindowRect(hwnd, out var windowRect))
            {
                // 比较剪辑矩形和窗口矩形
                return clipRect.Left >= windowRect.Left &&
                       clipRect.Top >= windowRect.Top &&
                       clipRect.Right <= windowRect.Right &&
                       clipRect.Bottom <= windowRect.Bottom;
            }
        }
        return false;
    }

    public bool GetMouseMode()
    {
        return IsCursorShown() || !IsCursorClippedToWindow(target);
    }

    public void SendKey(InputKeyObj key, bool down, bool message)
    {
        if (InputControlUtils.IsEditMode)
        {
            return;
        }

        int key1 = 0;
        if (key.KeyModifiers == KeyModifiers.Alt)
        {
            KeyInterop.VirtualKeyFromKey(Key.LeftAlt);
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
            Win32.SendMessage(target, key1, key2, IntPtr.Zero);
        }
        else
        {
            key1 = KeyInterop.VirtualKeyFromKey(key.Key);
            if (message)
            {
                //Win32.SendMessage(target, down ? Win32.WM_KEYDOWN : Win32.WM_KEYUP, key1, IntPtr.Zero);
            }
            else
            {
                
            }

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

    public void SendScoll(int count, bool up)
    {
        // 正数滚动向上，负数滚动向下，WHEEL_DELTA通常是120的倍数
        int wheelAmount = 120 * count * (up ? 1 : -1); // 滚动一次滚轮的距离

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

    internal unsafe class Win32
    {
        public const int WM_CLOSE = 0x0010;
        public const int WM_DESTROY = 0x0002;
        public const int WH_GETMESSAGE = 3;
        public const int WH_CALLWNDPROC = 4;
        public const int WM_GETICON = 0x007F;

        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_RBUTTONDBLCLK = 0x0206;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_MBUTTONUP = 0x0208;
        public const int WM_MBUTTONDBLCLK = 0x0209;
        public const int WM_XBUTTONDOWN = 0x020B;
        public const int WM_XBUTTONUP = 0x020C;
        public const int WM_XBUTTONDBLCLK = 0x020D;

        public const int INPUT_MOUSE = 0;
        public const int INPUT_KEYBOARD = 1;
        public const uint MOUSEEVENTF_WHEEL = 0x0800;
        public const uint MOUSEEVENTF_MOVE = 0x0001;
        public const uint KEYEVENTF_KEYUP = 0x0002;

        public const int WS_CAPTION = 0x00C00000;
        public const int WS_THICKFRAME = 0x00040000;
        public const int WS_MINIMIZE = 0x20000000;
        public const int WS_MAXIMIZE = 0x01000000;
        public const int WS_SYSMENU = 0x00080000;

        public const int MK_LBUTTON = 0x0001;
        public const int MK_MBUTTON = 0x0010;
        public const int MK_RBUTTON = 0x0002;
        public const int MK_XBUTTON1 = 0x0020;
        public const int MK_XBUTTON2 = 0x0040;

        // ShowWindow函数的命令常量
        public const int SW_MINIMIZE = 6;
        public const int SW_MAXIMIZE = 3;
        public const int SW_RESTORE = 9;

        // 常量用于SetWindowPos函数
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOZORDER = 0x0004;

        // 定义事件常量
        public const uint EVENT_OBJECT_NAMECHANGE = 0x800C;

        public const uint WINEVENT_OUTOFCONTEXT = 0;

        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public const int ICON_SMALL2 = 2;

        public const int GWL_WNDPROC = -4;
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TRANSPARENT = 0x20;

        public const uint LWA_COLORKEY = 0x00000001;
        public const uint LWA_ALPHA = 0x00000002;

        public const int CURSOR_SHOWING = 0x00000001;

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left; // x position of upper-left corner
            public int Top; // y position of upper-left corner
            public int Right; // x position of lower-right corner
            public int Bottom; // y position of lower-right corner
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            public int biSize;
            public int biWidth, biHeight;
            public short biPlanes, biBitCount;
            public int biCompression, biSizeImage;
            public int biXPelsPerMeter, biYPelsPerMeter;
            public int biClrUsed, biClrImportant;
            public int colors; // Actually a variable sized array of RGBQUAD
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SETTEXTEX
        {
            public uint Flags;
            public uint Codepage;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public short bmPlanes;
            public short bmBitsPixel;
            public IntPtr bmBits;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ICONINFO
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        // 定义CWPSTRUCT结构体，用于处理消息参数
        [StructLayout(LayoutKind.Sequential)]
        public struct CWPSTRUCT
        {
            public IntPtr lParam;
            public IntPtr wParam;
            public uint message;
            public IntPtr hwnd;
        }

        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern int GetObject(IntPtr hgdiobj, int cbBuffer, out BITMAP lpvObject);

        [DllImport("gdi32.dll")]
        public static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan,
           uint cScanLines, [Out] byte[] lpvBits, ref BITMAPINFO lpbmi, uint uUsage);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        public static extern bool GetCursorInfo(out CURSORINFO pci);

        //[DllImport("user32.dll")]
        //public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        //[DllImport("user32.dll", SetLastError = true)]
        //public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        //[DllImport("user32.dll", SetLastError = true)]
        //public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO pIconInfo);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SendMessageW")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        //[DllImport("user32.dll", SetLastError = true)]
        //public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool GetClipCursor(out RECT lpRect);

        //[DllImport("user32.dll")]
        //public static extern int ShowCursor(bool bShow);

        //[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern IntPtr GetModuleHandle(string lpModuleName);

        //[DllImport("kernel32.dll")]
        //public static extern uint GetCurrentThreadId();
    }
}
