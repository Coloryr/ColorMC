using Newtonsoft.Json;

namespace ColorMC.Core.Objs.MinecraftAPI;

public record MinecraftTexturesObj
{
    public record TexturesObj
    {
        public record SkinObj
        {
            //public record Metadata
            //{
            //    public string model { get; set; }
            //}
            [JsonProperty("url")]
            public string Url { get; set; }
            //public Metadata metadata { get; set; }
        }
        public record CapeObj
        {
            [JsonProperty("url")]
            public string Url { get; set; }
        }
        [JsonProperty("SKIN")]
        public SkinObj Skin { get; set; }
        [JsonProperty("CAPE")]
        public CapeObj Cape { get; set; }
    }
    //public string timestamp { get; set; }
    //public string profileId { get; set; }
    //public string profileName { get; set; }
    //public bool signatureRequired { get; set; }
    [JsonProperty("textures")]
    public TexturesObj Textures { get; set; }
}
