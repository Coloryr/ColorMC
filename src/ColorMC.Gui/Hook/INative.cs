using System;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.Hook;

public interface INative
{
    void AddHook(IntPtr id);
    void Stop();
    void SendMouse(double cursorX, double cursorY, bool message);
    void SendKey(InputKeyObj key, bool down, bool message);
    void SendScoll(bool up);
    IntPtr GetHandel();
}