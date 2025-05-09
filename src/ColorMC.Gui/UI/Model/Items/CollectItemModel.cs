using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core.Helpers;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 收藏项目
/// </summary>
/// <param name="obj"></param>
public partial class CollectItemModel(CollectItemObj obj) : SelectItemModel
{
    public const int DefaultWidth = 350;

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
    /// 宽度
    /// </summary>
    [ObservableProperty]
    private double _width = DefaultWidth;

    /// <summary>
    /// 收藏
    /// </summary>
    public readonly CollectItemObj Obj = obj;

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
            await Task.Run(() =>
            {
                _img = ImageManager.Load(Obj.Icon, true).Result;
            });
            return _img;
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("AddModPackWindow.Error5"), e);
        }

        return null;
    }

    /// <summary>
    /// 设置小模式
    /// </summary>
    /// <param name="min"></param>
    public void SetMin(bool min)
    {
        if (min)
        {
            Width = double.NaN;
        }
        else
        {
            Width = DefaultWidth;
        }
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
