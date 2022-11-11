using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Pack;

public record CurseForgePackObj
{
    public record Minecraft
    {
        public record ModLoaders
        { 
            public string id { get; set; }
            public bool primary { get; set; }
        }
        public string version { get; set; }
        public List<ModLoaders> modLoaders  { get;set;}
    }
    public record Files
    { 
        public int projectID { get; set; }
        public int fileID { get; set; }
        public bool required { get; set; }
    }
    public Minecraft minecraft { get; set; }
    public string manifestType { get; set; }
    public int manifestVersion { get; set; }
    public string name { get; set; }
    public string version { get; set; }
    public string author { get; set; }
    public List<Files> files { get; set; }
    public string overrides { get; set; }
}
