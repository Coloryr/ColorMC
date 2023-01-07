using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs;

public record LanguageObj
{
    public Dictionary<string, string> Language { get; set; }
}
