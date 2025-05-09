using System.Collections.Generic;

namespace ColorMC.Gui.Objs.Frp;

public record OpenFrpChannelObj
{
    public record ProxieObj
    {
        public string name { get; set; }
        public int id { get; set; }
        public string type { get; set; }
        public string remote { get; set; }
        public string local { get; set; }
    }
    public record OpenFrpChannelData
    {
        public string name { get; set; }
        public List<ProxieObj> proxies { get; set; }
    }
    public int status { get; set; }
    public bool success { get; set; }
    public string message { get; set; }
    public List<OpenFrpChannelData> data { get; set; }
}
