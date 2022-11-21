namespace ColorMC.Core.Objs.Game;

public record ServerInfoObj
{
    public string IP { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public bool AcceptTextures { get; set; }
}
