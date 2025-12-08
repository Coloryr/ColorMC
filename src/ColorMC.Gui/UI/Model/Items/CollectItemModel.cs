using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 收藏项目
/// </summary>
/// <param name="obj"></param>
public partial class CollectItemModel(CollectItemObj obj) : SelectItemModel
{
    /// <summary>
    /// 上层回调
    /// </summary>
    public ICollectControl Add { get; set; }

    /// <summary>
    /// 名字
    /// </summary>
    public string Name => Obj.Name;
    /// <summary>
    /// 类型
    /// </summary>
    public string Type => Obj.FileType.GetName();
    /// <summary>
    /// 下载源
    /// </summary>
    public string Source => Obj.Source.GetName();

    /// <summary>
    /// 图标
    /// </summary>
    public Task<Bitmap?> Image => GetImage();

    /// <summary>
    /// 图标缓存
    /// </summary>
    private Bitmap? _img;

    /// <summary>
    /// 是否选中
    /// </summary>
    [ObservableProperty]
    private bool _isCheck;
    /// <summary>
    /// 是否在下载中
    /// </summary>
    [ObservableProperty]
    private bool _isDownload;

    /// <summary>
    /// 收藏
    /// </summary>
    public CollectItemObj Obj => obj;

    partial void OnIsCheckChanged(bool value)
    {
        Add.ChoiseChange();
    }

    /// <summary>
    /// 打开网页
    /// </summary>
    [RelayCommand]
    public void OpenWeb()
    {
        BaseBinding.OpenUrl(Obj.Url);
    }

    /// <summary>
    /// 安装该资源
    /// </summary>
    [RelayCommand]
    public void Install()
    {
        Add?.Install(this);
    }

    /// <summary>
    /// 获取图标
    /// </summary>
    /// <returns></returns>
    private async Task<Bitmap?> GetImage()
    {
        if (_img != null || Obj.Icon == null)
        {
            return _img;
        }
        try
        {
            _img = await ImageManager.Load(Obj.Icon, 100);
            return _img;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageUtils.Get("AddModPackWindow.Text26"), e);
        }

        return null;
    }

    /// <summary>
    /// 清理图标
    /// </summary>
    public void Close()
    {
        _img?.Dispose();
    }

    /// <summary>
    /// 选中
    /// </summary>
    public void SetSelect()
    {
        Add?.SetSelect(this);
    }

    /// <summary>
    /// 取消选择
    /// </summary>
    public void Uncheck()
    {
        IsSelect = false;
        IsCheck = false;
    }
}
