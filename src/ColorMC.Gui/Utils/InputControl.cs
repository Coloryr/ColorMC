using Silk.NET.SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.Utils;

public static class InputControl
{
    private static Sdl sdl;
    public static bool IsInit { get; private set; }
    public static void Init()
    {
        sdl = Sdl.GetApi();

        IsInit = sdl.Init(Sdl.InitGamecontroller) != 0;
    }

    public static int Count => sdl.NumJoysticks();

    public static unsafe List<string> GetNames()
    {
        var list = new List<string>();
        for (int i = 0; i < Count; i++)
        {
            if (sdl.IsGameController(i) == SdlBool.True)
            {
                var gameController = sdl.GameControllerOpen(i);
                if (gameController != null)
                {
                    list.Add(BytePointerToString(sdl.GameControllerName(gameController)));
                    sdl.GameControllerClose(gameController);
                }
            }
        }

        return list;
    }

    private static unsafe string BytePointerToString(byte* bytePointer)
    {
        byte* tempPointer = bytePointer;
        while (*tempPointer != 0)
        {
            tempPointer++;
        }
        int length = (int)(tempPointer - bytePointer);
        byte[] byteArray = new byte[length];
        Marshal.Copy((IntPtr)bytePointer, byteArray, 0, length);

        string result = Encoding.Default.GetString(byteArray);
        return result;
    }

    public static unsafe GameController* Open(int index)
    {
        return sdl.GameControllerOpen(index);
    }

    public static void Close(IntPtr index)
    {
        unsafe
        {
            sdl.GameControllerClose((GameController*)index);
        }
    }

    public static unsafe Task<byte?> WaitInput(IntPtr input, CancellationToken token)
    {
        var instanceID = sdl.GameControllerGetJoystick((GameController*)input);
        var joystickID = sdl.JoystickInstanceID(instanceID);

        return Task.Run<byte?>(() =>
        {
            var sdlEvent = new Event();
            while (!token.IsCancellationRequested)
            {
                sdl.WaitEvent(&sdlEvent);
                switch (sdlEvent.Type)
                {
                    case (uint)EventType.Quit:
                        return null;
                    case (uint)EventType.Controllerbuttondown:
                        if (sdlEvent.Cbutton.Which == joystickID)
                        {
                            return sdlEvent.Cbutton.Button;
                        }
                        break;
                }
            }

            return null;
        });
    }

    public static unsafe Task<byte?> WaitAxis(IntPtr input, CancellationToken token)
    {
        var instanceID = sdl.GameControllerGetJoystick((GameController*)input);
        var joystickID = sdl.JoystickInstanceID(instanceID);

        return Task.Run<byte?>(() =>
        {
            var sdlEvent = new Event();
            while (!token.IsCancellationRequested)
            {
                sdl.WaitEvent(&sdlEvent);
                switch (sdlEvent.Type)
                {
                    case (uint)EventType.Controlleraxismotion:
                        if (sdlEvent.Caxis.Which == joystickID)
                        {
                            if (sdlEvent.Caxis.Axis == (uint)GameControllerAxis.Triggerleft ||
                                sdlEvent.Caxis.Axis == (uint)GameControllerAxis.Triggerright)
                            {
                                return sdlEvent.Caxis.Axis;
                            }
                        }
                        break;
                }
            }

            return null;
        });
    }

    public static unsafe void StartRead(Action<Event> action, IntPtr input, CancellationToken token)
    {
        var instanceID = sdl.GameControllerGetJoystick((GameController*)input);
        var joystickID = sdl.JoystickInstanceID(instanceID);

        Task.Run(() =>
        {
            var sdlEvent = new Event();
            while (!token.IsCancellationRequested)
            {
                if (sdlEvent.Type == (uint)EventType.Quit)
                {
                    return;
                }
                sdl.WaitEvent(&sdlEvent);
                if (sdlEvent.Cbutton.Which == joystickID)
                {
                    action.Invoke(sdlEvent);
                }
            }
        }, token);
    }
}
