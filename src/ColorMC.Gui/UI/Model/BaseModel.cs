using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using AvaloniaEdit.Utils;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using DialogHostAvalonia;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model;

public partial class BaseModel : ObservableObject
{
    private readonly Info1Model _info1;
    private readonly Info3Model _info3;
    private readonly Info4Model _info4;
    private readonly Info5Model _info5;
    private readonly Info6Model _info6;

    private readonly ConcurrentStack<Action> _listBack = new();

    private string? _nowUse;
    private string? _noChoiseUse;

    private bool _isWork;

    private Action? _choiseClick;

    public string NotifyText;

    [ObservableProperty]
    private Bitmap _icon = App.GameIcon;
    [ObservableProperty]
    private Bitmap? _back;

    [ObservableProperty]
    private string? _title;
    [ObservableProperty]
    private string? _title1;

    [ObservableProperty]
    private bool _bgVisible;
    [ObservableProperty]
    private double _bgOpacity;

    [ObservableProperty]
    private ThemeVariant _theme;
    [ObservableProperty]
    private IBrush _background;

    [ObservableProperty]
    private bool _enableHead = true;

    [ObservableProperty]
    private WindowTransparencyLevel[] _hints;

    public SelfPublisher<bool> HeadDisplayObservale = new();
    public SelfPublisher<bool> HeadBackObservale = new();
    public SelfPublisher<bool> HeadChoiseObservale = new();
    public SelfPublisher<bool> HeadCloseObservale = new();
    public SelfPublisher<string?> HeadChoiseContentObservale = new();
    public SelfPublisher<string?> HeadBackContentObservale = new();

    public bool HeadDisplay
    {
        set
        {
            HeadCloseObservale.Notify(value);
            HeadDisplayObservale.Notify(value);
        }
    }
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
    public bool HeadBackDisplay
    {
        set
        {
            if (_isWork && value)
            {
                return;
            }
            HeadBackObservale.Notify(value);
        }
    }
    public bool HeadChoiseDisplay
    {
        set
        {
            HeadChoiseObservale.Notify(value);
        }
    }

    public string Name { get; init; }

    public BaseModel()
    {
        _info1 = new();
        _info3 = new(Name);
        _info4 = new(Name);
        _info5 = new(Name);
        _info6 = new(Name);
    }

    public BaseModel(string name)
    {
        Name = name;
        _info1 = new();
        _info3 = new(Name);
        _info4 = new(Name);
        _info5 = new(Name);
        _info6 = new(Name);
    }

    private void Work()
    {
        HeadBackDisplay = false;
        HeadCloseDisplay = false;
        _isWork = true;
    }

    private void NoWork()
    {
        _isWork = false;
        if (!_listBack.IsEmpty)
        {
            HeadBackDisplay = true;
        }
        HeadCloseDisplay = true;
    }

    public void AddHeadContent(string now, string? choise, string? back = null)
    {
        if (_nowUse != now)
        {
            HeadChoiseObservale.Notify(true);
        }
        _nowUse = now;
        HeadBackContentObservale.Notify(back ?? App.Lang("Gui.Info31"));
        HeadChoiseContentObservale.Notify(choise ?? "");
    }

    public void RemoveHeadContent(string now)
    {
        if (_nowUse == now)
        {
            _nowUse = null;
            HeadBackContentObservale.Notify(App.Lang("Gui.Info31"));
            HeadChoiseContentObservale.Notify(null);
        }
    }

    public void AddHeadCall(string? use = null, Action? back = null, Action? choise = null)
    {
        if (use != null)
        {
            _noChoiseUse = use;
        }
        if (back != null)
        {
            _listBack.Push(back);
        }
        if (choise != null)
        {
            if (use == null)
            {
                throw new ArgumentNullException("use is null");
            }
            _choiseClick = choise;
        }
        if (!_listBack.IsEmpty)
        {
            HeadBackDisplay = true;
        }
    }

    public void RemoveBack()
    {
        _listBack.TryPop(out _);
        if (_listBack.IsEmpty)
        {
            HeadBackDisplay = false;
        }
    }

