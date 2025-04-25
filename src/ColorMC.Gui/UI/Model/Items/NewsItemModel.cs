using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// Minecraft news项目
/// </summary>
/// <param name="item"></param>
public partial class NewsItemModel(MinecraftNewObj.ResultObj.ResultsObj item) : ObservableObject
{
    /// <summary>
    /// 背景图片
    /// </summary>
    public Task<Bitmap?> Image => GetImage();
    /// <summary>
    /// 背景图片
    /// </summary>
    private Bitmap? _img;

    /// <summary>
    /// 标题
    /// </summary>
    public string Title => item.Title;
    /// <summary>
    /// 内容
    /// </summary>
    public string Description => item.Description;

    /// <summary>
    /// 打开连接
    /// </summary>
    public void OpenUrl()
    {
        BaseBinding.OpenUrl(item.Url);
    }

    /// <summary>
    /// 获取背景图
    /// </summary>
    /// <returns></returns>
    private async Task<Bitmap?> GetImage()
    {
        if (_img != null)
        {
            return _img;
        }
        if (item.Image == null)
        {
            return null;
        }
        try
        {
            await Task.Run(() =>
            {
                _img = ImageManager.Load(item.Image, false).Result;
            });
            return _img;
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("AddModPackWindow.Error5"), e);
        }

        return null;
    }
}
