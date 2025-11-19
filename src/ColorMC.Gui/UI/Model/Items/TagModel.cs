namespace ColorMC.Gui.UI.Model.Items;

public partial class TagModel(string name, string? logo, string? svg) : PicModel(name, logo, 30)
{
    public string? Svg { get; set; } = svg;
    public bool IsSvg { get; set; } = svg != null;
}
