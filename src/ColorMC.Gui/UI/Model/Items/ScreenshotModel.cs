using Avalonia.Media.Imaging;
using ColorMC.Gui.UI.Model.GameEdit;
using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Items;

public partial class ScreenshotModel : ObservableObject
{
    public readonly string Screenshot;

    public readonly GameEditTab9Model Top;

    [ObservableProperty]
    private bool _isSelect;

    public string Name { get; }

    public Task<Bitmap> Image => GetImage();

    private Bitmap _img;

    public ScreenshotModel(GameEditTab9Model top, string obj)
    {
        Top = top;
        Screenshot = obj;
        Name = Path.GetFileName(Screenshot);
    }

    private async Task<Bitmap> GetImage()
    {
        return await Task.Run(() =>
        {
            using var image = SixLabors.ImageSharp.Image.Load
                     (Screenshot);
            using var stream = new MemoryStream();
            image.Mutate(p =>
            {
                p.Resize(200, 120);
            });

            image.SaveAsBmp(stream);

            stream.Seek(0, SeekOrigin.Begin);
            _img = new Bitmap(stream);

            return _img;
        });
    }

    public void Select()
    {
        Top.SetSelect(this);
    }

    public void Close()
    {
        _img?.Dispose();
    }
}
