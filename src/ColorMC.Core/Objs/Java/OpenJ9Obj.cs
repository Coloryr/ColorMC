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


public record OpenJ9Obj1
{
    public record Downloads
    {
        public record Jdk
        {
            public record Opt
            { 
                public string displayName { get; set; }
                public string downloadLink { get; set; }
                public string checksum { get; set; }
                public string sig { get; set; }
            }
            public Opt opt1 { get; set; }
        }
        public string name { get; set; }
        public int version { get; set; }
        public string os { get; set; }
        public string arch { get; set; }
        public Jdk jdk { get; set; }
        public Jdk jre { get; set; }
    }
    public List<Downloads> downloads { get; set; }
}