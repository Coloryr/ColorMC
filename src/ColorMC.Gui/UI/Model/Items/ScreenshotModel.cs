using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Model.Items;

public partial class ScreenshotModel(GameEditModel top, string obj) : SelectItemModel
{
    public string Screenshot => obj;

    public GameEditModel Top => top;

    public string Name { get; } = Path.GetFileName(obj);

    public Task<Bitmap> Image => GetImage();

    private Bitmap _img;

    private async Task<Bitmap> GetImage()
    {
        return await Task.Run(() =>
        {
            //using var image = SKBitmap.Decode(Screenshot);
            //using var image1 = image.Resize(new SKSizeI(230, 129), SKFilterQuality.High);
            //using var data = image1.Encode(SKEncodedImageFormat.Png, 100);

            using var data = File.OpenRead(Screenshot);
            _img = Bitmap.DecodeToWidth(data, 230);

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
