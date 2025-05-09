using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.MinecraftAPI;

public record MinecraftTexturesObj
{
    public record TexturesObj
    {
        public record SkinObj
        {
            public record MetadataObj
            {
                [JsonPropertyName("model")]
                public string Model { get; set; }
            }
            [JsonPropertyName("url")]
            public string Url { get; set; }
            [JsonPropertyName("metadata")]
            public MetadataObj Metadata { get; set; }
        }
        public record CapeObj
        {
            [JsonPropertyName("url")]
            public string Url { get; set; }
        }
        [JsonPropertyName("SKIN")]
        public SkinObj Skin { get; set; }
        [JsonPropertyName("CAPE")]
        public CapeObj Cape { get; set; }
    }
    //public string timestamp { get; set; }
    //public string profileId { get; set; }
    //public string profileName { get; set; }
    //public bool signatureRequired { get; set; }
    [JsonPropertyName("textures")]
    public TexturesObj Textures { get; set; }
}
