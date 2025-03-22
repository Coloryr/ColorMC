using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// ��ͼ��Ŀ
/// </summary>
/// <param name="top"></param>
/// <param name="obj"></param>
public partial class ScreenshotModel(GameEditModel top, ScreenshotObj obj) : SelectItemModel
{
    /// <summary>
    /// ��ͼ��Ŀ
    /// </summary>
    public ScreenshotObj Obj => obj;
    /// <summary>
    /// ��ͼ·��
    /// </summary>
    public string Screenshot => obj.File;
    /// <summary>
    /// ��Ϸʵ���༭
    /// </summary>
    public GameEditModel TopModel => top;
    /// <summary>
    /// ����
    /// </summary>
    public string Name => obj.Name;
    /// <summary>
    /// ͼƬ
    /// </summary>
    public Task<Bitmap> Image => GetImage();
    /// <summary>
    /// ͼƬ
    /// </summary>
    private Bitmap _img;
    /// <summary>
    /// ��ȡͼƬ
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
    /// ѡ���ļ�
    /// </summary>
    public void Select()
    {
        TopModel.SetSelectScreenshot(this);
    }

    /// <summary>
    /// ����ͼƬ
    /// </summary>
    public void Close()
    {
        _img?.Dispose();
    }
}
