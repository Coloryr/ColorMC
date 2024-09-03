﻿using System;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.MusicPlayer;
using ColorMC.Gui.Objs;
using Silk.NET.SDL;

namespace ColorMC.Gui.Utils;

public static class SdlUtils
{
    public static bool SdlInit { get; private set; }

    public static void Init()
    {
        if (ColorMCGui.RunType == RunType.Program && SystemInfo.Os != OsType.Android)
        {
            try
            {
                var sdl = Sdl.GetApi();
                if (sdl.Init(Sdl.InitGamecontroller | Sdl.InitAudio) == 0)
                {
                    JoystickInput.Init(sdl);
                    Media.Init(sdl);
                    SdlInit = true;
                }
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("BaseBinding.Error1"), e);
            }
        }
    }
}
