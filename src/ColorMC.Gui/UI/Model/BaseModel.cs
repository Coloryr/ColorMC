using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// 窗口模型
/// </summary>
public partial class WindowModel : ObservableObject
{
    public const string NameInfoShow = "Info:Show";
    public const string NameIconChange = "Icon:Change";
    public const string NameGetTopLevel = "TopLevel:Get";

    /// <summary>
    /// 返回按钮
    /// </summary>
    private readonly ConcurrentStack<Action> _listBack = new();
    /// <summary>
    /// 当前选择按钮占用的窗口Id
    /// </summary>
    private string? _nowChoiseUse;
    /// <summary>
    /// 是否锁定关闭
    /// </summary>
    private bool _isWork;
    /// <summary>
    /// 选择按钮1
    /// </summary>
    private Action? _choise1Click;
    /// <summary>
    /// 选择按钮2
    /// </summary>
    private Action? _choise2Click;
    /// <summary>
    /// 系统窗口
    /// </summary>
    private TopLevel? _top;
    /// <summary>
    /// 锁定计数
    /// </summary>
    private int _lockCount;

    /// <summary>
    /// 提示弹出文本
    /// </summary>
    public string NotifyText { get; private set; }

    /// <summary>
    /// 窗口图标
    /// </summary>
    [ObservableProperty]
    private Bitmap _icon = ImageManager.GameIcon;
    /// <summary>
    /// 背景图
    /// </summary>
    [ObservableProperty]
    private Bitmap? _backImage;

    /// <summary>
    /// 主标题
    /// </summary>
    [ObservableProperty]
    private string? _title;
    /// <summary>
    /// 副标题
    /// </summary>
    [ObservableProperty]
    private string? _subTitle;
    /// <summary>
    /// 选择按钮1显示
    /// </summary>
    [ObservableProperty]
    private string? _headChoiseContent;
    /// <summary>
    /// 选择按钮2显示
    /// </summary>
    [ObservableProperty]
    private string? _headChoise1Content;

    /// <summary>
    /// 是否显示背景图
    /// </summary>
    [ObservableProperty]
    private bool _bgVisible;
    /// <summary>
    /// 是否显示窗口关闭按钮
    /// </summary>
    [ObservableProperty]
    private bool _enableHead = true;
    /// <summary>
    /// 是否显示选择按钮1
    /// </summary>
    [ObservableProperty]
    private bool _headChoiseDisplay;
    /// <summary>
    /// 是否显示选择按钮2
    /// </summary>
    [ObservableProperty]
    private bool _headChoise1Display;
    /// <summary>
    /// 是否显示返回按钮
    /// </summary>
    [ObservableProperty]
    private bool _headBack;
    /// <summary>
    /// 是否启动返回按钮
    /// </summary>
    [ObservableProperty]
    private bool _headBackEnable = true;
    /// <summary>
    /// 是否启用选择按钮1
    /// </summary>
    [ObservableProperty]
    private bool _choiseEnable = true;
    /// <summary>
    /// 是否启用选择按钮2
    /// </summary>
    [ObservableProperty]
    private bool _choise1Enable = true;

    /// <summary>
    /// 背景图透明度
    /// </summary>
    [ObservableProperty]
    private double _bgOpacity;

    /// <summary>
    /// 当前主题
    /// </summary>
    [ObservableProperty]
    private ThemeVariant _theme;

    /// <summary>
    /// 当前窗口透明
    /// </summary>
    [ObservableProperty]
    private WindowTransparencyLevel[] _hints;

    /// <summary>
    /// 显示通知
    /// </summary>
    public readonly SelfPublisher<bool> HeadDisplayObservale = new();
    /// <summary>
    /// 关闭按钮显示通知
    /// </summary>
    public readonly SelfPublisher<bool> HeadCloseObservale = new();

    /// <summary>
    /// 是否显示关闭按钮
    /// </summary>
    public bool HeadDisplay
    {
        set
        {
            HeadCloseObservale.Notify(value);
            HeadDisplayObservale.Notify(value);
        }
    }
    /// <summary>
    /// 是否启用关闭按钮
    /// </summary>
    public bool HeadCloseDisplay
    {
        set
        {
            if (!GuiConfigUtils.Config.WindowMode)
            {
                HeadCloseObservale.Notify(value);
            }
        }
    }
    /// <summary>
    /// 是否显示返回按钮
    /// </summary>
    public bool HeadBackDisplay
    {
        set
        {
            if (_isWork && value)
            {
                return;
            }
            HeadBack = value;
        }
    }

    /// <summary>
    /// 窗口ID
    /// </summary>
    public string WindowId { get; init; }

    public WindowModel()
    {

    }

    public WindowModel(string name)
    {
        WindowId = name;
    }

    /// <summary>
    /// 选择1按钮按下
    /// </summary>
    [RelayCommand]
    public void ChoiseClick()
    {
        _choise1Click?.Invoke();
    }
    /// <summary>
    /// 选择2按钮按下
    /// </summary>
    [RelayCommand]
    public void Choise1Click()
    {
        _choise2Click?.Invoke();
    }
    /// <summary>
    /// 返回按钮按下
    /// </summary>
    [RelayCommand]
    public void BackClick()
    {
        if (_isWork)
        {
            return;
        }
        if (_listBack.TryPeek(out var action))
        {
            action();
        }
    }

