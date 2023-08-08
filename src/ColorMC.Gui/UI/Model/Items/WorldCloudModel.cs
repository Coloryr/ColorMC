using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Model.Items;

public partial class WorldCloudModel : BaseModel
{
    public WorldObj World { get; init; }

    public WorldCloudModel(IUserControl con, WorldObj world) : base(con)
    {
        World = world;
    }


}
