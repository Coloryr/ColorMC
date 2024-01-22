using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Utils.Hook;

public interface INative
{
    Bitmap? GetIcon(IntPtr hWnd);
    event Action<string>? TitleChange;
    string GetWindowTitle(IntPtr hWnd);
    void AddHook(IntPtr handel);

    void SetWindowState(IntPtr handel, WindowState state);
    bool GetWindowSize(IntPtr handel, out int width, out int height);
    void NoBorder(IntPtr handel);
    IPlatformHandle CreateControl(IntPtr handel);
    void TransferEvent(IntPtr handel1);
    void Close(IntPtr handle);
}