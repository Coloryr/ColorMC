﻿using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Objs;
using Silk.NET.SDL;
using System;

namespace ColorMC.Gui.Utils;

public static class SdlUtils
{
    public static bool SdlInit { get; private set; }
    public static Sdl Sdl { get; private set; }

    public static void Init()
    {
        if (ColorMCGui.RunType == RunType.Program && SystemInfo.Os != OsType.Android)
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
