using System.IO;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 资源包显示
/// </summary>
public partial class ResourcePackModel : SelectItemModel
{
    /// <summary>
    /// 
    /// </summary>
    public readonly GameEditModel TopModel;
    /// <summary>
    /// 资源包
    /// </summary>
    public readonly ResourcepackObj Pack;

    /// <summary>
    /// 资源包位置
    /// </summary>
    public string Local => Path.GetFileName(Pack.Local);
    /// <summary>
    /// 资源包版本
    /// </summary>
    public string PackFormat => Pack.PackFormat.ToString();
    /// <summary>
    /// 资源包描述
    /// </summary>
    public Chat Description => GameBinding.StringToChat(Pack.Description);
    /// <summary>
    /// 是否为损坏的资源包
    /// </summary>
    public string Broken => Pack.Broken ? App.Lang("GameEditWindow.Tab8.Info4") : "";

    /// <summary>
    /// 资源包图标
    /// </summary>
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

    /// <summary>
    /// 选中该资源包
    /// </summary>
    public void Select()
    {
        TopModel.SetSelectResource(this);
    }

    /// <summary>
    /// 清理图标
    /// </summary>
    public void Close()
    {
        if (Pic != ImageManager.GameIcon)
        {
            Pic.Dispose();
        }
    }
}