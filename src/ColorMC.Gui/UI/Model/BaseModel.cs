using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using AvaloniaEdit.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// 窗口基础模型
/// </summary>
public partial class BaseModel : ObservableObject
{
    public const string NameInfoShow = "Info:Show";
    public const string NameIconChange = "Icon:Change";
    public const string NameGetTopLevel = "TopLevel:Get";

    /// <summary>
    /// 进度条
    /// </summary>
    private readonly Info1Model _info1;
    /// <summary>
    /// 输入框
    /// </summary>
    private readonly Info3Model _info3;
    /// <summary>
    /// 选择框
    /// </summary>
    private readonly Info4Model _info4;
    /// <summary>
    /// 单选框
    /// </summary>
    private readonly Info5Model _info5;
    /// <summary>
    /// 长文本
    /// </summary>
    private readonly Info6Model _info6;
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

    public BaseModel()
    {
        _info1 = new();
        _info3 = new(WindowId);
        _info4 = new(WindowId);
        _info5 = new(WindowId);
        _info6 = new(WindowId);
    }

    public BaseModel(string name)
    {
        WindowId = name;
        _info1 = new();
        _info3 = new(WindowId);
        _info4 = new(WindowId);
        _info5 = new(WindowId);
        _info6 = new(WindowId);
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
    }
    /// <summary>
    /// 解除锁定模式
    /// </summary>
    public void Unlock()
    {
        _isWork = false;
#if !Phone
        if (!_listBack.IsEmpty)
        {
            HeadBackEnable = true;
        }
#endif
        HeadCloseDisplay = true;
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
#if !Phone
        if (!_listBack.IsEmpty)
        {
            HeadBackDisplay = true;
        }
#endif
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
    /// 显示进度条
    /// </summary>
    public void Progress()
    {
        ShowClose();
        DialogHost.Show(_info1, WindowId);

        Lock();
    }
    /// <summary>
    /// 显示进度条信息
    /// </summary>
    /// <param name="data">信息</param>
    public void Progress(string data)
    {
        ShowClose();
        _info1.Indeterminate = true;
        _info1.Text = data;
        Progress();
    }
    /// <summary>
    /// 更新进度条信息
    /// </summary>
    /// <param name="value">进度</param>
    public void ProgressUpdate(double value)
    {
        if (value == -1)
        {
            _info1.Indeterminate = true;
        }
        else
        {
            _info1.Indeterminate = false;
            _info1.Value = value;
        }
    }
    /// <summary>
    /// 更新进度条信息
    /// </summary>
    /// <param name="data">信息</param>
    public void ProgressUpdate(string data)
    {
        _info1.Text = data;
    }
    /// <summary>
    /// 关闭进度条
    /// </summary>
    public void ProgressClose()
    {
        _info1.Indeterminate = false;

        ShowClose();

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
    /// 关闭输入框
    /// </summary>
    public void InputClose()
    {
        _info3.ValueVisable = false;

        ShowClose();
    }
    /// <summary>
    /// 显示一个输入框
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="data">显示内容</param>
    /// <returns>输入结果</returns>
    public async Task<InputRes> Input(string title, string data)
    {
        ShowClose();

        _info3.CancelEnable = true;
        _info3.CancelVisible = true;
        _info3.ConfirmEnable = true;

        _info3.Text2Visable = false;
        _info3.TextReadonly = false;
        _info3.ValueVisable = false;

        _info3.Text1 = data;
        _info3.Watermark1 = title;

        _info3.Call = null;
        _info3.ChoiseVisible = false;

        Lock();
        var res = await DialogHost.Show(_info3, WindowId);

        Unlock();

        return new InputRes
        {
            Cancel = res is not true,
            Text1 = _info3.Text1
        };
    }

    /// <summary>
    /// 显示一个输入框，里面有基础内容
    /// </summary>
    /// <param name="data1">内容1</param>
    /// <param name="data2">内容2</param>
    /// <param name="text1">提示内容1</param>
    /// <param name="text2">提示内容2</param>
    /// <returns></returns>
    public async Task<InputRes> InputWithEdit(string data1, string data2, string text1 = "", string text2 = "")
    {
        ShowClose();

        _info3.TextReadonly = false;
        _info3.Text1 = data1;
        _info3.Text2 = data2;

        _info3.Text2Visable = true;

        _info3.Watermark1 = text1;
        _info3.Watermark2 = text2;

        _info3.ValueVisable = false;

        _info3.CancelEnable = true;
        _info3.CancelVisible = true;
        _info3.ConfirmEnable = true;

        _info3.Password = '\0';

        _info3.Call = null;
        _info3.ChoiseVisible = false;

        Lock();
        var res = await DialogHost.Show(_info3, WindowId);

        Unlock();

        return new InputRes
        {
            Cancel = res is not true,
            Text1 = _info3.Text1,
            Text2 = _info3.Text2
        };
    }
    /// <summary>
    /// 显示一个输入框，里面有基础内容，并等待输入关闭
    /// </summary>
    /// <param name="title">显示的标题</param>
    /// <param name="lock1">是否为只读</param>
    /// <returns></returns>
    public async Task<InputRes> InputWithEditAsync(string title, bool lock1)
    {
        ShowClose();
        _info3.Text2Visable = false;
        _info3.TextReadonly = lock1;
        _info3.Call = null;
        _info3.ChoiseVisible = false;

        if (lock1)
        {
            _info3.Text1 = title;
            _info3.Watermark1 = "";

            _info3.ValueVisable = true;

            _info3.CancelEnable = false;
            _info3.CancelVisible = false;
            _info3.ConfirmEnable = false;

            _info3.Password = '\0';
        }
        else
        {
            _info3.Text1 = "";
            _info3.ValueVisable = false;

            _info3.Watermark1 = title;

            _info3.CancelEnable = true;
            _info3.CancelVisible = true;
            _info3.ConfirmEnable = true;
        }

        Lock();
        var res = await DialogHost.Show(_info3, WindowId);
        Unlock();

        return new InputRes
        {
            Cancel = res is not true,
            Text1 = _info3.Text1
        };
    }
    /// <summary>
    /// 显示一个输入框，并等待输入关闭
    /// </summary>
    /// <param name="title1">输入框提示1</param>
    /// <param name="title2">输入框提示2</param>
    /// <param name="password">是否为密码输入</param>
    /// <returns></returns>
    public async Task<InputRes> InputAsync(string title1, string title2, bool password)
    {
        ShowClose();
        _info3.TextReadonly = false;

        _info3.ValueVisable = false;

        _info3.Text1 = "";
        _info3.Text2 = "";

        _info3.Text2Visable = true;

        _info3.Watermark1 = title1;
        _info3.Watermark2 = title2;

        _info3.ConfirmEnable = true;

        _info3.CancelEnable = true;
        _info3.CancelVisible = true;

        _info3.Password = password ? '*' : '\0';

        _info3.ChoiseVisible = false;

        Lock();
        var res = await DialogHost.Show(_info3, WindowId);
        Unlock();

        _info3.Call = null;

        return new InputRes
        {
            Cancel = res is not true,
            Text1 = _info3.Text1,
            Text2 = _info3.Text2
        };
    }
    /// <summary>
    /// 显示一个输入框，显示两个只读内容
    /// </summary>
    /// <param name="text1">内容1</param>
    /// <param name="text2">内容2</param>
    /// <param name="iscancel">是否可以取消</param>
    /// <param name="isconfirm">是否可以确认</param>
    /// <param name="isvalue">是否显示滚动条</param>
    /// <param name="cancel">按下取消后执行</param>
    public void InputWithReadInfo(string text1, string text2, bool iscancel, bool isconfirm, bool isvalue, Action? cancel)
    {
        ShowClose();

        _info3.TextReadonly = true;
        _info3.Text1 = text1;
        _info3.Text2 = text2;

        _info3.Watermark1 = "";
        _info3.Watermark2 = "";

        _info3.Text2Visable = true;

        _info3.Password = '\0';

        _info3.ChoiseVisible = false;

        _info3.Call = cancel;
        _info3.ConfirmEnable = isconfirm;
        _info3.ValueVisable = isvalue;
        _info3.CancelEnable = iscancel;
        _info3.CancelVisible = iscancel;

        Lock();
        
        DialogHost.Show(_info3, WindowId);
    }
    /// <summary>
    /// 显示一个输入框，显示一个只读内容，设置一个选择按钮
    /// </summary>
    /// <param name="title">显示内容</param>
    /// <param name="choiseText">选项内容</param>
    /// <param name="choise">选项事件</param>
    public void InputWithChoise(string title, string choiseText, Action? choise)
    {
        ShowClose();

        _info3.TextReadonly = true;
        _info3.Text1 = title;

        _info3.Watermark1 = "";

        _info3.ValueVisable = false;

        _info3.Text2Visable = false;
        _info3.ConfirmEnable = true;

        _info3.ChoiseCall = choise;
        _info3.ChoiseText = choiseText;
        _info3.ChoiseVisible = true;

        DialogHost.Show(_info3, WindowId);
    }

    /// <summary>
    /// 显示一个对话框，选择确定还是取消
    /// </summary>
    /// <param name="data">要显示的内容</param>
    /// <returns>结果</returns>
    public async Task<bool> ShowAsync(string data)
    {
        ShowClose();

        _info4.ConfirmVisable = true;
        bool reut = false;
        _info4.EnableButton = true;
        _info4.Text = data;
        _info4.CancelVisable = true;
        _info4.ChoiseVisiable = false;

        _info4.Call = (res) =>
        {
            reut = res;
        };

        Lock();
        await DialogHost.Show(_info4, WindowId);

        Unlock();

        _info4.Call = null;

        return reut;
    }
    /// <summary>
    /// 显示一个对话框，自定义取消操作
    /// </summary>
    /// <param name="data">显示内容</param>
    /// <param name="action">取消操作</param>
    public void ShowWithCancel(string data, Action action)
    {
        ShowClose();

        _info4.EnableButton = true;
        _info4.Text = data;
        _info4.CancelVisable = true;
        _info4.ConfirmVisable = false;
        _info4.ChoiseVisiable = false;

        _info4.Call = (res) =>
        {
            _info4.ConfirmVisable = true;
            action.Invoke();
        };

        DialogHost.Show(_info4, WindowId);
    }
    /// <summary>
    /// 显示一个对话框，关闭取消按钮
    /// </summary>
    /// <param name="data">内容</param>
    public void Show(string data)
    {
        ShowClose();

        _info4.ConfirmVisable = true;
        _info4.EnableButton = true;
        _info4.CancelVisable = false;
        _info4.Call = null;
        _info4.Text = data;
        _info4.ChoiseVisiable = false;

        DialogHost.Show(_info4, WindowId);
    }
    /// <summary>
    /// 显示一个对话框，自定义确认操作
    /// </summary>
    /// <param name="data">显示内容</param>
    /// <param name="action">确认执行</param>
    public void ShowWithOk(string data, Action action)
    {
        ShowClose();

        _info4.ConfirmVisable = true;
        _info4.EnableButton = true;
        _info4.CancelVisable = false;
        _info4.Text = data;
        _info4.ChoiseVisiable = false;

        _info4.Call = (res) =>
        {
            action.Invoke();
        };

        DialogHost.Show(_info4, WindowId);
    }
    /// <summary>
    /// 显示一个选择框，并显示一个自定义选项
    /// </summary>
    /// <param name="data">内容</param>
    /// <param name="choise">选择按钮内容</param>
    /// <param name="action">选择执行内容</param>
    public void ShowWithChoise(string data, string choise, Action action)
    {
        ShowClose();

        _info4.ConfirmVisable = true;
        _info4.EnableButton = true;
        _info4.CancelVisable = false;
        _info4.Call = null;
        _info4.Text = data;
        _info4.ChoiseVisiable = true;
        _info4.ChoiseText = choise;
        _info4.ChoiseCall = action;

        DialogHost.Show(_info4, WindowId);
    }
    /// <summary>
    /// 显示一个选择框，自定义取消操作，显示一个自定义选项，并等待执行结束
    /// </summary>
    /// <param name="data">显示内容</param>
    /// <param name="choise">选择内容</param>
    /// <param name="action">选择执行</param>
    /// <param name="cancel">取消时执行</param>
    /// <returns></returns>
    public async Task ShowChoiseCancelWait(string data, string choise, Action action, Action<bool> cancel)
    {
        ShowClose();

        _info4.ConfirmVisable = true;
        _info4.EnableButton = true;
        _info4.CancelVisable = true;
        _info4.Call = cancel;
        _info4.Text = data;
        _info4.ChoiseVisiable = true;
        _info4.ChoiseText = choise;
        _info4.ChoiseCall = action;

        await DialogHost.Show(_info4, WindowId);
    }

    /// <summary>
    /// 显示一个选择框，下拉选择
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="data1">下拉内容</param>
    /// <returns></returns>
    public async Task<ComboRes> Combo(string title, IEnumerable<string> data1)
    {
        ShowClose();

        _info5.Text = title;
        _info5.Items.Clear();
        _info5.Items.AddRange(data1);
        _info5.Select = null!;
        foreach (var item in data1)
        {
            _info5.Select = item;
            break;
        }

        Lock();
        var res = await DialogHost.Show(_info5, WindowId);
        Unlock();

        return new ComboRes
        {
            Cancel = res is not true,
            Index = _info5.Index,
            Item = _info5.Select
        };
    }

    /// <summary>
    /// 显示一个文字框，显示两段文字
    /// </summary>
    /// <param name="data1">文字1</param>
    /// <param name="data2">文字2</param>
    public void Text(string data1, string data2)
    {
        ShowClose();

        _info6.Text1 = data1;
        _info6.Text2 = data2;
        _info6.CancelEnable = false;

        DialogHost.Show(_info6, WindowId);
    }
    /// <summary>
    /// 显示一个文字框，等待选择确认还是取消
    /// </summary>
    /// <param name="data">文字1</param>
    /// <param name="data1">文字2</param>
    /// <returns></returns>
    public async Task<bool> TextAsync(string data, string data1)
    {
        ShowClose();

        _info6.Text1 = data;
        _info6.Text2 = data1;
        _info6.CancelEnable = true;

        Lock();
        var res = await DialogHost.Show(_info6, WindowId);

        Unlock();

        return res is true;
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

    /// <summary>
    /// 关闭弹框
    /// </summary>
    public void ShowClose()
    {
        try
        {
            if (DialogHost.IsDialogOpen(WindowId))
            {
                DialogHost.Close(WindowId);
            }
        }
        catch
        {

        }
    }
}