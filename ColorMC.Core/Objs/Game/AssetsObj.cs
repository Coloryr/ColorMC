namespace ColorMC.Core.Objs.Game;

public record AssetsObj
{
    public record Objects
    {
        public string hash { get; set; }
        public long size { get; set; }
    }
    public Dictionary<string, Objects> objects;
}
