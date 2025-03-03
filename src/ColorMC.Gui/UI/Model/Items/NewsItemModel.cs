using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// Minecraft news项目
/// </summary>
/// <param name="item"></param>
public partial class NewsItemModel(MinecraftNewObj.ArticleObj item) : ObservableObject
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
    public string Title => item.DefaultTile.Title.ToUpper();
    /// <summary>
    /// 子标题
    /// </summary>
    public string SubTitle => item.DefaultTile.SubHeader;
    /// <summary>
    /// 分类
    /// </summary>
    public string Category => item.PrimaryCategory;

    /// <summary>
    /// 打开连接
    /// </summary>
    public void OpenUrl()
    {
        BaseBinding.OpenUrl("https://www.minecraft.net" + item.ArticleUrl);
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
        if (item.DefaultTile.Image.ImageURL == null)
        {
            return null;
        }
        try
        {
            await Task.Run(() =>
            {
                _img = ImageManager.Load("https://www.minecraft.net" + item.DefaultTile.Image.ImageURL, false).Result;
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
