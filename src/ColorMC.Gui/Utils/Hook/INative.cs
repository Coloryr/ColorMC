using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ColorMC.Gui.Objs;
using System;

namespace ColorMC.Gui.Utils.Hook;

public interface INative
{
    event Action<string>? TitleChange;

    void AddHook(IntPtr handel);
    void SetWindowState(WindowState state);
    Bitmap? GetIcon();
    string GetWindowTitle();
    bool GetWindowSize(out int width, out int height);
    void NoBorder();
    IPlatformHandle CreateControl();
    void TransferEvent(IntPtr handel);
    void Close();
    void DestroyWindow();
    void Stop();
    void SendMouse(double cursorX, double cursorY, bool message);
    bool GetMouseMode();
    void SendKey(InputKeyObj key, bool down);
    void SendScoll(int count, bool up);
}