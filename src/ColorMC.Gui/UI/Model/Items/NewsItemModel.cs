using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class NewsItemModel(MinecraftNewObj.ArticleObj item) : ObservableObject
{
    public Task<Bitmap?> Image => GetImage();

    private Bitmap? _img;

    public string Title => item.DefaultTile.Title.ToUpper();
    public string SubTitle => item.DefaultTile.SubHeader;
    public string Category => item.PrimaryCategory;

    public void OpenUrl()
    {
        BaseBinding.OpenUrl("https://www.minecraft.net" + item.ArticleUrl);
    }

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
