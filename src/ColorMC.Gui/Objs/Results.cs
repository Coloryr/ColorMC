using System.Collections.Generic;
using System.Diagnostics;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.Objs.ColorMC;
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
    public FileItemObj Item;
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

public record MusicPlayRes
{
    public bool Res;
    public string? Message;
    public MusicInfoObj? MusicInfo;
}

public record FileListRes
{
    public List<FileVersionItemModel>? List;
    public int Count;
}

public record ModPackListRes
{
    public List<FileItemModel>? List;
    public int Count;
}

public record InputRes
{
    public bool Cancel;
    public string? Text1;
    public string? Text2;
}

public record ComboRes
{
    public bool Cancel;
    public int Index;
    public string? Item;
}

public record CloudRes : MessageRes
{
    public bool Data1;
    public string? Data2;
}

public record CloudWorldRes : MessageRes
{
    public List<CloudWorldObj>? Data;
}

public record CloudUploadRes : MessageRes
{
    public int Data1;
    public string? Data2;
}