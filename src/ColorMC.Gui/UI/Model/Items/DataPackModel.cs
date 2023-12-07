using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Gui.UI.Model.Items;

public partial class DataPackModel(DataPackObj obj)
{
    public DataPackObj Pack => obj;

    public bool? Enable => obj.Enable;
    public string Name => obj.Name;
    public string Description => obj.Description;
    public int PackFormat => obj.PackFormat;
}
