using System.Collections.Generic;

namespace ColorMC.Gui.Objs;

public enum ViewType
{
    Button, Label, ServerMotd,
    GameItem, UsearHead, StackPanel, Grid
}

public record ViewObj
{
    public string Type { get; set; }
    public string Content { get; set; }
    public string VerticalAlignment { get; set; }
    public string HorizontalAlignment { get; set; }
    public string Funtion { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Margin { get; set; }
    public string Background { get; set; }
    public string Foreground { get; set; }
    public List<ViewObj> Views { get; set; }
}

public record UIObj
{
    public string Title { get; set; }
    public List<ViewObj> Views { get; set; }
}