using System.Collections.Generic;

namespace ColorMC.Gui.Objs.Frp;

public record OpenFrpDownloadObj
{
    public record Data
    {
        public record Soft
        {
            public record OpenFrpDownloadArchObj
            {
                public string label { get; set; }
                public string file { get; set; }
            }
            public string os { get; set; }
            public string label { get; set; }
            public List<OpenFrpDownloadArchObj> arch { get; set; }
        }
        public record Launcher
        {
            public string latest { get; set; }
        }
        public record Source
        {
            public string label { get; set; }
            public string value { get; set; }
        }
        public string latest { get; set; }
        public string latest_full { get; set; }
        public string latest_ver { get; set; }
        public string latest_msg { get; set; }
        public string common_details { get; set; }
        public Launcher launcher { get; set; }
        public List<Source> source { get; set; }
        public List<Soft> soft { get; set; }
    }
    public Data data { get; set; }
}
