using System.IO;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Model.Items;

public partial class ResourcePackModel : SelectItemModel
{
    public readonly GameEditModel TopModel;
    public readonly ResourcepackObj Pack;

    public string Local => Path.GetFileName(Pack.Local);
    public string PackFormat => Pack.pack_format.ToString();
    public Chat Description => GameBinding.StringToChat(Pack.description);
    public string Broken => Pack.Broken ? App.Lang("GameEditWindow.Tab8.Info4") : "";

    public Bitmap Pic { get; }

    public ResourcePackModel(GameEditModel top, ResourcepackObj pack)
    {
        TopModel = top;
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
        TopModel.SetSelectResource(this);
    }

    public void Close()
    {
        if (Pic != ImageManager.GameIcon)
        {
            Pic.Dispose();
        }
    }
}