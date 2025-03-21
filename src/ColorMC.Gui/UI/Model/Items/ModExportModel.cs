using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 打包模组项目
/// </summary>
public partial class ModExportModel : ObservableObject
{
    /// <summary>
    /// 校验
    /// </summary>
    public string Sha1;
    /// <summary>
    /// 校验
    /// </summary>
    public string Sha512;
    /// <summary>
    /// 下载地址
    /// </summary>
    public string Url;
    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize;

    /// <summary>
    /// 打包类型
    /// </summary>
    public PackType Type;
    /// <summary>
    /// 模组数据
    /// </summary>
    public ModInfoObj? Obj1;
    /// <summary>
    /// 模组数据
    /// </summary>
    public ModObj Obj;

    /// <summary>
    /// 导出
    /// </summary>
    [ObservableProperty]
    private bool _export;
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
    /// 名字
    /// </summary>
    public string Name => Obj.Name;
    /// <summary>
    /// modid
    /// </summary>
    public string Modid => Obj.ModId;
    /// <summary>
    /// 加载器
    /// </summary>
    public string Loader => StringHelper.MakeString(Obj.Loaders);
    /// <summary>
    /// 下载源
    /// </summary>
    public SourceType? Source { get; init; }

    /// <summary>
    /// 操作
    /// </summary>
    private readonly BaseModel _model;

    public ModExportModel(BaseModel model, string? pid, string? fid)
    {
        _pID = pid;
        _fID = fid;
        _model = model;

        if (string.IsNullOrWhiteSpace(PID) || string.IsNullOrWhiteSpace(FID))
        {
            Source = null;
        }
        else
        {
            Source = DownloadItemHelper.TestSourceType(PID, FID);
        }

        Reload();
    }

    partial void OnPIDChanged(string? value)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Source)));
        Reload();
    }

    partial void OnFIDChanged(string? value)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Source)));
        Reload();
    }

    partial void OnExportChanged(bool value)
    {
        if (value)
        {
            if (Obj1 == null)
            {
                _model.Show(App.Lang("GameEditWindow.Tab4.Error5"));
                Export = false;
            }
        }
    }

    /// <summary>
    /// 重载数据
    /// </summary>
    public void Reload()
    {
        if (Type == PackType.CurseForge)
        {
            Export = Source == SourceType.CurseForge;
        }
        else if (Type == PackType.Modrinth)
        {
            Export = Source != null;
        }
    }
}