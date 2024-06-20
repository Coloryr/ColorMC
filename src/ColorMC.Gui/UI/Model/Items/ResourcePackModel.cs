using System.IO;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.GameEdit;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class ResourcePackModel : ObservableObject
{
    [ObservableProperty]
    private bool _isSelect;

    public readonly GameEditModel Top;
    public readonly ResourcepackObj Pack;

    public string Local => Pack.Local;
    public string PackFormat => Pack.pack_format.ToString();
    public string Description => Pack.description;
    public string Broken => Pack.Broken ? App.Lang("GameEditWindow.Tab8.Info4") : "";

    public Bitmap Pic { get; }

    public ResourcePackModel(GameEditModel top, ResourcepackObj pack)
    {
        Top = top;
        Pack = pack;
        Pic = Pack.Icon == null ? ImageManager.GameIcon : GetImage();
    }

    public Bitmap GetImage()
    {
        using var stream = new MemoryStream(Pack.Icon);
        return new Bitmap(stream);
    }

    public void Select()
    {
        Top.SetSelectResource(this);
    }

    public void Close()
    {
        if (Pic != ImageManager.GameIcon)
        {
            Pic.Dispose();
        }
    }
}