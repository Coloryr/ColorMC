using static ColorMC.Core.Objs.Minecraft.GameArgObj.Libraries.Downloads;

namespace ColorMC.Core.Objs.Loader;

/// <summary>
/// Forge启动数据
/// </summary>
public record ForgeLaunchObj
{
    public record Logging
    {

    }
    public record Libraries
    {
        public record Downloads
        {
            public Artifact artifact { get; set; }
        }
        public string name { get; set; }
        public Downloads downloads { get; set; }
    }
    public record Arguments
    {
        public List<string> game { get; set; }
        public List<string> jvm { get; set; }
    }
    public List<string> _comment_ { get; set; }
    public string id { get; set; }
    public string time { get; set; }
    public string releaseTime { get; set; }
    public string type { get; set; }
    public string mainClass { get; set; }
    public string inheritsFrom { get; set; }
    public Logging logging { get; set; }
    public string minecraftArguments { get; set; }
    public Arguments arguments { get; set; }
    public List<Libraries> libraries { get; set; }
}
