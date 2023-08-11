using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class ResourcePackModel : ObservableObject
{
    [ObservableProperty]
    private bool _isSelect;

    public readonly GameEditTab8Model Top;
    public readonly ResourcepackObj Pack;

    public string Local => Pack.Local;
    public string PackFormat => Pack.pack_format.ToString();
    public string Description => Pack.description;
    public string Broken => Pack.Broken ?
            App.GetLanguage("GameEditWindow.Tab8.Info4") : "";

    public Bitmap Pic => Pack.Icon == null ? App.GameIcon : GetImage();

    public ResourcePackModel(GameEditTab8Model top, ResourcepackObj pack)
    {
        Top = top;
        Pack = pack;
    }

    public Bitmap GetImage()
    {
        using var stream = new MemoryStream(Pack.Icon);
        return new Bitmap(stream);
    }

    public void Select()
    {
        Top.SetSelect(this);
    }
}