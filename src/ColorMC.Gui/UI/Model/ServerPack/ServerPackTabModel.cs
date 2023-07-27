using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Model.ServerPack;

public abstract partial class ServerPackTabModel : BaseModel
{
    public ServerPackObj Obj { get; }

    public ServerPackTabModel(IUserControl con, ServerPackObj obj) : base(con)
    {
        Obj = obj;
    }
}