    /// <summary>
    /// 设置窗口图标
    /// </summary>
    /// <param name="image">图标</param>
    public void SetIcon(Bitmap image)
    {
        Icon = image;
    }

    /// <summary>
    /// 进入锁定模式
    /// </summary>
    public void Lock()
    {
        if (!_listBack.IsEmpty)
        {
            HeadBackEnable = false;
        }
        HeadCloseDisplay = false;
        _isWork = true;
        _lockCount++;
    }
    /// <summary>
    /// 解除锁定模式
    /// </summary>
    public void Unlock()
    {
        _lockCount--;
        if (_lockCount <= 0)
        {
            _isWork = false;
            if (!_listBack.IsEmpty)
            {
                HeadBackEnable = true;
                HeadBackDisplay = true;
            }
            HeadCloseDisplay = true;
        }
    }

    /// <summary>
    /// 设置选择按钮内容
    /// </summary>
    /// <param name="now">当前窗口Id</param>
    /// <param name="choise"></param>
    /// <param name="choise1"></param>
    public void SetChoiseContent(string now, string? choise, string? choise1 = null)
    {
        _nowChoiseUse = now;
        HeadChoiseContent = choise ?? "";
        HeadChoise1Content = choise1 ?? "";
    }
    /// <summary>
    /// 设置选择按钮事件
    /// </summary>
    /// <param name="use">当前窗口Id</param>
    /// <param name="choise"></param>
    /// <param name="choise1"></param>
    public void SetChoiseCall(string use, Action? choise, Action? choise1 = null)
    {
        _nowChoiseUse = use;
        _choise1Click = choise;
        if (choise != null)
        {
            HeadChoiseDisplay = true;
        }
        _choise2Click = choise1;
        if (choise1 != null)
        {
            HeadChoise1Display = true;
        }
    }
    /// <summary>
    /// 删除选择按钮内容
    /// </summary>
    /// <param name="now">当前窗口Id</param>
    public void RemoveChoiseData(string now)
    {
        if (_nowChoiseUse == now)
        {
            _nowChoiseUse = null;
            HeadChoiseContent = null;
            HeadChoise1Content = null;

            HeadChoiseDisplay = false;
            HeadChoise1Display = false;

            _choise1Click = null;
            _choise2Click = null;
        }
    }

    /// <summary>
    /// 添加一个返回
    /// </summary>
    /// <param name="back"></param>
    public void PushBack(Action back)
    {
        _listBack.Push(back);
        if (!_listBack.IsEmpty)
        {
            HeadBackDisplay = true;
        }
    }
    /// <summary>
    /// 删除一个返回
    /// </summary>
    public void PopBack()
    {
        _listBack.TryPop(out _);
        if (_listBack.IsEmpty)
        {
            HeadBackDisplay = false;
        }
    }
    /// <summary>
    /// 显示弹窗
    /// </summary>
    /// <param name="model"></param>
    public void ShowDialog(object model)
    {
        DialogHost.Show(model, WindowId);
    }

    /// <summary>
    /// 打开弹窗并等待
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public Task<object?> ShowDialogWait(object model)
    {
        return DialogHost.Show(model, WindowId);
    }

    /// <summary>
    /// 关闭弹窗
    /// </summary>
    public void CloseDialog(object model)
    {
        DialogHost.Close(WindowId, null, model);
        Unlock();
    }

    /// <summary>
    /// 显示一条提示信息
    /// </summary>
    /// <param name="data">信息</param>
    public void Notify(string data)
    {
        NotifyText = data;
        OnPropertyChanged(NameInfoShow);
    }

    /// <summary>
    /// 设置系统窗口
    /// </summary>
    /// <param name="top"></param>
    public void SetTopLevel(TopLevel? top)
    {
        _top = top;
    }

    /// <summary>
    /// 需要获取系统窗口
    /// </summary>
    /// <returns></returns>
    public TopLevel? GetTopLevel()
    {
        OnPropertyChanged(NameGetTopLevel);
        var top = _top;
        _top = null;
        return top;
    }

    public void Show(string text)
    {
        ShowDialog(new ChoiceModel(WindowId)
        {
            Text = text
        });
    }

    public Task<object?> ShowWait(string text)
    {
        Lock();
        var res = ShowDialogWait(new ChoiceModel(WindowId)
        {
            Text = text
        });
        Unlock();

        return res;
    }

    public ProgressModel ShowProgress(string text)
    {
        Lock();
        var dialog = new ProgressModel
        {
            Text = text
        };
        ShowDialog(dialog);

        return dialog;
    }

    public async Task<bool> ShowChoice(string text)
    {
        Lock();
        var dialog = new ChoiceModel(WindowId)
        {
            Text = text,
            CancelVisable = true
        };

        var res = await DialogHost.Show(dialog, WindowId) is true;
        Unlock();

        return res;
    }
}