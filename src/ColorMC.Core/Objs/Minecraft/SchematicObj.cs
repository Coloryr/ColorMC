namespace ColorMC.Core.Objs.Minecraft;

public record SchematicObj
{
    public string Name { get; set; }
    public string Local { get; set; }
    public int Height { get; set; }
    public int Length { get; set; }
    public int Width { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }


    public bool Broken;
}
