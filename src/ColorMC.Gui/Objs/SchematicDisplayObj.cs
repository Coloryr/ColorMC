using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Gui.Objs;

public record SchematicDisplayObj
{
    public string Name { get; set; }
    public string Local { get; set; }
    public int Height { get; set; }
    public int Length { get; set; }
    public int Width { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }


    public SchematicObj Schematic;
}
