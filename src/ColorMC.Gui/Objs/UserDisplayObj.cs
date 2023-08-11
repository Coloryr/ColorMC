using ColorMC.Core.Objs;

namespace ColorMC.Gui.Objs;

public record UserDisplayObj
{
    public bool Use { get; set; }
    public string Name { get; set; }
    public string UUID { get; set; }
    public string Type { get; set; }
    public string Text1 { get; set; }
    public string Text2 { get; set; }

    public AuthType AuthType { get; set; }
}