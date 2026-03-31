using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 服务器包项目
/// </summary>
public partial class ServerPackItemModel : ObservableObject
{
    /// <summary>
    /// 下载地址
    /// </summary>
    [ObservableProperty]
    public partial string Url { get; set; }

    /// <summary>
    /// 项目ID
    /// </summary>
    [ObservableProperty]
    public partial string? PID { get; set; }

    /// <summary>
    /// 文件ID
    /// </summary>
    [ObservableProperty]
    public partial string? FID { get; set; }

    /// <summary>
    /// 校验
    /// </summary>
    [ObservableProperty]
    public partial string Sha256 { get; set; }

    /// <summary>
    /// 是否选中
    /// </summary>
    [ObservableProperty]
    public partial bool Check { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    [ObservableProperty]
    public partial string FileName { get; set; }

    /// <summary>
    /// 下载源名字
    /// </summary>
    public string Source
    {
        get
        {
            if (SourceType is { } type)
            {
                return type.GetName();
            }
            else
            {
                return "";
            }
        }
    }

    /// <summary>
    /// 下载源
    /// </summary>
    public SourceType? SourceType;

    /// <summary>
    /// 模组信息
    /// </summary>
    public ModDisplayModel Mod;
    /// <summary>
    /// 资源包信息
    /// </summary>
    public ResourcepackObj Resourcepack;

    partial void OnPIDChanged(string? value)
    {
        Update();
    }

    partial void OnFIDChanged(string? value)
    {
        Update();
    }

    /// <summary>
    /// 更新信息
    /// </summary>
    public void Update()
    {
        if (string.IsNullOrWhiteSpace(FID) || string.IsNullOrWhiteSpace(PID))
        {
            SourceType = null;
        }
        else
        {
            SourceType = GameDownloadHelper.TestSourceType(PID, FID);
        }
    }
}
