using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.CurseForge;

public record CurseForgeCategoriesObj
{
    public record Data
    { 
        public int id { get; set; }
        public int gameId { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string url { get; set; }
        public string iconUrl { get; set; }
        public string dateModified { get; set; }
        public int classId { get; set; }
        public int parentCategoryId { get; set; }
        public int displayIndex { get; set; }
    }
    public List<Data> data { get; set; }
}
