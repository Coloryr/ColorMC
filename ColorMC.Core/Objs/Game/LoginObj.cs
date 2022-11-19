using ColorMC.Core.Game.Auth;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Core.Objs.Game;

public record LoginObj
{
    public string UserName { get; set; }
    public string UUID { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string ClientToken { get; set; }
    public AuthType AuthType { get; set; }
    public List<UserPropertyObj> Properties { get; set; }
    public string Text1 { get; set; }
    public string Text2 { get; set; }
}
