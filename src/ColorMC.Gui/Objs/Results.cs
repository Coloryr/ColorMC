using System.Collections.Generic;
using System.Diagnostics;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.Objs.ColorMC;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.Objs;

/// <summary>
/// Frp启动结果
/// </summary>
public record FrpLaunchRes
{
    /// <summary>
    /// 是否启动
    /// </summary>
    public bool Res;
    /// <summary>
    /// 启动后进程
    /// </summary>
    public Process? Process;
    /// <summary>
    /// 启动后地址
    /// </summary>
    public string? IP;
}

/// <summary>
/// 游戏实例启动结果
/// </summary>
public record GameLaunchOneRes
{
    /// <summary>
    /// 是否启动
    /// </summary>
    public bool Res;
    /// <summary>
    /// 错误消息
    /// </summary>
    public string? Message;
    /// <summary>
    /// 登录账户
    /// </summary>
    public LoginObj? User;
    /// <summary>
    /// 是否登录失败
    /// </summary>
    public bool LoginFail;
}

/// <summary>
/// 启动多个游戏实例结果
/// </summary>
public record GameLaunchListRes
{
    public string? Message;
    /// <summary>
    /// 错误列表
    /// </summary>
    public Dictionary<string, GameLaunchOneRes> States;
    /// <summary>
    /// 使用的账户
    /// </summary>
    public LoginObj? User;
}

/// <summary>
/// 获取启动账户
/// </summary>
public record GameLaunchUserRes
{
    /// <summary>
    /// 账户
    /// </summary>
    public LoginObj? User;
    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Message;
}

/// <summary>
/// 模组下载列表结果
/// </summary>
public record ModDownloadListRes
{
    /// <summary>
    /// 下载信息
    /// </summary>
    public FileItemObj Item;
    /// <summary>
    /// 模组信息
    /// </summary>
    public ModInfoObj Info;
    /// <summary>
    /// 版本列表
    /// </summary>
    public List<FileModVersionModel> List;
}

/// <summary>
/// 获取Java列表
/// </summary>
public record GetJavaListRes
{
    /// <summary>
    /// 是否获取
    /// </summary>
    public bool Res;
    /// <summary>
    /// 支持的进制
    /// </summary>
    public List<string>? Arch;
    /// <summary>
    /// 支持的系统版本
    /// </summary>
    public List<string>? Os;
    /// <summary>
    /// 支持的主版本
    /// </summary>
    public List<string>? MainVersion;
    /// <summary>
    /// 下载列表
    /// </summary>
    public List<JavaDownloadModel>? Download;
}

/// <summary>
/// 获取Java列表
/// </summary>
public record GetJavaAdoptiumListRes
{
    /// <summary>
    /// 是否获取
    /// </summary>
    public bool Res;
    /// <summary>
    /// 支持的进制
    /// </summary>
    public List<string>? Arch;
    /// <summary>
    /// 下载列表
    /// </summary>
    public List<JavaDownloadModel>? Download;
}

/// <summary>
/// 音乐播放结果
/// </summary>
public record MusicPlayRes
{
    /// <summary>
    /// 是否播放
    /// </summary>
    public bool Res;
    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Message;
    /// <summary>
    /// 音乐信息
    /// </summary>
    public MusicInfoObj? MusicInfo;
}

/// <summary>
/// 下载项目列表
/// </summary>
public record FileListRes
{
    /// <summary>
    /// 列表
    /// </summary>
    public List<FileVersionItemModel>? List;
    /// <summary>
    /// 总数量
    /// </summary>
    public int Count;
    /// <summary>
    /// 标题
    /// </summary>
    public string Name;
}

/// <summary>
/// 整合包列表获取结果
/// </summary>
public record ModPackListRes
{
    /// <summary>
    /// 项目列表
    /// </summary>
    public List<FileItemModel>? List;
    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount;
}

/// <summary>
/// 云同步结果
/// </summary>
public record CloudRes : StringRes
{
    /// <summary>
    /// 是否成功获取
    /// </summary>
    public bool Data1;
    /// <summary>
    /// 获取结果
    /// </summary>
    public string? Data2;
}

/// <summary>
/// 获取云同步存档结果
/// </summary>
public record CloudWorldRes : StringRes
{
    /// <summary>
    /// 存档列表
    /// </summary>
    public List<CloudWorldObj>? Worlds;
}

/// <summary>
/// 云同步升级结果
/// </summary>
public record CloudUploadRes : StringRes
{
    /// <summary>
    /// 升级结果
    /// </summary>
    public int Data1;
    /// <summary>
    /// 信息
    /// </summary>
    public string? Data2;
}

/// <summary>
/// 文件下载获取结果
/// </summary>
public record FileItemRes
{
    /// <summary>
    /// 路径
    /// </summary>
    public string? Path;
    /// <summary>
    /// 下载项目
    /// </summary>
    public FileItemObj? File;
}

/// <summary>
/// 选中文件结果
/// </summary>
public record SelectRes
{
    /// <summary>
    /// 路径
    /// </summary>
    public string? Path;
    /// <summary>
    /// 文件名
    /// </summary>
    public string? FileName;
}

public record KeyRes
{
    public bool Positive;
    public byte Key;
}