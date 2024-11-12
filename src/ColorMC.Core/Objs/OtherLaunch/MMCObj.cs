using Newtonsoft.Json;

namespace ColorMC.Core.Objs.OtherLaunch;

public record MMCObj
{
    public record ComponentsObj
    {
        //public record CachedRequires
        //{
        //    public string equals { get; set; }
        //    public string suggests { get; set; }
        //    public string uid { get; set; }
        //}
        //public string cachedName { get; set; }
        //public string cachedVersion { get; set; }
        //public bool cachedVolatile { get; set; }
        //public bool dependencyOnly { get; set; }
        [JsonProperty("uid")]
        public string Uid { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        //public List<CachedRequires> cachedRequires { get; set; }
        //public bool important { get; set; }
    }
    [JsonProperty("components")]
    public List<ComponentsObj> Components { get; set; }
}
