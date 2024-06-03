using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 下载项目显示
/// </summary>
public partial class DownloadItemModel(int index) : ObservableObject
{
    public int Index { get; init; } = index;

    /// <summary>
    /// 名字
    /// </summary>
    [ObservableProperty]
    private string _name;
    /// <summary>
    /// 总大小
    /// </summary>
    [ObservableProperty]
    private long _allSize;
    /// <summary>
    /// 已下载大小
    /// </summary>
    [ObservableProperty]
    private long _nowSize;
    /// <summary>
    /// 当前状态
    /// </summary>
    [ObservableProperty]
    private string _state = App.Lang("DownloadWindow.Info4");
    /// <summary>
    /// 错误次数
    /// </summary
    [ObservableProperty]
    private int _errorTime;

    [ObservableProperty]
    private double _nowProgress;

    [ObservableProperty]
    private bool _isNotSize;

    [ObservableProperty]
    private string _nowTemp;

    partial void OnNowSizeChanged(long value)
    {
        if (AllSize == 0)
        {
            IsNotSize = true;
            NowProgress = 0.0f;
            NowTemp = $"{(double)NowSize / 1000 / 1000:0.##} MB";
        }
        else
        {
            NowProgress = (double)NowSize / (double)AllSize;
            NowTemp = $"{(double)NowSize / 1000 / 1000:0.##} / {AllSize / 1000 / 1000:0.##} MB";
        }
    }

    public void Clear()
    {
        ErrorTime = 0;
        State = App.Lang("DownloadWindow.Info4");
        NowProgress = 0;
        AllSize = 0;
        NowSize = 0;
        IsNotSize = false;
    }
}
