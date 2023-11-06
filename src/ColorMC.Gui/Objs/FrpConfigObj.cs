namespace ColorMC.Gui.Objs;

public record SakuraFrpObj
{
    public string Key { get; set; }
}

public record FrpConfigObj
{
    public SakuraFrpObj SakuraFrp { get; set; }
}
