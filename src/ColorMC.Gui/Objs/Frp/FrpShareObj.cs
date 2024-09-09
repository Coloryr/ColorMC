namespace ColorMC.Gui.Objs.Frp;

public record FrpShareObj
{
    public string Version { get; set; }
    public string Text { get; set; }
    public bool IsLoader { get; set; }
    public int Loader { get; set; }
}
