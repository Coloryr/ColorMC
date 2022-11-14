using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Game;

public record QuiltLoaderObj
{
    public record Arguments
    {
        public List<string> game { get; set; }
    }
    public record Libraries
    {
        public string name { get; set; }
        public string url { get; set; }
    }
    public string id { get; set; }
    public string inheritsFrom { get; set; }
    public string releaseTime { get; set; }
    public string time { get; set; }
    public string type { get; set; }
    public string mainClass { get; set; }
    public Arguments arguments { get; set; }
    public List<Libraries> libraries { get; set; }
}
