using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class ScreenshotModel : ObservableObject
{
    public readonly string Screenshot;

    public readonly GameEditTab9Model Top;

    [ObservableProperty]
    private bool _isSelect;

    public string Name { get; }

    public Task<Bitmap> Image => GetImage();

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
            return new Bitmap(stream);
        });
    }

    public void Select()
    {
        Top.SetSelect(this);
    }
}
