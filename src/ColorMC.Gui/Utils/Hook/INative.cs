using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Objs;
using System;
using System.Diagnostics;

namespace ColorMC.Gui.Utils.Hook;

public interface INative
{
    void AddHook(Process id);
    bool GetWindowSize(out int width, out int height);
    void Stop();
    void SendMouse(double cursorX, double cursorY, bool message);
    void SendKey(InputKeyObj key, bool down, bool message);
    void SendScoll(bool up);
    IntPtr GetHandel();
}