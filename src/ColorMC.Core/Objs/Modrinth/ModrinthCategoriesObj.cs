using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthCategoriesObj
{
    public string icon { get; set; }
    public string name { get; set; }
    public string project_type { get; set; }
    public string header { get; set; }
}
