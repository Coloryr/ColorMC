using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;

namespace ColorMC.Gui.Objs;

public record FrpLaunchRes
{
    public bool Res;
    public Process? Process;
    public string? IP;
}

public record GameLaunchOneRes
{
    public bool Res;
    public string? Message;
    public bool LoginFail;
    public LoginObj? User;
}

public record GameLaunchListRes
{
    public List<string>? Done;
    public string? Message;
    public Dictionary<string, LaunchState> Fail;
    public LoginObj? User;
}
