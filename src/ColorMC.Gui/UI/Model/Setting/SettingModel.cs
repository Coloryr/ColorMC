using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel : TopModel
{
    public SettingModel(BaseModel model) : base(model)
    {
        if (SystemInfo.Os == OsType.Linux)
        {
            _enableWindowMode = false;
        }
    }

    protected override void Close()
    {
        FontList.Clear();
        JavaList.Clear();
        _uuids.Clear();
        GameList.Clear();
    }
}
