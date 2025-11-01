using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 下载项目显示
/// </summary>
public partial class DownloadItemModel(int index) : ObservableObject
{
    /// <summary>
    /// 下载线程
    /// </summary>
    public int Index { get; init; } = index;

    /// <summary>
    /// 名字
    /// </summary>
    [ObservableProperty]
    private string _name;
    /// <summary>
    /// 当前状态
    /// </summary>
    [ObservableProperty]
    private string _state = LanguageUtils.Get("DownloadWindow.Info4");
    /// <summary>
    /// 错误次数
    /// </summary
    [ObservableProperty]
    private int _errorTime;
    /// <summary>
    /// 是否没有总大小
    /// </summary>
    [ObservableProperty]
    private bool _isNotSize;

    /// <summary>
    /// 当前进度
    /// </summary>
    public double NowProgress { get; private set; }
    /// <summary>
    /// 当前进度
    /// </summary>
    public string NowTemp { get; private set; }

    /// <summary>
    /// 当前大小
    /// </summary>
    private long _nowSize;

    /// <summary>
    /// 总大小
    /// </summary>
    public long AllSize { get; set; }
    /// <summary>
    /// 已下载大小
    /// </summary>
    public long NowSize
    {
        get
        {
            return _nowSize;
        }
        set
        {
            _nowSize = value;
            if (AllSize == 0)
            {
                IsNotSize = true;
                NowProgress = 0.0f;
                NowTemp = $"{(double)value / 1000 / 1000:0.##} MB";
            }
            else
            {
                NowProgress = value / (double)AllSize * 100;
                NowTemp = $"{(double)value / 1000 / 1000:0.##} / {AllSize / 1000 / 1000:0.##} MB";
            }
        }
    }

    /// <summary>
    /// 刷新UI
    /// </summary>
    public void Update()
    {
        OnPropertyChanged(nameof(NowTemp));
        OnPropertyChanged(nameof(NowProgress));
    }

    /// <summary>
    /// 清理内容
    /// </summary>
    public void Clear()
    {
        Name = "";
        ErrorTime = 0;
        State = LanguageUtils.Get("DownloadWindow.Info4");
        NowProgress = 0;
        AllSize = 0;
        NowSize = 0;
        IsNotSize = false;
    }
}
