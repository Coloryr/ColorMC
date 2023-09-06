using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Model.Items;

public partial class WorldCloudModel : TopModel
{
    public WorldObj World { get; init; }

    public WorldCloudModel(BaseModel model, WorldObj world) : base(model)
    {
        World = world;
    }

    protected override void Close()
    {

    }
}
