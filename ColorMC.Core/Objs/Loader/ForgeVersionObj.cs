using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Loader;

public record ForgeVersionObj
{
    public string Version { get; set; }
    public string Build { get; set; }
}

public record ForgeVersionObj1
{
    public record Files
    { 
        public string format { get; set; }
        public string category { get; set; }
        public string hash { get; set; }
    }
    public int build { get; set; }
    public List<Files> files { get; set; }
    public string mcversion { get; set; }
    public string modified { get; set; }
    public string version { get; set; }
}