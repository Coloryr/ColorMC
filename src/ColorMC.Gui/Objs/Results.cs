﻿using System.Collections.Generic;
using System.Diagnostics;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.UI.Model.Items;

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

public record GameLaunchUserRes
{
    public LoginObj? User;
    public string? Message;
}

public record ModDownloadRes
{
    public DownloadItemObj Item;
    public ModInfoObj Info;
    public List<FileModVersionModel> List;
}

public record GetJavaListRes
{
    public bool Res;
    public List<string>? Arch;
    public List<string>? Os;
    public List<string>? MainVersion;
    public List<JavaDownloadModel>? Download;
}

public record GetJavaAdoptiumListRes
{
    public bool Res;
    public List<string>? Arch;
    public List<JavaDownloadModel>? Download;
}