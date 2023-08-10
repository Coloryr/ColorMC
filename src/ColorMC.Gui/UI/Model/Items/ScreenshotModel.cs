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
    public ScreenshotDisplayObj Screenshot { get; init; }

    private readonly ILoadFuntion<ScreenshotModel> _top;
    private readonly IUserControl _con;

    [ObservableProperty]
    private bool _isSelect;

    public string Name => Screenshot.Name;

    public Task<Bitmap> Image => GetImage();

    public ScreenshotModel(IUserControl con, ILoadFuntion<ScreenshotModel> top, ScreenshotDisplayObj obj)
    {
        _con = con;
        _top = top;
        Screenshot = obj;
    }

    public void Load()
    {
        using var image = SixLabors.ImageSharp.Image.Load
                 (Screenshot.Local);
        var stream = new MemoryStream();
        Bitmap bitmap = null!;
        //image.Mutate(p =>
        //{
        //    p.Resize(200, 120);
        //});
    }

    private async Task<Bitmap> GetImage()
    {
        await Task.Run(Load);

        

        //image.SaveAsBmp(stream);
        //image.Dispose();

        //stream.Seek(0, SeekOrigin.Begin);
        //bitmap = new Bitmap(stream);
        //stream.Dispose();

        return null;
    }

    public void Select()
    {
        _top.SetSelect(this);
    }

    public void Flyout(Control con)
    {
        _ = new GameEditFlyout4(con, this);
    }

    public async void Delete()
    {
        var Window = _con.Window;
        var res = await Window.OkInfo.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab9.Info1"), Screenshot.Local));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteScreenshot(Screenshot.Local);
        Window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        await _top.Load();
    }
}
