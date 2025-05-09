using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Java;

public record AdoptiumObj
{
    public record BinaryObj
    {
        public record PackageObj
        {
            [JsonPropertyName("checksum")]
            public string Checksum { get; set; }
            //public string checksum_link { get; set; }
            //public int download_count { get; set; }
            [JsonPropertyName("link")]
            public string Link { get; set; }
            //public string metadata_link { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
            //public string signature_link { get; set; }
            [JsonPropertyName("size")]
            public long Size { get; set; }
        }
        [JsonPropertyName("architecture")]
        public string Architecture { get; set; }
        //public int download_count { get; set; }
        //public string heap_size { get; set; }
        [JsonPropertyName("image_type")]
        public string ImageType { get; set; }
        //public string jvm_impl { get; set; }
        [JsonPropertyName("os")]
        public string Os { get; set; }
        [JsonPropertyName("package")]
        public PackageObj Package { get; set; }
        //public string project { get; set; }
        [JsonPropertyName("scm_ref")]
        public string ScmRef { get; set; }
        //public string updated_at { get; set; }
    }
    public record AdoptiumVersionObj
    {
        //public int build { get; set; }
        //public int major { get; set; }
        //public int minor { get; set; }
        [JsonPropertyName("openjdk_version")]
        public string OpenjdkVersion { get; set; }
        //public int security { get; set; }
        //public string semver { get; set; }
    }
    [JsonPropertyName("binary")]
    public BinaryObj Binary { get; set; }
    //public string release_link { get; set; }
    //public string release_name { get; set; }
    //public string vendor { get; set; }
    [JsonPropertyName("version")]
    public AdoptiumVersionObj Version { get; set; }
}

public record AdoptiumJavaVersionObj
{
    [JsonPropertyName("available_releases")]
    public List<int> AvailableReleases { get; set; }
}