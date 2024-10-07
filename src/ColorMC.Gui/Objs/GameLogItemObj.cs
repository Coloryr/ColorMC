namespace ColorMC.Gui.Objs;

public record GameLogItemObj
{
    public string Log { get; init; }
    public string Thread { get; init; }
    public string Time { get; init; }
    public string Category { get; init; }
    public LogLevel Level { get; init; }
}

