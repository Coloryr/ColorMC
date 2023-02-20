using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Gui.Objs;

public record ShaderpackDisplayObj
{
    public string Name { get; set; }
    public string Local { get; set; }

    public ShaderpackObj Shaderpack { get; set; }
}
