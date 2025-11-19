namespace ColorMC.Gui.UI.Model.Items;

public partial class AuthorModel(string name, string? logo) : PicModel(name, logo, 30)
{
    public bool HaveLogo { get; init; } = logo != null;
}
