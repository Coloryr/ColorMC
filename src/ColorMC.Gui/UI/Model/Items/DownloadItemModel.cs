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
    private bool _isNotSize;

    public double NowProgress { get; private set; }
    public string NowTemp { get; private set; }

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

    public void Update()
    {
        OnPropertyChanged(nameof(NowTemp));
        OnPropertyChanged(nameof(NowProgress));
    }

    public void Clear()
    {
        Name = "";
        ErrorTime = 0;
        State = App.Lang("DownloadWindow.Info4");
        NowProgress = 0;
        AllSize = 0;
        NowSize = 0;
        IsNotSize = false;
    }
}
