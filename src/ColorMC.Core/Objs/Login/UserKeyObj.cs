using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Login;

public record UserKeyObj
{
    public required string UUID { get; init; }
    public required AuthType Type { get; init; }
}
