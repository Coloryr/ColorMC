using ColorMC.Core.Login;
using ColorMC.Core.Objs.Minecraft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Game;

public record LoginObj
{
    public string UserName { get; set; }
    public string Token { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public AuthType AuthType { get; set; }
    public string UUID { get; set; }
    public List<UserPropertyObj> Properties { get; set; }
}
