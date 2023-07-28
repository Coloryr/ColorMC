using Newtonsoft.Json;

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
    public int formatVersion { get; set; }
    public string game { get; set; }
    public string versionId { get; set; }
    public string name { get; set; }
    public string summary { get; set; }
    public List<File> files { get; set; }
    public Dictionary<string, string> dependencies { get; set; }
}
