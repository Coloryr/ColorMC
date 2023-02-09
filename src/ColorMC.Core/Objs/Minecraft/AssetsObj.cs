namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 资源数据
/// </summary>
public record AssetsObj
{
    public record Objects
    {
        public string hash { get; set; }
        public long size { get; set; }
    }
    public Dictionary<string, Objects> objects;
}
