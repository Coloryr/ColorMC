namespace ColorMC.Gui.UI.Model.Items;

public partial class WebPicModel(string name, string description, string? logo) : PicModel(name, logo, false)
{
    /// <summary>
    /// 描述
    /// </summary>
    public string Description => description;
}
