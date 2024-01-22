using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ColorMC.Gui.UI.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Utils.Hook;

public class Win32Native : INative
{
    private static Win32.HookProc _tranEventDelegate;
    private static Win32.WinEventDelegate _winEventDelegate;

    public event Action<string>? TitleChange;

    private IntPtr hWnd;

    private IntPtr _winEventId;
    private IntPtr _winTranId;

    public Win32Native()
    {
        _tranEventDelegate = new(WinTranEventProc);
        _winEventDelegate = new(WinEventProc);
    }

    public void TransferEvent(IntPtr handel)
    {
        Win32.GetWindowThreadProcessId(handel, out uint threadId);
        _winTranId = Win32.SetWindowsHookEx(Win32.WH_CALLWNDPROC, _tranEventDelegate,
             IntPtr.Zero, threadId);
        if (_winTranId == 0)
        {
            int errorCode = Marshal.GetLastWin32Error();
        }
    }

    public void AddHook(IntPtr handel)
    {
        hWnd = handel;
        _winEventId = Win32.SetWinEventHook(Win32.EVENT_OBJECT_NAMECHANGE, Win32.EVENT_OBJECT_NAMECHANGE,
            IntPtr.Zero, _winEventDelegate, 0, 0, Win32.WINEVENT_OUTOFCONTEXT);
    }

    private IntPtr WinTranEventProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            Win32.SendMessage(hWnd, nCode, wParam, lParam);
        }

        return Win32.CallNextHookEx(_winEventId, nCode, wParam, lParam);
    }

    private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject,
        int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        if (eventType == Win32.EVENT_OBJECT_NAMECHANGE)
        {
            var title = GetWindowTitle(hwnd);
            TitleChange?.Invoke(title);
        }
    }

    public bool GetWindowSize(IntPtr handel, out int width, out int height)
    {
        if (Win32.GetWindowRect(handel, out var rect))
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

    public void NoBorder(IntPtr handel)
    {
        int wndStyle = Win32.GetWindowLong(handel, Win32.GWL_STYLE);
        wndStyle &= ~(Win32.WS_CAPTION | Win32.WS_THICKFRAME | Win32.WS_MINIMIZE
            | Win32.WS_MAXIMIZE | Win32.WS_SYSMENU);
        Win32.SetWindowLong(handel, Win32.GWL_STYLE, wndStyle);
        Win32.SetWindowPos(handel, Win32.HWND_TOP, 0, 0, 0, 0, Win32.SWP_FRAMECHANGED
            | Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOZORDER);
    }

    public IPlatformHandle CreateControl(IntPtr handel)
    {
        NoBorder(handel);
        return new Win32WindowControlHandle(this, handel);
    }

    public void SetWindowState(IntPtr handel, WindowState state)
    {
        if (state == WindowState.Minimized)
        {
            Win32.ShowWindow(handel, Win32.SW_MINIMIZE);
        }
        else if (state == WindowState.Maximized)
        {
            Win32.ShowWindow(handel, Win32.SW_MAXIMIZE);
        }
        else if (state == WindowState.Normal)
        {
            Win32.ShowWindow(handel, Win32.SW_RESTORE);
        }
    }

    public string GetWindowTitle(IntPtr hWnd)
    {
        int length = Win32.GetWindowTextLength(hWnd);
        var sb = new StringBuilder(length + 1);
        Win32.GetWindowText(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private IntPtr GetWindowIcon(IntPtr hWnd, int iconSize = Win32.ICON_SMALL)
    {
        IntPtr hIcon = Win32.SendMessage(hWnd, Win32.WM_GETICON, iconSize, 0);
        if (hIcon == IntPtr.Zero)
        {
            hIcon = Win32.SendMessage(hWnd, Win32.WM_GETICON, Win32.ICON_BIG, 0);
        }
        if (hIcon == IntPtr.Zero)
        {
            hIcon = Win32.SendMessage(hWnd, Win32.WM_GETICON, Win32.ICON_SMALL2, 0);
        }
        return hIcon;
    }

    public Bitmap? GetIcon(IntPtr hWnd)
    {
        var ptr = GetWindowIcon(hWnd);
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
                // Clean up GDI and icon objects
                if (iconInfo.hbmColor != IntPtr.Zero)
                    Win32.DeleteObject(iconInfo.hbmColor);
                if (iconInfo.hbmMask != IntPtr.Zero)
                    Win32.DeleteObject(iconInfo.hbmMask);
                Win32.DestroyIcon(ptr);
            }
        }

        return null;
    }

    public void Close(IntPtr handle)
    {
        Win32.SendMessage(handle, Win32.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
    }

    public void DestroyWindow(IntPtr handle)
    {
        Win32.DestroyWindow(handle);
    }

    internal unsafe class Win32
    {
        public const int WM_CLOSE = 0x0010;

        // ShowWindow函数的命令常量
        public const int SW_MINIMIZE = 6;
        public const int SW_MAXIMIZE = 3;
        public const int SW_RESTORE = 9;

        // 窗口样式常量
        public const int GWL_STYLE = -16;
        public const int WS_CAPTION = 0x00C00000;
        public const int WS_THICKFRAME = 0x00040000;
        public const int WS_MINIMIZE = 0x20000000;
        public const int WS_MAXIMIZE = 0x01000000;
        public const int WS_SYSMENU = 0x00080000;

        // 常量用于SetWindowPos函数
        public static readonly IntPtr HWND_TOP = new(0);
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOZORDER = 0x0004;

        // 定义事件常量
        public const uint EVENT_OBJECT_NAMECHANGE = 0x800C;
        public const uint WINEVENT_OUTOFCONTEXT = 0;

        public const int WM_GETICON = 0x007F;
        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public const int ICON_SMALL2 = 2;

        public const int WH_GETMESSAGE = 3;
        public const int WH_CALLWNDPROC = 4;

        public enum CommonControls : uint
        {
            ICC_LISTVIEW_CLASSES = 0x00000001, // listview, header
            ICC_TREEVIEW_CLASSES = 0x00000002, // treeview, tooltips
            ICC_BAR_CLASSES = 0x00000004, // toolbar, statusbar, trackbar, tooltips
            ICC_TAB_CLASSES = 0x00000008, // tab, tooltips
            ICC_UPDOWN_CLASS = 0x00000010, // updown
            ICC_PROGRESS_CLASS = 0x00000020, // progress
            ICC_HOTKEY_CLASS = 0x00000040, // hotkey
            ICC_ANIMATE_CLASS = 0x00000080, // animate
            ICC_WIN95_CLASSES = 0x000000FF,
            ICC_DATE_CLASSES = 0x00000100, // month picker, date picker, time picker, updown
            ICC_USEREX_CLASSES = 0x00000200, // comboex
            ICC_COOL_CLASSES = 0x00000400, // rebar (coolbar) control
            ICC_INTERNET_CLASSES = 0x00000800,
            ICC_PAGESCROLLER_CLASS = 0x00001000, // page scroller
            ICC_NATIVEFNTCTL_CLASS = 0x00002000, // native font control
            ICC_STANDARD_CLASSES = 0x00004000,
            ICC_LINK_CLASS = 0x00008000
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
        public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO pIconInfo);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

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

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
