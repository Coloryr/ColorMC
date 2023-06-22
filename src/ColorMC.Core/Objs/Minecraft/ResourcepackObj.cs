using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Minecraft;

public record ResourcepackObj
{
    public string description { get; set; }
    public int pack_format { get; set; }

    public string Sha1 { get; set; }
    public string Local { get; set; }
    public byte[] Icon { get; set; }
    public bool Broken { get; set; }
}
