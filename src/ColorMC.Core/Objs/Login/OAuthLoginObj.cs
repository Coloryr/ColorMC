using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Login;

/// <summary>
/// 登录参数
/// </summary>
public record OAuthLoginObj
{
    public record OAuthLoginPropertiesObj
    { 
        public string AuthMethod { get; set; }
        public string SiteName { get; set; }
        public string RpsTicket { get; set; }
    }
    public OAuthLoginPropertiesObj Properties { get; set; }
    public string RelyingParty { get; set; }
    public string TokenType { get; set; }
}

public record OAuthLogin1Obj
{
    public record OAuthLogin1PropertiesObj
    {
        public string SandboxId { get; set; }
        public string[] UserTokens { get; set; }
    }
    public OAuthLogin1PropertiesObj Properties { get; set; }
    public string RelyingParty { get; set; }
    public string TokenType { get; set; }
}
