using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 模组项目
/// </summary>
public partial class ModDisplayModel : ObservableObject
{
    public const string NameModText = "ModText";

    /// <summary>
    /// 是否启用
    /// </summary>
    [ObservableProperty]
    private bool _enable;
    /// <summary>
    /// 文本
    /// </summary>
    [ObservableProperty]
    private string? _text;

    /// <summary>
    /// 加载测
    /// </summary>
    public string Side => Obj.Side.GetName();
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// 模组ID
    /// </summary>
    public string Modid => Obj.ModId;
    /// <summary>
    /// 版本号
    /// </summary>
    public string Version => Obj.Version + (IsNew ? " " + App.Lang("GameEditWindow.Tab4.Info21") : "");
    /// <summary>
    /// 文件位置
    /// </summary>
    public string Local => Obj.Local;
    /// <summary>
    /// 作者
    /// </summary>
    public string Author => StringHelper.MakeString(Obj.Author);
    /// <summary>
    /// 链接
    /// </summary>
    public string? Url => Obj.Url;
    /// <summary>
    /// 加载器
    /// </summary>
    public string Loader => StringHelper.MakeString(Obj.Loaders);
    /// <summary>
    /// 下载源
    /// </summary>
    public string Source { get; init; }
    /// <summary>
    /// 项目ID
    /// </summary>
    public string? PID => Obj1?.ModId;
    /// <summary>
    /// 文件ID
    /// </summary>
    public string? FID => Obj1?.FileId;

    /// <summary>
    /// 是否有新版本
    /// </summary>
    public bool IsNew;
    /// <summary>
    /// Mod信息
    /// </summary>
    public ModInfoObj? Obj1;
    /// <summary>
    /// Mod内容
    /// </summary>
    public ModObj Obj;
    /// <summary>
    /// 顶层
    /// </summary>
    private readonly IModEdit? _top;

    public ModDisplayModel(ModObj obj, ModInfoObj? obj1, IModEdit? top)
    {
        _top = top;
        Obj = obj;
        Obj1 = obj1;

        Name = obj.ReadFail ? App.Lang("GameBinding.Info15") : obj.Name;
        Enable = !obj.Disable;
        if (string.IsNullOrWhiteSpace(PID) || string.IsNullOrWhiteSpace(FID))
        {
            Source = "";
        }
        else
        {
            Source = GameDownloadHelper.TestSourceType(PID, FID).GetName();
        }
    }

    partial void OnTextChanged(string? value)
    {
        _top?.EditModText(this);
    }

    /// <summary>
    /// 文件位置修改后
    /// </summary>
    public void LocalChange()
    {
        OnPropertyChanged(nameof(Local));
    }
}

