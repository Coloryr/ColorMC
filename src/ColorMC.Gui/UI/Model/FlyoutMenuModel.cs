using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Model;

public class FlyoutMenuModel(string name, bool enable, Action? action)
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name => name;
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable => enable;
    /// <summary>
    /// 操作
    /// </summary>
    public Action? Action => action;

    /// <summary>
    /// 子项目
    /// </summary>
    public ICollection<FlyoutMenuModel>? SubItem;
}
