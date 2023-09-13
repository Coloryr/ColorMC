using Avalonia.Media.Imaging;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model;

public partial class BaseModel : ObservableObject
{
    public Task Info1Task;

    public string NotifyText;

    private Action? _info3Call;
    private Action<bool>? _info4Call;

    private bool _info3Cancel;
    private bool _info5Cancel;
    private bool _info6Cancel;

    private readonly Semaphore _info3Semaphore = new(0, 2);
    private readonly Semaphore _info5Semaphore = new(0, 2);
    private readonly Semaphore _info6Semaphore = new(0, 2);

    [ObservableProperty]
    private string _info1Text;
    [ObservableProperty]
    private double _info1Value;
    [ObservableProperty]
    private bool _info1Indeterminate;

    [ObservableProperty]
    private string _info3Text1;
    [ObservableProperty]
    private string _info3Text2;
    [ObservableProperty]
    private string _info3Watermark1;
    [ObservableProperty]
    private string _info3Watermark2;
    [ObservableProperty]
    private bool _info3ConfirmEnable;
    [ObservableProperty]
    private bool _info3CancelEnable;
    [ObservableProperty]
    private bool _info3CancelVisible;
    [ObservableProperty]
    private bool _info3TextReadonly;
    [ObservableProperty]
    private bool _info3Text2Visable;
    [ObservableProperty]
    private bool _info3ValueVisable;
    [ObservableProperty]
    private char _info3Password;

    [ObservableProperty]
    private string _info4Text;
    [ObservableProperty]
    private bool _info4Enable;
    [ObservableProperty]
    private bool _info4CancelVisable;

    [ObservableProperty]
    private string _info5Text;
    [ObservableProperty]
    private string _info5Select;
    [ObservableProperty]
    private int _info5Index;

    [ObservableProperty]
    private string _info6Text1;
    [ObservableProperty]
    private string _info6Text2;

    public ObservableCollection<string> Info5Items { get; init; } = new();

    [ObservableProperty]
    private Bitmap _icon = App.GameIcon;
    [ObservableProperty]
    private string? _title;
    [ObservableProperty]
    private string? _title1;

    [RelayCommand]
    public void Info3Cancel()
    {
        if (_info3Call != null)
        {
            _info3Call();
            Info3CancelEnable = false;
            _info3Call = null;
            return;
        }

        _info3Cancel = true;
        _info3Semaphore.Release();

        InputClose();
    }

    [RelayCommand]
    public void Info3Confirm()
    {
        _info3Cancel = false;
        _info3Semaphore.Release();

        InputClose();
    }

    [RelayCommand]
    public void Info4Cancel()
    {
        Info4Enable = false;
        OnPropertyChanged("Info4Close");

        _info4Call?.Invoke(false);
    }

    [RelayCommand]
    public void Info4Confirm()
    {
        Info4Enable = false;
        OnPropertyChanged("Info4Close");

        _info4Call?.Invoke(true);
    }

    [RelayCommand]
    public void Info5Cancel()
    {
        _info5Cancel = true;
        _info5Semaphore.Release();
        OnPropertyChanged("Info5Close");
    }

    [RelayCommand]
    public void Info5Confirm()
    {
        _info5Cancel = false;
        _info5Semaphore.Release();
        OnPropertyChanged("Info5Close");
    }

    [RelayCommand]
    public void Info6Cancel()
    {
        _info6Cancel = true;
        _info6Semaphore.Release();
        OnPropertyChanged("Info6Close");
    }

    [RelayCommand]
    public void Info6Confirm()
    {
        _info6Cancel = false;
        _info6Semaphore.Release();
        OnPropertyChanged("Info6Close");
    }

    /// <summary>
    /// 显示进度条
    /// </summary>
    public void Progress()
    {
        OnPropertyChanged("Info1Show");
    }

