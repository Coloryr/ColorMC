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
    /// 顶层
    /// </summary>
    private readonly GameEditModel _top;
    /// <summary>
    /// 资源包
    /// </summary>
    public readonly ResourcepackObj Obj;

    /// <summary>
    /// 资源包位置
    /// </summary>
    public string Local => Path.GetFileName(Obj.Local);
    /// <summary>
    /// 资源包版本
    /// </summary>
    public int PackFormat => Obj.PackFormat;
    /// <summary>
    /// 资源包描述
    /// </summary>
    public Chat Description => GameBinding.StringToChat(Obj.Description);
    /// <summary>
    /// 是否为损坏的资源包
    /// </summary>
    public string Broken => Obj.Broken ? App.Lang("GameEditWindow.Tab8.Info4") : "";

    /// <summary>
    /// 资源包图标
    /// </summary>
    public Bitmap Pic { get; set; }

    public ResourcePackModel(GameEditModel top, ResourcepackObj pack)
    {
        _top = top;
        Obj = pack;
        Pic = GetImage();
    }

    /// <summary>
    /// 获取图片
    /// </summary>
    /// <returns></returns>
    public Bitmap GetImage()
    {
        if (Obj.Icon == null)
        { 
            return ImageManager.GameIcon;
        }
        using var stream = new MemoryStream(Obj.Icon);
        return new Bitmap(stream);
    }

    /// <summary>
    /// 选中该资源包
    /// </summary>
    public void Select()
    {
        _top.SetSelectResource(this);
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

    /// <summary>
    /// 删除资源
    /// </summary>
    public void DeleteResource()
    {
        _top.DeleteResource(Obj);
    }
}