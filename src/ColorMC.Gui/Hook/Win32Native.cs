using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Avalonia.Input;
using Avalonia.Win32.Input;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.Hook;

public class Win32Native : INative
{
    private IntPtr target;

    private IntPtr _winEventId;

    public void AddHook(IntPtr handel)
    {
        target = handel;
    }

    public IntPtr GetHandel()
    {
        return target;
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

    public void SendKey(InputKeyObj key, bool down, bool message)
    {
        if (JoystickInput.IsEditMode)
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

    public void SendScoll(bool up)
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

        internal static void WaitWindowDisplay(Process process)
        {
            try
            {
                while (!process.HasExited)
                {
                    process.WaitForInputIdle();
                    Thread.Sleep(500);
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        break;
                    }
                }
            }
            catch
            {

            }
        }

        internal static void SetTitle(Process process, string title)
        {
            SetWindowText(process.MainWindowHandle, title);
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
        public static extern bool SetWindowText(IntPtr hWnd, string lpString);

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
