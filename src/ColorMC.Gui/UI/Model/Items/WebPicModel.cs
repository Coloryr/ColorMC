using ColorMC.Gui.Manager;

namespace ColorMC.Gui.UI.Model.Items;

public partial class WebPicModel(string? name, string? description, string? logo) : PicModel(name, logo, 400)
{
    /// <summary>
    /// 描述
    /// </summary>
    public string? Description => description;

    public void Open()
    {
        if (Logo == null)
        {
            return;
        }
        ImageManager.Open(Logo);
    }
}
