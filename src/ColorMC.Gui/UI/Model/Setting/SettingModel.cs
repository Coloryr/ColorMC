using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

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
