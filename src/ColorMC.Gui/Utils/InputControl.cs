using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.SDL;
using Thread = System.Threading.Thread;

namespace ColorMC.Gui.Utils;

public static class InputControl
{
    private static Sdl sdl;
    private static bool _isRun;

    public static bool IsInit { get; private set; }
    public static event Action<Event>? OnEvent;
    public static bool IsEditMode = false;

    /// <summary>
    /// �ֱ����Ƴ�ʼ��
    /// </summary>
    public static void Init()
    {
        sdl = Sdl.GetApi();

        IsInit = sdl.Init(Sdl.InitGamecontroller) == 0;

        if (IsInit)
        {
            App.OnClose += App_OnClose;
            _isRun = true;
            new Thread(() =>
            {
                var sdlEvent = new Event();
                while (_isRun)
                {
                    sdl.WaitEvent(ref sdlEvent);
                    OnEvent?.Invoke(sdlEvent);
                }
            }).Start();
        }
    }

    private static void App_OnClose()
    {
        _isRun = false;
    }

    /// <summary>
    /// ��ȡ�ֱ�����
    /// </summary>
    public static int Count => sdl.NumJoysticks();

    /// <summary>
    /// ��ȡ�ֱ������б�
    /// </summary>
    /// <returns></returns>
    public static List<string> GetNames()
    {
        var list = new List<string>();
        for (int i = 0; i < Count; i++)
        {
            if (sdl.IsGameController(i) == SdlBool.True)
            {
                unsafe
                {
                    var gameController = sdl.GameControllerOpen(i);
                    if (gameController != null)
                    {
                        list.Add(BytePointerToString(sdl.GameControllerName(gameController)));
                        sdl.GameControllerClose(gameController);
                    }
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
        Marshal.Copy((nint)bytePointer, byteArray, 0, length);

        string result = Encoding.Default.GetString(byteArray);
        return result;
    }

    /// <summary>
    /// ���ֱ�
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static nint Open(int index)
    {
        unsafe
        {
            return new nint(sdl.GameControllerOpen(index));
        }
    }

    /// <summary>
    /// �ر��ֱ�
    /// </summary>
    /// <param name="index"></param>
    public static void Close(nint index)
    {
        unsafe
        {
            sdl.GameControllerClose((GameController*)index);
        }
    }

    /// <summary>
    /// ��ȡ�ֱ����
    /// </summary>
    /// <param name="ptr"></param>
    /// <returns></returns>
    public static unsafe int GetJoystickID(nint ptr)
    {
        var instanceID = sdl.GameControllerGetJoystick((GameController*)ptr);
        return sdl.JoystickInstanceID(instanceID);
    }
}
