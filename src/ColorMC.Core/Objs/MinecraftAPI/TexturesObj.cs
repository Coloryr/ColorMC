namespace ColorMC.Core.Objs.MinecraftAPI;

public record TexturesObj
{
    public record Textures
    {
        public record Skin
        {
            public record Metadata
            {
                public string model { get; set; }
            }
            public string url { get; set; }
            public Metadata metadata { get; set; }
        }
        public record Cape
        {
            public string url { get; set; }
        }
        public Skin SKIN { get; set; }
        public Cape CAPE { get; set; }
    }
    public string timestamp { get; set; }
    public string profileId { get; set; }
    public string profileName { get; set; }
    public bool signatureRequired { get; set; }
    public Textures textures { get; set; }
}
