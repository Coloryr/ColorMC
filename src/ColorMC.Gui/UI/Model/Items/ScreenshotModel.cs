using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Gui.UI.Model.GameEdit;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;

namespace ColorMC.Gui.UI.Model.Items;

public partial class ScreenshotModel(GameEditModel top, string obj) : ObservableObject
{
    public string Screenshot => obj;

    public GameEditModel Top => top;

    [ObservableProperty]
    private bool _isSelect;

    public string Name { get; } = Path.GetFileName(obj);

    public Task<Bitmap> Image => GetImage();

    private Bitmap _img;

    private async Task<Bitmap> GetImage()
    {
        return await Task.Run(() =>
        {
            using var image = SKBitmap.Decode(Screenshot);
            using var image1 = image.Resize(new SKSizeI(200, 120), SKFilterQuality.High);
            using var data = image1.Encode(SKEncodedImageFormat.Png, 100);

            _img = new Bitmap(data.AsStream());

            return _img;
        });
    }

    public void Select()
    {
        Top.SetSelectScreenshot(this);
    }

    public void Close()
    {
        _img?.Dispose();
    }
}
