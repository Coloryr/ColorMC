using System;
using System.Collections.Generic;
using System.Text;
using ColorMC.Core.Objs;

namespace ColorMC.Core.GuiHandle;

/// <summary>
/// 整合包安装回调
/// </summary>
public interface IModPackGui : IProgressGui
{
    /// <summary>
    /// 设置整合包安装状态
    /// </summary>
    /// <param name="state"></param>
    void SetStateText(ModpackState state);
    /// <summary>
    /// 设置当前进度
    /// </summary>
    /// <param name="value"></param>
    /// <param name="all"></param>
    void SetNow(int value, int all);
    /// <summary>
    /// 显示文件
    /// </summary>
    /// <param name="text"></param>
    void SetText(string? text);
    /// <summary>
    /// 子进度
    /// </summary>
    /// <param name="value"></param>
    /// <param name="all"></param>
    void SetNowSub(int value, int all);
}
