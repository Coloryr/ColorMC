using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Model.Setting;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 手柄按钮
/// </summary>
/// <param name="setting"></param>
public partial class InputAxisButtonModel(SettingModel setting) : InputButtonModel(setting)
{
    /// <summary>
    /// 死区开始
    /// </summary>
    [ObservableProperty]
    private short? _start;
    /// <summary>
    /// 死区结束
    /// </summary>
    [ObservableProperty]
    private short? _end;
    /// <summary>
    /// 当前数值
    /// </summary>
    [ObservableProperty]
    private short _nowValue;
    /// <summary>
    /// 是否启用回弹取消
    /// </summary>
    [ObservableProperty]
    private bool _backCancel;

    /// <summary>
    /// 图标
    /// </summary>
    public new string Icon => IconConverter.GetInputAxisIcon(InputKey);

    /// <summary>
    /// 配置UUID
    /// </summary>
    public string UUID;

    /// <summary>
    /// 开始设置
    /// </summary>
    private bool _changeStart;

    /// <summary>
    /// 设置
    /// </summary>
    private readonly SettingModel _setting = setting;

    partial void OnBackCancelChanged(bool value)
    {
        _setting.InputSave(this);
    }

    partial void OnStartChanged(short? value)
    {
        if (_changeStart)
        {
            return;
        }
        _changeStart = true;

        if (value == null)
        {
            Start = 0;
        }

        _changeStart = false;

        _setting.InputSave(this);
    }

    partial void OnEndChanged(short? value)
    {
        if (_changeStart)
        {
            return;
        }
        _changeStart = true;

        if (value == null)
        {
            End = short.MaxValue;
        }

        _changeStart = false;

        _setting.InputSave(this);
    }

    /// <summary>
    /// 生成回配置文件
    /// </summary>
    /// <returns></returns>
    public InputAxisObj GenObj()
    {
        return new(Obj)
        {
            BackCancel = BackCancel,
            Start = Start == null ? (short)0 : (short)Start,
            End = End == null ? short.MaxValue : (short)End,
            InputKey = InputKey
        };
    }
}
