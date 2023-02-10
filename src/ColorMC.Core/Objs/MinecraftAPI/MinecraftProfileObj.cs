namespace ColorMC.Core.Objs.MoJang;

/// <summary>
/// 账户信息
/// </summary>
/// <value></value>
public record MinecraftProfileObj
{
    public record MinecraftSkin
    {
        public string id { get; set; }
        public string state { get; set; }
        public string url { get; set; }
        public string variant { get; set; }
        public string alias { get; set; }
    }

    public record Capes
    {
        public string id { get; set; }
        public string state { get; set; }
        public string url { get; set; }
        public string alias { get; set; }
    }
    public string id { get; set; }
    public string name { get; set; }
    public List<MinecraftSkin> skins { get; set; }
    public List<Capes> capes { get; set; }
}
