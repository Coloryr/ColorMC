using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Java;

public record AdoptiumObj
{
    public record BinaryObj
    {
        public record PackageObj
        {
            [JsonProperty("checksum")]
            public string Checksum { get; set; }
            //public string checksum_link { get; set; }
            //public int download_count { get; set; }
            [JsonProperty("link")]
            public string Link { get; set; }
            //public string metadata_link { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            //public string signature_link { get; set; }
            [JsonProperty("size")]
            public long Size { get; set; }
        }
        [JsonProperty("architecture")]
        public string Architecture { get; set; }
        //public int download_count { get; set; }
        //public string heap_size { get; set; }
        [JsonProperty("image_type")]
        public string ImageType { get; set; }
        //public string jvm_impl { get; set; }
        [JsonProperty("os")]
        public string Os { get; set; }
        [JsonProperty("package")]
        public PackageObj Package { get; set; }
        //public string project { get; set; }
        [JsonProperty("scm_ref")]
        public string ScmRef { get; set; }
        //public string updated_at { get; set; }
    }
    public record VersionObj
    {
        //public int build { get; set; }
        //public int major { get; set; }
        //public int minor { get; set; }
        [JsonProperty("openjdk_version")]
        public string OpenjdkVersion { get; set; }
        //public int security { get; set; }
        //public string semver { get; set; }
    }
    [JsonProperty("binary")]
    public BinaryObj Binary { get; set; }
    //public string release_link { get; set; }
    //public string release_name { get; set; }
    //public string vendor { get; set; }
    [JsonProperty("version")]
    public VersionObj Version { get; set; }
}

public record AdoptiumJavaVersionObj
{
    [JsonProperty("available_releases")]
    public List<int> AvailableReleases { get; set; }
}