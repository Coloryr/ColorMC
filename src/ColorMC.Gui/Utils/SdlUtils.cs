using System;
using ColorMC.Core.Utils;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Objs;
using Silk.NET.SDL;

namespace ColorMC.Gui.Utils;

public static class SdlUtils
{
    public static bool SdlInit { get; private set; }
    public static Sdl Sdl { get; private set; }

    /// <summary>
    /// 初始化SDL
    /// </summary>
    public static void Init()
    {
        if (ColorMCGui.RunType == RunType.Program)
        {
            try
            {
                Sdl = Sdl.GetApi();
                var config = GuiConfigUtils.Config.Input.Disable;
                if (config)
                {
                    if (Sdl.Init(Sdl.InitAudio) == 0)
                    {
                        SdlInit = true;
                    }
                }
                else
                {
                    if (Sdl.Init(Sdl.InitGamecontroller | Sdl.InitAudio) == 0)
                    {
                        JoystickInput.Init(Sdl);
                        SdlInit = true;
                    }
                }
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("BaseBinding.Error1"), e);
            }
        }
    }
#if DEBUG
    public static void InitTest()
    {
        try
        {
            Sdl = Sdl.GetApi();
            if (Sdl.Init(Sdl.InitGamecontroller | Sdl.InitAudio) == 0)
            {
                JoystickInput.Init(Sdl);
                SdlInit = true;
            }
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("BaseBinding.Error1"), e);
        }
    }
#endif
}
