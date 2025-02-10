//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Avalonia.Input;
//using ColorMC.Gui.Objs;

//namespace ColorMC.Gui.Hook;

//internal class MacNative : INative
//{
//    public void AddHook(nint id)
//    {
        
//    }

//    public void SendKey(InputKeyObj key, bool down)
//    {
//        if (key.MouseButton != MouseButton.None)
//        {
//            HandleMouseButtonEvent(key, down);
//        }
//        else
//        {
//            HandleKeyboardEvent(key, down);
//        }
//    }

//    private void HandleKeyboardEvent(InputKeyObj key, bool down)
//    {
//        var keyCode = KeyToCGKeyCode(key.Key);
//        var source = CGEventSource.Create(CGEventSourceStateID.CombinedSessionState);
//        if (source == null)
//        {
//            // 处理无法创建事件源的情况
//            return;
//        }

//        var keyEvent = new CGEvent(source, keyCode, down);
//        keyEvent.Flags = GetModifiersFlags(key.KeyModifiers);
//        keyEvent.Post(CGEventTapLocation.HID);
//        keyEvent.Dispose();
//        source.Dispose();
//    }

//    private void HandleMouseButtonEvent(InputKeyObj key, bool down)
//    {
//        var button = key.MouseButton;
//        var eventType = GetMouseEventType(button, down);
//        var source = CGEventSource.Create(CGEventSourceStateID.CombinedSessionState);
//        if (source == null)
//            return;

//        // 获取当前鼠标位置
//        var currentMousePos = new CGEvent(NSEvent.CurrentEvent).MouseLocation;
//        var mouseEvent = new CGEvent(source, eventType, currentMousePos, (CGMouseButton)button);
//        mouseEvent.Flags = GetModifiersFlags(key.KeyModifiers);
//        mouseEvent.Post(CGEventTapLocation.HID);
//        mouseEvent.Dispose();
//        source.Dispose();
//    }

//    private CGEventFlags GetModifiersFlags(KeyModifiers modifiers)
//    {
//        CGEventFlags flags = 0;
//        if (modifiers.HasFlag(KeyModifiers.Shift))
//            flags |= CGEventFlags.MaskShift;
//        if (modifiers.HasFlag(KeyModifiers.Control))
//            flags |= CGEventFlags.MaskControl;
//        if (modifiers.HasFlag(KeyModifiers.Alt))
//            flags |= CGEventFlags.MaskAlternate;
//        if (modifiers.HasFlag(KeyModifiers.Command))
//            flags |= CGEventFlags.MaskCommand;
//        return flags;
//    }

//    private CGEventType GetMouseEventType(MouseButton button, bool down)
//    {
//        switch (button)
//        {
//            case MouseButton.Left:
//                return down ? CGEventType.LeftMouseDown : CGEventType.LeftMouseUp;
//            case MouseButton.Right:
//                return down ? CGEventType.RightMouseDown : CGEventType.RightMouseUp;
//            case MouseButton.Middle:
//                return down ? CGEventType.OtherMouseDown : CGEventType.OtherMouseUp;
//            default:
//                return CGEventType.Null;
//        }
//    }

//    public void SendMouse(double cursorX, double cursorY)
//    {
//        // 获取当前鼠标位置
//        IntPtr currentEvent = NativeMethods.CGEventCreateMouseEvent(IntPtr.Zero, NativeMethods.kCGEventMouseMoved, new NativeMethods.CGPoint(), 0);
//        NativeMethods.CGPoint currentPoint = NativeMethods.CGEventGetLocation(currentEvent);
//        NativeMethods.CFRelease(currentEvent);

//        // 计算新的鼠标位置
//        NativeMethods.CGPoint newPoint = new NativeMethods.CGPoint
//        {
//            X = currentPoint.X + cursorX,
//            Y = currentPoint.Y + cursorY
//        };

//        // 创建鼠标移动事件
//        IntPtr eventRef = NativeMethods.CGEventCreateMouseEvent(IntPtr.Zero, NativeMethods.kCGEventMouseMoved, newPoint, 0);

//        // 发送事件
//        NativeMethods.CGEventPost(NativeMethods.kCGHIDEventTap, eventRef);

//        // 释放事件
//        NativeMethods.CFRelease(eventRef);
//    }

//    public void SendScoll(bool up)
//    {
        
//    }

//    public void Stop()
//    {
        
//    }
//}
