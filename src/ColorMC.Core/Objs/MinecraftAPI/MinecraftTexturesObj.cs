using Newtonsoft.Json;

namespace ColorMC.Core.Objs.MinecraftAPI;

public record MinecraftTexturesObj
{
    public record TexturesObj
    {
        public record SkinObj
        {
            public record MetadataObj
            {
                [JsonProperty("model")]
                public string Model { get; set; }
            }
            [JsonProperty("url")]
            public string Url { get; set; }
            [JsonProperty("metadata")]
            public MetadataObj Metadata { get; set; }
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
