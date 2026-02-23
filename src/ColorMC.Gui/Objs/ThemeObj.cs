using Avalonia.Media;

namespace ColorMC.Gui.Objs;

public record ThemeObj
{
    /// <summary>
    /// 主要颜色
    /// </summary>
    public IBrush MainColor;
    /// <summary>
    /// 字体颜色
    /// </summary>
    public IBrush FontColor;
    public IBrush WindowBG;
    public IBrush WindowTranColor;
    public IBrush ProgressBarBG;
    public IBrush MainGroupBG;
    public IBrush MainGroupBorder;
    public IBrush ItemBG;
    public IBrush GameItemBG;
    public IBrush TopViewBG;
    public IBrush AllBorder;
    public IBrush ButtonBG;
    public IBrush ButtonOver;
    /// <summary>
    /// 按钮边框背景色
    /// </summary>
    public IBrush ButtonBorder;
    /// <summary>
    /// 覆盖层不带透明度背景色
    /// </summary>
    public IBrush TopBGColor;
    /// <summary>
    /// 拖拽文件显示框背景色
    /// </summary>
    public IBrush TopGridColor;
    /// <summary>
    /// 覆盖层边框颜色
    /// </summary>
    public IBrush OverBrushColor;
    public IBrush SelectItemBG;
    public IBrush SelectItemOver;
    public IBrush MenuBG;
    public IBrush BorderColor;
    public IBrush RadioGroupBG;
    public IBrush RadioSelectBG;
    public IBrush RadioSelect;
    public IBrush BorderBG;
}
