using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Game;

public record ForgeInstallObj
{
    public record Logging
    {

    }
    public record Libraries
    {
        public record Downloads
        {
            public record Artifact
            {
                public string path { get; set; }
                public string url { get; set; }
                public string sha1 { get; set; }
                public long size { get; set; }
            }
            public Artifact artifact { get; set; }
        }
        public string name { get; set; }
        public Downloads downloads { get; set; }
    }
    public List<string> _comment_ { get; set; }
    public string id { get; set; }
    public string time { get; set; }
    public string releaseTime { get; set; }
    public string type { get; set; }
    public string mainClass { get; set; }
    public string inheritsFrom { get; set; }
    public Logging logging { get; set; }
    public string minecraftArguments { get; set; }
    public List<Libraries> libraries { get; set; }
}
