namespace ColorMC.Gui.Objs.ColorMC;

/// <summary>
/// 映射共享
/// </summary>
public record ColorMCCloudShareObj
{
    public string Version { get; set; }
    public string Text { get; set; }
    public bool IsLoader { get; set; }
    public int Loader { get; set; }
}
