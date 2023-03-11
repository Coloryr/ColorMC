using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ColorMC.Core.Objs;

/// <summary>
/// 下载项目显示
/// </summary>
public class DownloadDisplayObj : INotifyPropertyChanged
{
    /// <summary>
    /// 名字
    /// </summary>
    private string name;
    /// <summary>
    /// 总大小
    /// </summary>
    private string allsize;
    /// <summary>
    /// 已下载大小
    /// </summary>
    private string nowsize;
    /// <summary>
    /// 当前状态
    /// </summary>
    private string state;
    /// <summary>
    /// 错误次数
    /// </summary>
    private int errortime;

    public string Name { get { return name; } set { name = value; NotifyPropertyChanged(); } }
    public string AllSize { get { return allsize; } set { allsize = value; NotifyPropertyChanged(); } }
    public string NowSize { get { return nowsize; } set { nowsize = value; NotifyPropertyChanged(); } }
    public string State { get { return state; } set { state = value; NotifyPropertyChanged(); } }
    public int ErrorTime { get { return errortime; } set { errortime = value; NotifyPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// 1秒下载的大小
    /// </summary>
    public long Last;
}
