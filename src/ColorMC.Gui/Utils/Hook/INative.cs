using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ColorMC.Gui.Objs;
using System;

namespace ColorMC.Gui.Utils.Hook;

public interface INative
{
    event Action<string>? TitleChange;
    event Action<bool>? IsFocus;
    void AddHook(uint id, IntPtr handel);
    void AddHookTop(IntPtr top);
    void SetWindowState(WindowState state);
    Bitmap? GetIcon();
    string GetWindowTitle();
    bool GetWindowSize(out int width, out int height);
    void NoBorder();
    IPlatformHandle CreateControl();
    void TransferTop();
    void NoTranferTop();
    void Close();
    void DestroyWindow();
    void Stop();
    void SendMouse(double cursorX, double cursorY, bool message);
    bool GetMouseMode();
    void SendKey(InputKeyObj key, bool down, bool message);
    void SendScoll(int count, bool up);
}