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
    public string Name;
}

/// <summary>
/// 整合包列表获取结果
/// </summary>
public record ModPackListRes
{
    public List<FileItemModel>? List;
    public int Count;
}

/// <summary>
/// 输入信息结果
/// </summary>
public record InputRes
{
    public bool Cancel;
    public string? Text1;
    public string? Text2;
}

/// <summary>
/// 选中框选择结果
/// </summary>
public record ComboRes
{
    public bool Cancel;
    public int Index;
    public string? Item;
}

/// <summary>
/// 云同步结果
/// </summary>
public record CloudRes : StringRes
{
    public bool Data1;
    public string? Data2;
}

/// <summary>
/// 获取云同步存档结果
/// </summary>
public record CloudWorldRes : StringRes
{
    public List<CloudWorldObj>? Worlds;
}

/// <summary>
/// 云同步升级结果
/// </summary>
public record CloudUploadRes : StringRes
{
    public int Data1;
    public string? Data2;
}

/// <summary>
/// 文件下载获取结果
/// </summary>
public record FileItemRes
{
    public string? Path;
    public FileItemObj? File;
}

/// <summary>
/// 选中文件结果
/// </summary>
public record SelectRes
{
    public string? Path;
    public string? FileName;
}