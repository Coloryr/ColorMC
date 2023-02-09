using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Java;

public  record OpenJ9Obj
{
    public record Results
    { 
        public string content { get; set; }
        public string pagepost_custom_js_value { get; set; }
    }
    public int code { get; set; }
    public bool error { get; set; }
    public List<Results> results { get; set; }
}
