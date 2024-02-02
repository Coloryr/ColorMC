using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Objs;
using System;
using System.Diagnostics;

namespace ColorMC.Gui.Utils.Hook;

public interface INative
{
    event Action<string>? TitleChange;
    event Action<bool>? IsFocus;
    void AddHook(Process id);
    void AddHookTop(IntPtr top);
    void SetWindowState(WindowState state);
    Bitmap? GetIcon();
    string GetWindowTitle();
    bool GetWindowSize(out int width, out int height);
    void NoBorder();
    void TransferTop();
    void NoTranferTop();
    void Close();
    void DestroyWindow();
    void Stop();
    void SendMouse(double cursorX, double cursorY, bool message);
    bool GetMouseMode();
    void SendKey(InputKeyObj key, bool down, bool message);
    void SendScoll(bool up);
    IntPtr GetHandel();
}