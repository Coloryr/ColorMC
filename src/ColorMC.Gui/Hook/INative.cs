using System;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.Hook;

public interface INative
{
    /// <summary>
    /// 添加程序钩子
    /// </summary>
    /// <param name="id">进程句柄</param>
    void AddHook(IntPtr id);
    /// <summary>
    /// 停止所有钩子
    /// </summary>
    void Stop();
    /// <summary>
    /// 发送鼠标位置
    /// </summary>
    /// <param name="cursorX">X位置</param>
    /// <param name="cursorY">Y位置</param>
    /// <param name="message">是否以Message发送</param>
    void SendMouse(double cursorX, double cursorY, bool message);
    /// <summary>
    /// 发送键盘案件
    /// </summary>
    /// <param name="key">按键值</param>
    /// <param name="down">是否按下</param>
    /// <param name="message">是否以Message发送</param>
    void SendKey(InputKeyObj key, bool down, bool message);
    /// <summary>
    /// 发送滚轮
    /// </summary>
    /// <param name="up">是否位向上</param>
    void SendScoll(bool up);
}