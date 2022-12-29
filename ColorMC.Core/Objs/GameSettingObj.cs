namespace ColorMC.Core.Objs;

public enum Loaders
{
    Normal, Forge, Fabric, Quilt
}

public record ServerObj
{
    public string IP { get; set; }
    public ushort Port { get; set; }
}

public record ProxyHostObj
{
    public string IP { get; set; }
    public ushort Port { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
}

public record GameSettingObj
{
    public string DirName { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public Loaders Loader { get; set; }
    public string LoaderVersion { get; set; }
    public JvmArgObj JvmArg { get; set; }
    public WindowSettingObj Window { get; set; }
    public ServerObj StartServer { get; set; }
    public ProxyHostObj ProxyHost { get; set; }
    public string AdvencedGameArguments { get; set; }
    public bool ModPack { get; set; }
}
