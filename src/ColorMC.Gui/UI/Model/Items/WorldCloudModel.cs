using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Items;

public partial class WorldCloudModel : BaseModel
{
    public WorldObj World { get; }

    public WorldCloudModel(IUserControl con, WorldObj world) : base(con)
    {
        World = world;
    }


}
