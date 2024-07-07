using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Setting;

namespace ColorMC.Gui.UI.Model.Items;

public record LockLoginModel(SettingModel top, LockLoginSetting login)
{ 
    public string Name => login.Name;
    public string Data => login.Data;
    public string Type => login.Type.GetName();

    public AuthType AuthType => login.Type;

    public void Delete()
    {
        top.Delete(this);
    }
}
