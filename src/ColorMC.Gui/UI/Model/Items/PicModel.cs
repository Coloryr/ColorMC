using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class PicModel(string name, string? logo, bool min) : ObservableObject
{
    /// <summary>
    /// 图标
    /// </summary>
    public Task<Bitmap?> Image => GetImage();
    /// <summary>
    /// 图标
    /// </summary>
    private Bitmap? _img;

    public string Name => name;
    public string? Logo => logo;

    /// <summary>
    /// 是否已经关闭
    /// </summary>
    private bool _close;

    /// <summary>
    /// 获取图标
    /// </summary>
    /// <returns></returns>
    private async Task<Bitmap?> GetImage()
    {
        if (_close || Logo == null)
        {
            return null;
        }
        if (_img != null)
        {
            return _img;
        }
        try
        {
            _img = await ImageManager.Load(Logo, min);
            return _img;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageUtils.Get("AddModPackWindow.Text26"), e);
        }

        return null;
    }

    /// <summary>
    /// 清理图标
    /// </summary>
    public void Close()
    {
        _close = true;
        if (_img != ImageManager.GameIcon)
        {
            _img?.Dispose();
            _img = null;
            OnPropertyChanged(nameof(Image));
        }
    }
}
