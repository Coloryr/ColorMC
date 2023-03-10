using Newtonsoft.Json;

namespace ColorMC.Core.Objs.ServerPack;

public record ServerPackObj
{
    public string MCVersion { get; set; }
    public Loaders Loader { get; set; }
    public string LoaderVersion { get; set; }
    public string Url { get; set; }
    public string Version { get; set; }
    public string UI { get; set; }
    public string Text { get; set; }
    public bool LockMod { get; set; }
    public bool ForceUpdate { get; set; }

    public List<ModItem> Mod { get; set; }
    public List<ModItem> Resourcepack { get; set; }
    public List<ConfigPackObj> Config { get; set; }

    [JsonIgnore]
    public GameSettingObj Game;
}