    /// <summary>
    /// 显示进度条信息
    /// </summary>
    /// <param name="data">信息</param>
    public void Progress(string data)
    {
        Info1Indeterminate = true;
        Info1Text = data;
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
            Info1Indeterminate = true;
        }
        else
        {
            Info1Indeterminate = false;
            Info1Value = value;
        }
    }

    /// <summary>
    /// 更新进度条信息
    /// </summary>
    /// <param name="data">信息</param>
    public void ProgressUpdate(string data)
    {
        Info1Text = data;
    }

    public void ProgressClose()
    {
        Info1Indeterminate = false;
        OnPropertyChanged("Info1Close");
    }

    public Task ProgressCloseAsync()
    {
        Info1Indeterminate = false;
        OnPropertyChanged("Info1CloseAsync");
        return Info1Task;
    }

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
        OnPropertyChanged("Info3Close");
    }

    public async Task<(bool Cancel, string? Text1)>
        ShowEdit(string title, string data)
    {
        Info3Text2Visable = false;
        Info3TextReadonly = false;
        Info3ValueVisable = false;

        Info3Text1 = data;
        Info3Watermark1 = title;
        OnPropertyChanged("Info3Show");

        _info3Call = null;
        await Task.Run(() =>
        {
            _info3Semaphore.WaitOne();

        });

        return (_info3Cancel, Info3Text1);
    }

    public async Task<(bool Cancel, string? Text1, string? Text2)>
       ShowEditInput(string data, string data1)
    {
        Info3TextReadonly = false;
        Info3Text1 = data;
        Info3Text2 = data1;

        Info3Watermark1 = "";
        Info3Watermark2 = "";

        Info3ValueVisable = false;

        Info3CancelEnable = true;
        Info3CancelVisible = true;
        Info3ConfirmEnable = true;

        Info3Password = '\0';

        OnPropertyChanged("Info3Show");

        _info3Call = null;
        await Task.Run(() =>
        {
            _info3Semaphore.WaitOne();
        });

        return (_info3Cancel, Info3Text1, Info3Text2);
    }

    public async Task<(bool Cancel, string? Text)>
        ShowOne(string title, bool lock1)
    {
        Info3Text2Visable = false;
        Info3TextReadonly = lock1;
        if (lock1)
        {
            Info3Text1 = title;
            Info3Watermark1 = "";

            Info3ValueVisable = true;

            Info3CancelEnable = false;
            Info3CancelVisible = false;
            Info3ConfirmEnable = false;

            Info3Password = '\0';
        }
        else
        {
            Info3Text1 = "";
            Info3ValueVisable = false;

            Info3Watermark1 = title;

            Info3CancelEnable = true;
            Info3CancelVisible = true;
            Info3ConfirmEnable = true;
        }

        OnPropertyChanged("Info3Show");

        if (!lock1)
        {
            _info3Call = null;
            await Task.Run(() =>
            {
                _info3Semaphore.WaitOne();
            });
        }

        return (_info3Cancel, Info3Text1);
    }

    public async Task<(bool Cancel, string? Text1, string? Text2)>
        ShowInput(string title, string title1, bool password)
    {
        Info3TextReadonly = false;

        Info3ValueVisable = false;

        Info3Text1 = "";
        Info3Text2 = "";

        Info3Watermark1 = title;
        Info3Watermark2 = title1;

        Info3ConfirmEnable = true;

        Info3CancelEnable = true;
        Info3CancelVisible = true;

        Info3Password = password ? '*' : '\0';

        OnPropertyChanged("Info3Show");

        _info3Call = null;

        await Task.Run(() =>
        {
            _info3Semaphore.WaitOne();
        });

        return (_info3Cancel, Info3Text1, Info3Text2);
    }

    public void ShowInput(string title, string title1, Action cancel)
    {
        Info3TextReadonly = true;
        Info3Text1 = title;
        Info3Text2 = title1;

        Info3Watermark1 = "";
        Info3Watermark2 = "";

        Info3Text2Visable = true;
        Info3ConfirmEnable = false;

        _info3Call = cancel;

        Info3CancelEnable = true;
        Info3CancelVisible = true;

        Info3Password = '\0';

        OnPropertyChanged("Info3Show");
    }

    public async Task<bool> ShowWait(string data)
    {
        bool reut = false;
        using Semaphore semaphore = new(0, 2);
        Info4Enable = true;
        Info4Text = data;
        Info4CancelVisable = true;

        _info4Call = (res) =>
        {
            reut = res;
            semaphore.Release();
        };

        OnPropertyChanged("Info4Show");

        await Task.Run(() =>
        {
            semaphore.WaitOne();
        });

        _info4Call = null;

        return reut;
    }

    public void Show(string data)
    {
        Info4Enable = true;
        Info4CancelVisable = false;
        _info4Call = null;
        Info4Text = data;

        OnPropertyChanged("Info4Show");
    }

    public void ShowOk(string data, Action action)
    {
        using Semaphore semaphore = new(0, 2);
        Info4Enable = true;
        Info4CancelVisable = false;
        Info4Text = data;

        _info4Call = (res) =>
        {
            action.Invoke();
        };

        OnPropertyChanged("Info4Show");
    }

    public async Task<(bool Cancel, int Index, string? Item)>
        ShowCombo(string data, List<string> data1)
    {
        Info5Text = data;
        Info5Items.Clear();
        Info5Items.AddRange(data1);
        Info5Select = null!;
        Info5Select = data1[0];

        OnPropertyChanged("Info5Show");

        await Task.Run(() =>
        {
            _info5Semaphore.WaitOne();
        });

        return (_info5Cancel, Info5Index, Info5Select);
    }

    public async Task<bool> TextInfo(string data, string data1)
    {
        Info6Text1 = data;
        Info6Text2 = data1;

        await Task.Run(() =>
        {
            _info6Semaphore.WaitOne();
        });

        return _info6Cancel;
    }
}
