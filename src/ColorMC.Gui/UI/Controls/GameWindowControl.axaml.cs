using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Windows;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

public partial class GameWindowControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title { get; set; }

    public string UseName { get; set; }

    private readonly IntPtr _handel;
    private readonly INativeControl? _implementation;
    private EmbedControl _control;

    public GameWindowControl()
    {
        InitializeComponent();
    }

    public GameWindowControl(Process process, IntPtr handel) : this()
    {
        _handel = handel;

        process.Exited += Process_Exited;

        if (SystemInfo.Os == OsType.Windows)
        {
            _implementation = new Win32NativeControl();
        }
    }

    private void Process_Exited(object? sender, EventArgs e)
    {
        Window.Close();
    }

    public void Opened()
    {
        if (_implementation.GetWindowSize(_handel, out var width, out var height))
        {
            Window.SetSize(width, height);
        }
        _implementation.TitleChange += TitleChange;
        Window.SetTitle(_implementation.GetWindowTitle(_handel));
        if (_implementation.GetIcon(_handel) is { } icon)
        {
            Window.SetIcon(icon);
        }
        _control = new(this)
        {
            Margin = new(1, 0, 1, 1)
        };
        Content = _control;
    }

    private void TitleChange(string title)
    {
        Window.SetTitle(title);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
    }

    public void WindowStateChange(WindowState state) 
    {
        _implementation.SetWindowState(_handel, state);
    }

    public void SetBaseModel(BaseModel model)
    {
        
    }

    public Task<bool> Closing()
    {
        Win32.SendMessage(_handel, Win32.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

        return Task.FromResult(false);
    }

    public class EmbedControl(GameWindowControl window) : NativeControlHost
    {
        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            return window._implementation?.CreateControl(window._handel) ?? base.CreateNativeControlCore(parent);
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            base.DestroyNativeControlCore(control);
        }
    }
}

public interface INativeControl
{
    Bitmap? GetIcon(IntPtr hWnd);
    event Action<string>? TitleChange;
    string GetWindowTitle(IntPtr hWnd);
    void AddHook(IntPtr handel);
    void SetWindowState(IntPtr handel, WindowState state);
    bool GetWindowSize(IntPtr handel, out int width, out int height);
    void NoBorder(IntPtr handel);
    IPlatformHandle CreateControl(IntPtr handel);
}

public class Win32NativeControl : INativeControl
{
    private IntPtr hWinEventHook;
    private Win32.WinEventDelegate winEventDelegate;

    public event Action<string>? TitleChange;

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
        return new Win32WindowControlHandle(handel);
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

    public void AddHook(IntPtr handel)
    {
        winEventDelegate = new Win32.WinEventDelegate(WinEventProc);
        hWinEventHook = Win32.SetWinEventHook(Win32.EVENT_OBJECT_NAMECHANGE, Win32.EVENT_OBJECT_NAMECHANGE, 
            IntPtr.Zero, winEventDelegate, 0, 0, Win32.WINEVENT_OUTOFCONTEXT);
    }

    public string GetWindowTitle(IntPtr hWnd)
    {
        int length = Win32.GetWindowTextLength(hWnd);
        var sb = new StringBuilder(length + 1);
        Win32.GetWindowText(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }

    // 事件处理回调函数
    private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, 
        int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        if (eventType == Win32.EVENT_OBJECT_NAMECHANGE)
        {
            var title = GetWindowTitle(hwnd);
            TitleChange?.Invoke(title);
        }
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
}

internal class Win32WindowControlHandle(IntPtr handle) 
    : PlatformHandle(handle, "HWND"), INativeControlHostDestroyableControlHandle
{
    public void Destroy()
    {
        Win32.DestroyWindow(Handle);
    }
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

    // WinEventHook回调函数的委托定义
    public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

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
    public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

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
}