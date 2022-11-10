using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Game;

public record GameArgObj
{
    public record Arguments
    {
        public List<JObject> game { get; set; }
        public List<JObject> jvm { get; set; }
    }

    public record AssetIndex
    {
        public string id { get; set; }
        public string sha1 { get; set; }
        public long size { get; set; }
        public long totalSize { get; set; }
        public string url { get; set; }
    }
    public record Download
    {
        public string sha1 { get; set; }
        public long size { get; set; }
        public string url { get; set; }
    }
    public record Downloads
    {
        public Download client { get; set; }
        public Download client_mappings { get; set; }
        public Download server { get; set; }
        public Download server_mappings { get; set; }
    }

    public record JavaVersion
    {
        public string component { get; set; }
        public int majorVersion { get; set; }
    }
    public record Downloads1
    {
        public Artifact artifact { get; set; }
    }
    public record Artifact
    {
        public string path { get; set; }
        public string sha1 { get; set; }
        public long size { get; set; }
        public string url { get; set; }
    }
    public record OS
    { 
        public string name { get; set; }
        public string version { get; set; }
    }
    public record Rules
    { 
        public string action { get; set; }
        public OS os { get; set; }
    }
    public record Libraries
    { 
        public Downloads1 downloads { get; set; }
        public string name { get; set; }
        public List<Rules> rules { get; set; }
    }
    public record File
    { 
        public string id { get; set; }
        public string sha1 { get; set; }
        public long size { get; set; }
        public string url { get; set; }
    }
    public record Client1
    { 
        public string argument { get; set; }
        public File file { get; set; }
        public string type { get; set; }
    }
    public record Logging
    { 
        public Client1 client { get; set; }
    }

    public Arguments arguments { get; set; }
    public AssetIndex assetIndex { get; set; }
    public string assets { get; set; }
    public int complianceLevel { get; set; }
    public Downloads downloads { get; set; }
    public string id { get; set; }
    public JavaVersion javaVersion { get; set; }
    public List<Libraries> libraries { get; set; }
    public Logging logging { get; set; }
    public string mainClass { get; set; }
    public int minimumLauncherVersion { get; set; }
    public string releaseTime { get; set; }
    public string time { get; set; }
    public string type { get; set; }
}
