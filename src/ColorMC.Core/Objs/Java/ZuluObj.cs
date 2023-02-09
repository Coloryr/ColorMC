using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Java;

public record ZuluObj
{
    public string abi { get; set; }
    public string arch { get; set; }
    public string bundle_type { get; set; }
    public List<object> cpu_gen { get; set; }
    public string ext { get; set; }
    public List<string> features { get; set; }
    public string hw_bitness { get; set; }
    public int id { get; set; }
    public List<int> java_version { get; set; }
    public bool javafx { get; set; }
    public List<int> jdk_version { get; set; }
    public bool latest { get; set; }
    public string name { get; set; }
    public int? openjdk_build_number { get; set; }
    public string os { get; set; }
    public string release_status { get; set; }
    public string sha256_hash { get; set; }
    public string support_term { get; set; }
    public string url { get; set; }
    public List<int> zulu_version { get; set; }
}
