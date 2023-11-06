using ColorMC.Core.Objs.Minecraft;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class DataPackModel : ObservableObject
{
    public readonly DataPackObj Pack;

    public bool? Enable => Pack.Enable;
    public string Name => Pack.Name;
    public string Description => Pack.Description;
    public int PackFormat => Pack.PackFormat;

    public DataPackModel(DataPackObj obj)
    {
        Pack = obj;
    }
}
