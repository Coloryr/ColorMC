namespace ColorMC.Core.Objs;

public enum Loaders
{
    Normal, Forge, Fabric
}

public record LoaderInfo
{
    public string Name { get; set; }
    public string Version { get; set; }
}

public record GameSettingObj
{
    public string Dir { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public Loaders Loader { get; set; }
    public LoaderInfo LoaderInfo { get; set; }
    public JvmArgObj JvmArg { get; set; }
}
