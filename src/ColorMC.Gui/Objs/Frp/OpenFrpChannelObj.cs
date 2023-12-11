using System.Collections.Generic;

namespace ColorMC.Gui.Objs.Frp;

public record OpenFrpChannelObj
{
    public record Proxie
    {
        public string name { get; set; }
        public int id { get; set; }
        public string type { get; set; }
        public string remote { get; set; }
        public string local { get; set; }
    }
    public record Data
    {
        public string name { get; set; }
        public List<Proxie> proxies { get; set; }
    }
    public int status { get; set; }
    public bool success { get; set; }
    public string message { get; set; }
    public List<Data> data { get; set; }
}
