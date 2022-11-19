using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Game;

public record RefreshObj
{
    public string accessToken { get; set; }
    public string clientToken { get; set; }
    public bool requestUser { get; set; }
}
