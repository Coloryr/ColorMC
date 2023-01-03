namespace ColorMC.Core.Objs.OtherLaunch;

public record MMCObj
{
    public record Components
    {
        public record CachedRequires
        {
            public string equals { get; set; }
            public string suggests { get; set; }
            public string uid { get; set; }
        }
        public string cachedName { get; set; }
        public string cachedVersion { get; set; }
        public bool cachedVolatile { get; set; }
        public bool dependencyOnly { get; set; }
        public string uid { get; set; }
        public string version { get; set; }
        public List<CachedRequires> cachedRequires { get; set; }
        public bool important { get; set; }
    }
    public List<Components> components { get; set; }
}
