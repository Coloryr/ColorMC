namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthVersionObj
{
    public record File
    {
        public record Hashe
        {
            public string sha1 { get; set; }
            public string sha512 { get; set; }
        }
        public Hashe hashes { get; set; }
        public string url { get; set; }
        public string filename { get; set; }
        public bool primary { get; set; }
        public long size { get; set; }
    }
    public record Dependencie
    {
        public string version_id { get; set; }
        public string project_id { get; set; }
        public string file_name { get; set; }
        public string dependency_type { get; set; }
    }
    public string id { get; set; }
    public string project_id { get; set; }
    public string iauthor_idd { get; set; }
    public bool featured { get; set; }
    public string name { get; set; }
    public string version_number { get; set; }
    public string date_published { get; set; }
    public int downloads { get; set; }
    public string version_type { get; set; }
    public string status { get; set; }
    public List<File> files { get; set; }
    public List<Dependencie> dependencies { get; set; }
}
