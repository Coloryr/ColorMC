using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthPackObj
{
    public record File
    {
        public record Hash
        { 
            public string sha1 { get; set; }
            public string sha256 { get; set; }
        }
        public string path { get; set; }
        public Hash hashes { get; set; }
        public List<string> downloads { get; set; }
        public long fileSize { get; set; }
    }
    public record Dependencies
    {
        public string minecraft { get; set; }
        [JsonProperty("fabric-loader")]
        public string fabric_loader { get; set; }
        public string forge { get; set; }
        [JsonProperty("quilt-loader")]
        public string quilt_loader { get; set; }
    }
    public int formatVersion { get; set; }
    public string game { get; set; }
    public string versionId { get; set; }
    public string name { get; set; }
    public string summary { get; set; }
    public List<File> files { get; set; }
    public Dependencies dependencies { get; set; }
}
