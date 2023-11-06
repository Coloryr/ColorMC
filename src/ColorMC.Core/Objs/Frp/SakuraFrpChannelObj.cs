namespace ColorMC.Core.Objs.Frp;

public record SakuraFrpChannelObj
{
    public int id { get; set; }
    public string name { get; set; }
    public bool online { get; set; }
    public string type { get; set; }
    public int remote { get; set; }
}
