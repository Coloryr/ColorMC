using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
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
    private string _url;
    /// <summary>
    /// 项目ID
    /// </summary>
    [ObservableProperty]
    private string? _pID;
    /// <summary>
    /// 文件ID
    /// </summary>
    [ObservableProperty]
    private string? _fID;
    /// <summary>
    /// 校验
    /// </summary>
    [ObservableProperty]
    public string _sha256;
    /// <summary>
    /// 是否选中
    /// </summary>
    [ObservableProperty]
    public bool _check;
    /// <summary>
    /// 文件名
    /// </summary>
    [ObservableProperty]
    public string _fileName;

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
