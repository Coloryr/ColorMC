namespace ColorMC.Core.Objs.McMod;

public record McModSearchItemObj
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string Icon { get; set; }
    public string Text { get; set; }
    public string Time { get; set; }

    public FileType Type;
}
