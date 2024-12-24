using System;
using System.Collections.Generic;

namespace ColorMC.Gui.Objs;

public record FlyoutMenuObj(string name, bool enable, Action? action)
{
    public string Name => name;
    public bool Enable => enable;
    public Action? Action => action;
    public ICollection<FlyoutMenuObj>? SubItem;
}
