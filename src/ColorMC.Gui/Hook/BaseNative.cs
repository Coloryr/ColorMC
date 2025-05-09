using System;
using ColorMC.Gui.Objs.Config;

namespace ColorMC.Gui.Hook;

/// <summary>
/// 游戏实例与系统平台绑定
/// </summary>
/// <param name="id">进程句柄</param>
public abstract class BaseNative(IntPtr id)
{
    public IntPtr Target { get; init; } = id;

    /// <summary>
    /// 停止所有钩子
    /// </summary>
    public abstract void Stop();
    /// <summary>
    /// 发送鼠标位置
    /// </summary>
    /// <param name="cursorX">X位置</param>
    /// <param name="cursorY">Y位置</param>
    public abstract void SendMouse(double cursorX, double cursorY);
    /// <summary>
    /// 发送键盘案件
    /// </summary>
    /// <param name="key">按键值</param>
    /// <param name="down">是否按下</param>
    public abstract void SendKey(InputKeyObj key, bool down);
    /// <summary>
    /// 发送滚轮
    /// </summary>
    /// <param name="up">是否位向上</param>
    public abstract void SendScoll(bool up);
}