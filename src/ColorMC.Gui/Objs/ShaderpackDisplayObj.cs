using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 光影包显示
/// </summary>
public record ShaderpackDisplayObj
{
    public string Name { get; set; }
    public string Local { get; set; }

    public ShaderpackObj Shaderpack { get; set; }
}
