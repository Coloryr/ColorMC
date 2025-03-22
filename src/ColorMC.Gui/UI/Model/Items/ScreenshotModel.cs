using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 截图项目
/// </summary>
/// <param name="top"></param>
/// <param name="obj"></param>
public partial class ScreenshotModel(GameEditModel top, ScreenshotObj obj) : SelectItemModel
{
    /// <summary>
    /// 截图项目
    /// </summary>
    public ScreenshotObj Obj => obj;
    /// <summary>
    /// 截图路径
    /// </summary>
    public string Screenshot => obj.File;
    /// <summary>
    /// 游戏实例编辑
    /// </summary>
    public GameEditModel TopModel => top;
    /// <summary>
    /// 名字
    /// </summary>
    public string Name => obj.Name;
    /// <summary>
    /// 图片
    /// </summary>
    public Task<Bitmap> Image => GetImage();
    /// <summary>
    /// 图片
    /// </summary>
    private Bitmap _img;
    /// <summary>
    /// 获取图片
    /// </summary>
    /// <returns></returns>
    private async Task<Bitmap> GetImage()
    {
        return await Task.Run(() =>
        {
            using var data = File.OpenRead(Screenshot);
            _img = Bitmap.DecodeToWidth(data, 230);

            return _img;
        });
    }

    /// <summary>
    /// 选中文件
    /// </summary>
    public void Select()
    {
        TopModel.SetSelectScreenshot(this);
    }

    /// <summary>
    /// 清理图片
    /// </summary>
    public void Close()
    {
        _img?.Dispose();
    }
}
