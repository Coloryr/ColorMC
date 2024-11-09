using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Model.Items;

public partial class ScreenshotModel(GameEditModel top, ScreenshotObj obj) : SelectItemModel
{
    public ScreenshotObj Obj => obj;

    public string Screenshot => obj.File;

    public GameEditModel TopModel => top;

    public string Name => obj.Name;

    public Task<Bitmap> Image => GetImage();

    private Bitmap _img;

    private async Task<Bitmap> GetImage()
    {
        return await Task.Run(() =>
        {
            using var data = File.OpenRead(Screenshot);
            _img = Bitmap.DecodeToWidth(data, 230);

            return _img;
        });
    }

    public void Select()
    {
        TopModel.SetSelectScreenshot(this);
    }

    public void Close()
    {
        _img?.Dispose();
    }
}
