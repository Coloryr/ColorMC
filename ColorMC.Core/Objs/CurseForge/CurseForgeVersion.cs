namespace ColorMC.Core.Objs.CurseForge;
public record CurseForgeVersion
{
    public record Item
    {
        public int type { get; set; }
        public List<string> versions { get; set; }
    }

    public List<Item> data { get; set; }
}

public record CurseForgeVersionType
{
    public record Item
    {
        public int id { get; set; }
        public int gameId { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public List<Item> data { get; set; }
}