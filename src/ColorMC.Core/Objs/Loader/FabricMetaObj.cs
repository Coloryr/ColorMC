﻿using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Loader;

public record FabricMetaObj
{
    public record GameObj
    {
        [JsonProperty("version")]
        public string Version { get; set; }
        //public bool stable { get; set; }
    }
    //public record Mappings
    //{
    //    public string gameVersion { get; set; }
    //    public string separator { get; set; }
    //    public int build { get; set; }
    //    public string maven { get; set; }
    //    public string version { get; set; }
    //    public bool stable { get; set; }
    //}
    //public record Intermediary
    //{
    //    public string maven { get; set; }
    //    public string version { get; set; }
    //    public bool stable { get; set; }
    //}
    public record LoaderObj
    {
        //public string separator { get; set; }
        //public int build { get; set; }
        //public string maven { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("stable")]
        public bool Stable { get; set; }
    }
    //public record Installer
    //{
    //    public string url { get; set; }
    //    public string maven { get; set; }
    //    public string version { get; set; }
    //    public bool stable { get; set; }
    //}
    //public List<GameObj> game { get; set; }
    //public List<Mappings> mappings { get; set; }
    //public List<Intermediary> intermediary { get; set; }
    [JsonProperty("loader")]
    public List<LoaderObj> Loader { get; set; }
    //public List<Installer> installer { get; set; }
}
