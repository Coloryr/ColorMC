using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.FTB;

public record FTBModpackObj
{
    public record Art
    {
        public int width { get; set; }
        public int height { get; set; }
        public bool compressed { get; set; }
        public string url { get; set; }
        public List<string> mirrors { get; set; }
        public string sha1 { get; set; }
        public long size { get; set; }
        public int id { get; set; }
        public string type { get; set; }
        public long updated { get; set; }
    }
    public record Links
    {
        public int id { get; set; }
        public string name { get; set; }
        public string link { get; set; }
        public string type { get; set; }
    }
    public record Authors
    {
        public string website { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public long updated { get; set; }
    }
    public record Versions
    {
        public record Specs
        {
            public int id { get; set; }
            public int minimum { get; set; }
            public int recommended { get; set; }
        }
        public record Targets
        {
            public string version { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public long updated { get; set; }
        }
        public Specs specs { get; set; }
        public List<Targets> targets { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public long updated { get; set; }
        [JsonProperty("private")]
        public bool _private { get; set; }
    }
    public record Tags
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public record Rating
    {
        public int id { get; set; }
        public bool configured { get; set; }
        public bool verified { get; set; }
        public int age { get; set; }
        public bool gambling { get; set; }
        public bool frightening { get; set; }
        public bool alcoholdrugs { get; set; }
        public bool nuditysexual { get; set; }
        public bool sterotypeshate { get; set; }
        public bool language { get; set; }
        public bool violence { get; set; }
    }
    public string synopsis { get; set; }
    public string description { get; set; }
    public List<Art> art { get; set; }
    public List<Links> links { get; set; }
    public List<Authors> authors { get; set; }
    public List<Versions> versions { get; set; }
    public long installs { get; set; }
    public long plays { get; set; }
    public List<Tags> tags { get; set; }
    public bool featured { get; set; }
    public long refreshed { get; set; }
    public string notification { get; set; }
    public Rating rating { get; set; }
    public string status { get; set; }
    public long released { get; set; }
    public long plays_14d { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public string type { get; set; }
    public long updated { get; set; }
    [JsonProperty("private")]
    public bool _private { get; set; }
}
