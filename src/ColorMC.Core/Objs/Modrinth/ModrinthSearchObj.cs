using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthSearchObj
{
    public record Hit
    {
        public string project_id { get; set; }
        public string project_type { get; set; }
        public string slug { get; set; }
        public string author { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public List<string> categories { get; set; }
        public List<string> display_categories { get; set; }
        public List<string> versions { get; set; }
        public int downloads { get; set; }
        public int follows { get; set; }
        public string icon_url { get; set; }
        public string date_created { get; set; }
        public string date_modified { get; set; }
        public string latest_version { get; set; }
        public string license { get; set; }
        public string client_side { get; set; }
        public string server_side { get; set; }
    }
    public List<Hit> hits { get; set; }
}