    public void RemoveChoiseCall(string now)
    {
        if (_noChoiseUse == now)
        {
            HeadChoiseObservale.Notify(false);
            _noChoiseUse = null;
            _choiseClick = null;
        }
    }

    public void ChoiseClick()
    {
        _choiseClick?.Invoke();
    }

    public void BackClick()
    {
        if (_listBack.TryPeek(out var action))
        {
            action();
        }
    }

    /// <summary>
    /// 显示进度条
    /// </summary>
    public void Progress()
    {
        DClose();
        DialogHost.Show(_info1, Name);

        Work();
    }

    /// <summary>
    /// 显示进度条信息
    /// </summary>
    /// <param name="data">信息</param>
    public void Progress(string data)
    {
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

    public void ProgressClose()
    {
        _info1.Indeterminate = false;

        DClose();

        NoWork();
    }

    /// <summary>
    /// 显示一条提示信息
    /// </summary>
    /// <param name="data">信息</param>
    public void Notify(string data)
    {
        NotifyText = data;
        OnPropertyChanged("Info2Show");
    }

    public void SetIcon(Bitmap image)
    {
        Icon = image;
    }

    public void InputClose()
    {
        _info3.ValueVisable = false;

        DClose();
    }

    public async Task<(bool Cancel, string? Text1)> ShowEdit(string title, string data)
    {
        _info3.CancelEnable = true;
        _info3.CancelVisible = true;
        _info3.ConfirmEnable = true;

        _info3.Text2Visable = false;
        _info3.TextReadonly = false;
        _info3.ValueVisable = false;

        _info3.Text1 = data;
        _info3.Watermark1 = title;

        _info3.Call = null;

        DClose();
        Work();
        await DialogHost.Show(_info3, Name);

        NoWork();

        return (_info3.IsCancel, _info3.Text1);
    }

    public async Task<(bool Cancel, string? Text1, string? Text2)>
       ShowEditInput(string data, string data1)
    {
        _info3.TextReadonly = false;
        _info3.Text1 = data;
        _info3.Text2 = data1;

        _info3.Watermark1 = "";
        _info3.Watermark2 = "";

        _info3.ValueVisable = false;

        _info3.CancelEnable = true;
        _info3.CancelVisible = true;
        _info3.ConfirmEnable = true;

        _info3.Password = '\0';

        _info3.Call = null;

        DClose();
        Work();
        await DialogHost.Show(_info3, Name);

        NoWork();

        return (_info3.IsCancel, _info3.Text1, _info3.Text2);
    }

    /// <summary>
    /// 打开一个对话框，显示只读内容
    /// </summary>
    /// <param name="title"></param>
    /// <param name="lock1">是否不等待用户确认</param>
    /// <returns></returns>
    public async Task<(bool Cancel, string? Text)> ShowInputOne(string title, bool lock1)
    {
        _info3.Text2Visable = false;
        _info3.TextReadonly = lock1;
        _info3.Call = null;
        _info3.IsCancel = true;

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

        DClose();
        Work();
        await DialogHost.Show(_info3, Name);

        NoWork();

        return (_info3.IsCancel, _info3.Text1);
    }

    public async Task<(bool Cancel, string? Text1, string? Text2)>
        ShowInput(string title, string title1, bool password)
    {
        _info3.TextReadonly = false;

        _info3.ValueVisable = false;

        _info3.Text1 = "";
        _info3.Text2 = "";

        _info3.Watermark1 = title;
        _info3.Watermark2 = title1;

        _info3.ConfirmEnable = true;

        _info3.CancelEnable = true;
        _info3.CancelVisible = true;

        _info3.Password = password ? '*' : '\0';

        _info3.IsCancel = true;

        DClose();
        Work();
        await DialogHost.Show(_info3, Name);

        NoWork();

        _info3.Call = null;

        return (_info3.IsCancel, _info3.Text1, _info3.Text2);
    }

    /// <summary>
    /// 打开一个对话框，显示两个只读内容
    /// </summary>
    /// <param name="title"></param>
    /// <param name="title1"></param>
    /// <param name="cancel"></param>
    public void ShowReadInfo(string title, string title1, Action? cancel)
    {
        _info3.TextReadonly = true;
        _info3.Text1 = title;
        _info3.Text2 = title1;

        _info3.Watermark1 = "";
        _info3.Watermark2 = "";

        _info3.Text2Visable = true;

        _info3.Password = '\0';

        _info3.IsCancel = true;

        if (cancel != null)
        {
            _info3.ConfirmEnable = false;
            _info3.ValueVisable = true;

            _info3.Call = cancel;

            _info3.CancelEnable = true;
            _info3.CancelVisible = true;
        }
        else
        {
            _info3.ConfirmEnable = true;

            _info3.CancelEnable = false;
            _info3.CancelVisible = false;
        }

        DClose();
        DialogHost.Show(_info3, Name);
    }

    /// <summary>
    /// 打开一个对话框，显示一个只读内容
    /// </summary>
    /// <param name="title"></param>
    /// <param name="title1"></param>
    /// <param name="cancel"></param>
    public void ShowReadInfoOne(string title, Action? cancel)
    {
        _info3.TextReadonly = true;
        _info3.Text1 = title;

        _info3.Watermark1 = "";

        _info3.ValueVisable = false;

        _info3.Text2Visable = false;
        _info3.ConfirmEnable = true;

        _info3.IsCancel = true;

        if (cancel != null)
        {
            _info3.Call = cancel;

            _info3.CancelEnable = true;
            _info3.CancelVisible = true;

            _info3.Password = '\0';
        }
        else
        {
            _info3.Call = null;
            _info3.CancelVisible = false;
        }

        DClose();
        DialogHost.Show(_info3, Name);
    }

    /// <summary>
    /// 打开一个对话框，选择确定还是取消
    /// </summary>
    /// <param name="data">要显示的内容</param>
    /// <returns>结果</returns>
    public async Task<bool> ShowWait(string data)
    {
        bool reut = false;
        _info4.Enable = true;
        _info4.Text = data;
        _info4.CancelVisable = true;

        _info4.Call = (res) =>
        {
            reut = res;
        };

        DClose();
        Work();
        await DialogHost.Show(_info4, Name);

        NoWork();

        _info4.Call = null;

        return reut;
    }

    /// <summary>
    /// 打开一个对话框，显示内容
    /// </summary>
    /// <param name="data">内容</param>
    public void Show(string data)
    {
        _info4.Enable = true;
        _info4.CancelVisable = false;
        _info4.Call = null;
        _info4.Text = data;

        DClose();
        DialogHost.Show(_info4, Name);
    }

    public void ShowOk(string data, Action action)
    {
        _info4.Enable = true;
        _info4.CancelVisable = false;
        _info4.Text = data;

        _info4.Call = (res) =>
        {
            action.Invoke();
        };

        DClose();
        DialogHost.Show(_info4, Name);
    }

    public async Task<(bool Cancel, int Index, string? Item)>
        ShowCombo(string data, List<string> data1)
    {
        _info5.Text = data;
        _info5.Items.Clear();
        _info5.Items.AddRange(data1);
        _info5.Select = null!;
        _info5.Select = data1[0];
        _info5.IsCancel = true;

        DClose();
        Work();
        await DialogHost.Show(_info5, Name);

        NoWork();

        return (_info5.IsCancel, _info5.Index, _info5.Select);
    }

    public void ShowText(string data, string data1)
    {
        _info6.Text1 = data;
        _info6.Text2 = data1;
        _info6.IsCancel = true;
        _info6.NeedCancel = false;

        DClose();
        DialogHost.Show(_info6, Name);
    }

    public async Task<bool> ShowTextWait(string data, string data1)
    {
        _info6.Text1 = data;
        _info6.Text2 = data1;
        _info6.IsCancel = true;
        _info6.NeedCancel = true;

        DClose();
        Work();
        await DialogHost.Show(_info6, Name);

        NoWork();

        return !_info6.IsCancel;
    }

    public void DClose()
    {
        try
        {
            if (DialogHost.IsDialogOpen(Name))
            {
                DialogHost.Close(Name);
            }
        }
        catch
        { 
        
        }
    }
}
