using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Java;

public record DragonwellObj
{
    public record Item
    { 
        public string version8 { get; set; }
        public string xurl8 { get; set; }
        public string aurl8 { get; set; }
        public string wurl8 { get; set; }
        public string type8 { get; set; }
        public string version11 { get; set; }
        public string xurl11 { get; set; }
        public string aurl11 { get; set; }
        public string apurl11 { get; set; }
        public string wurl11 { get; set; }
        public string rurl11 { get; set; }
        public string type11 { get; set; }
        public string version17 { get; set; }
        public string xurl17 { get; set; }
        public string aurl17 { get; set; }
        public string apurl17 { get; set; }
        public string wurl17 { get; set; }
        public string type17 { get; set; }
    }

    public Item extended { get; set; }
    public Item standard { get; set; }
}
