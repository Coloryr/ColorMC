using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Event = Silk.NET.SDL.Event;
using EventType = Silk.NET.SDL.EventType;
using GameControllerAxis = Silk.NET.SDL.GameControllerAxis;

namespace ColorMC.Gui.UI.Model.Setting;

/// <summary>
/// 设置页面
/// </summary>
public partial class SettingModel
{
    /// <summary>
    /// 手柄配置列表
    /// </summary>
    public ObservableCollection<string> Configs { get; init; } = [];
    /// <summary>
    /// 手柄按钮列表
    /// </summary>
    public ObservableCollection<InputButtonModel> InputList { get; init; } = [];
    /// <summary>
    /// 手柄列表
    /// </summary>
    public ObservableCollection<string> InputNames { get; init; } = [];
    /// <summary>
    /// 摇杆列表
    /// </summary>
    public ObservableCollection<InputAxisButtonModel> InputAxisList { get; init; } = [];

    /// <summary>
    /// 摇杆类型
    /// </summary>
    public string[] AxisType { get; init; } = LanguageUtils.GetAxisTypeName();

    /// <summary>
    /// 选中的按钮
    /// </summary>
    [ObservableProperty]
    private InputButtonModel _inputItem;
    /// <summary>
    /// 选中的摇杆
    /// </summary>
    [ObservableProperty]
    private InputAxisButtonModel _inputAxisItem;

    /// <summary>
    /// 是否初始化了手柄输入
    /// </summary>
    [ObservableProperty]
    private bool _inputInit;
    /// <summary>
    /// 是否存在手柄
    /// </summary>
    [ObservableProperty]
    private bool _inputExist;
    /// <summary>
    /// 是否启用手柄映射
    /// </summary>
    [ObservableProperty]
    private bool _inputEnable;
    /// <summary>
    /// 是否启用物品循环
    /// </summary>
    [ObservableProperty]
    private bool _itemCycle;
    /// <summary>
    /// 是否启用物品循环
    /// </summary>
    [ObservableProperty]
    private bool _inputDisable;

    /// <summary>
    /// 手柄数量
    /// </summary>
    [ObservableProperty]
    private int _inputNum;
    /// <summary>
    /// 选中的手柄
    /// </summary>
    [ObservableProperty]
    private int _inputIndex = -1;
    /// <summary>
    /// 选中的摇杆
    /// </summary>
    [ObservableProperty]
    private int _inputRotateAxis = 0;
    /// <summary>
    /// 光标摇杆死区大小
    /// </summary>
    [ObservableProperty]
    private int _cursorDeath;
    /// <summary>
    /// 移动摇杆死区大小
    /// </summary>
    [ObservableProperty]
    private int _rotateDeath;
    /// <summary>
    /// 光标摇杆
    /// </summary>
    [ObservableProperty]
    private int _inputCursorAxis = 0;
    /// <summary>
    /// 回滚值
    /// </summary>
    [ObservableProperty]
    private int _toBackValue;

    /// <summary>
    /// 当前配置
    /// </summary>
    [ObservableProperty]
    private int _nowConfig = -1;
    /// <summary>
    /// 选中的配置
    /// </summary>
    [ObservableProperty]
    private int _selectConfig = -1;
    /// <summary>
    /// 选中的摇杆
    /// </summary>
    [ObservableProperty]
    private int _nowAxis1;
    /// <summary>
    /// 选中的摇杆
    /// </summary>
    [ObservableProperty]
    private int _nowAxis2;

    /// <summary>
    /// 物品循环按钮
    /// </summary>
    [ObservableProperty]
    private byte _itemCycleLeft;
    /// <summary>
    /// 物品循环按钮
    /// </summary>
    [ObservableProperty]
    private byte _itemCycleRight;

    /// <summary>
    /// 物品循环图标
    /// </summary>
    [ObservableProperty]
    private string _cycleLeftIcon;
    /// <summary>
    /// 物品循环图标
    /// </summary>
    [ObservableProperty]
    private string _cycleRightIcon;

    /// <summary>
    /// 移动倍率
    /// </summary>
    [ObservableProperty]
    private float _rotateRate;
    /// <summary>
    /// 光标倍率
    /// </summary>
    [ObservableProperty]
    private float _cursorRate;
    /// <summary>
    /// 降速
    /// </summary>
    [ObservableProperty]
    private float _downRate;

    /// <summary>
    /// 控制器列表
    /// </summary>
    private readonly List<string> _controlUUIDs = [];
    /// <summary>
    /// 当前数值
    /// </summary>
    private short _leftX, _leftY, _rightX, _rightY;

    //数值更新回调
    private Action<byte>? _input;
    private Action<byte, bool>? _inputAxis;
    private Action<InputKeyObj>? _inputKey;

    //当前手柄
    private IntPtr _controlPtr;
    private int _joystickID;

    /// <summary>
    /// 当前控制器
    /// </summary>
    private InputControlObj? _controlObj;

    //是否加载配置中
    private bool _isInputConfigLoad;
    private bool _isInputLoad;

    //配置修改
    partial void OnNowConfigChanged(int value)
    {
        if (_isInputLoad)
        {
            return;
        }
        else if (value == -1)
        {
            ConfigBinding.SaveNowInputConfig(null);
            return;
        }
        else if (_controlUUIDs.Count <= value)
        {
            NowConfig = -1;
            return;
        }

        var uuid = _controlUUIDs[value];

        ConfigBinding.SaveNowInputConfig(uuid);
    }

    partial void OnSelectConfigChanged(int value)
    {
        if (InputDisable)
        {
            InputExist = false;
        }
        else
        {
            InputExist = value != -1;
        }
        if (_isInputLoad || value == -1)
        {
            return;
        }
        else if (_controlUUIDs.Count <= value)
        {
            SelectConfig = -1;
            return;
        }

        string uuid = _controlUUIDs[value];

        JoystickConfig.Configs.TryGetValue(uuid, out _controlObj);
        if (_controlObj != null)
        {
            LoadInputConfig(_controlObj);
        }
    }

    partial void OnRotateDeathChanged(int value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetInputDeath(_controlObj, RotateDeath, CursorDeath);
    }

    partial void OnCursorDeathChanged(int value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetInputDeath(_controlObj, RotateDeath, CursorDeath);
    }

    partial void OnDownRateChanged(float value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetInputRate(_controlObj, RotateRate, CursorRate, DownRate);
    }

    partial void OnCursorRateChanged(float value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetInputRate(_controlObj, RotateRate, CursorRate, DownRate);
    }

    partial void OnRotateRateChanged(float value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetInputRate(_controlObj, RotateRate, CursorRate, DownRate);
    }

    partial void OnItemCycleChanged(bool value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SaveInput(_controlObj, ItemCycle);
    }

    partial void OnItemCycleLeftChanged(byte value)
    {
        CycleLeftIcon = IconConverter.GetInputKeyIcon(ItemCycleLeft);

        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetItemCycle(_controlObj, ItemCycleLeft, ItemCycleRight);
    }

    partial void OnItemCycleRightChanged(byte value)
    {
        CycleRightIcon = IconConverter.GetInputKeyIcon(ItemCycleRight);

        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetItemCycle(_controlObj, ItemCycleLeft, ItemCycleRight);
    }

    partial void OnInputCursorAxisChanged(int value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetInputAxis(_controlObj, InputRotateAxis, InputCursorAxis);
    }

    partial void OnInputRotateAxisChanged(int value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetInputAxis(_controlObj, InputRotateAxis, InputCursorAxis);
    }

    partial void OnInputIndexChanged(int value)
    {
        InputClose();
        if (value != -1)
        {
            unsafe
            {
                _controlPtr = new(JoystickInput.Open(InputIndex));
            }
            if (_controlPtr == IntPtr.Zero)
            {
                Window.Show(LanguageUtils.Get("SettingWindow.Tab8.Error1"));
            }
            else
            {
                _joystickID = JoystickInput.GetJoystickInstanceID(_controlPtr);
            }
        }
    }

    partial void OnInputDisableChanged(bool value)
    {
        if (_isInputLoad)
        {
            return;
        }

        ConfigBinding.SaveInputInfo(InputEnable, InputDisable);
        ColorMCGui.Reboot();
    }

    partial void OnInputEnableChanged(bool value)
    {
        if (_isInputLoad)
        {
            return;
        }

        ConfigBinding.SaveInputInfo(InputEnable, InputDisable);
    }

    /// <summary>
    /// 导出手柄设置
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task ExportInputConfig()
    {
        if (_controlObj == null)
        {
            return;
        }
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.SaveFileAsync(top, FileType.InputConfig, [_controlObj.Name, _controlObj]);
        if (res == null)
        {
            return;
        }
        else if (res != true)
        {
            Window.Show(LanguageUtils.Get("SettingWindow.Tab8.Error4"));
            return;
        }

        Window.Notify(LanguageUtils.Get("SettingWindow.Tab8.Info14"));
    }
    /// <summary>
    /// 导入手柄设置
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task ImportInputConfig()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFileAsync(top, FileType.InputConfig);
        if (file.Path == null)
        {
            return;
        }

        var obj = JoystickConfig.Load(file.Path);
        if (obj == null)
        {
            Window.Show(LanguageUtils.Get("SettingWindow.Tab8.Error2"));
            return;
        }

        obj.UUID = Guid.NewGuid().ToString().ToLower();

        ConfigBinding.SaveInputConfig(obj);
        Configs.Add(obj.Name);
        _controlUUIDs.Add(obj.UUID);

        Window.Notify(LanguageUtils.Get("SettingWindow.Tab8.Info15"));
    }
    /// <summary>
    /// 删除手柄设置
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task DeleteInputConfig()
    {
        if (_controlObj == null)
        {
            return;
        }

        var res = await Window.ShowChoice(string.Format(LanguageUtils.Get("SettingWindow.Tab8.Info1"), _controlObj.Name));
        if (!res)
        {
            return;
        }

        ConfigBinding.RemoveInputConfig(_controlObj);

        _controlUUIDs.Remove(_controlObj.UUID);
        Configs.Remove(_controlObj.Name);

        if (Configs.Count > 0)
        {
            SelectConfig = 0;
        }
    }
    /// <summary>
    /// 重命名手柄设置
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task RenameInputConfig()
    {
        if (_controlObj == null)
        {
            return;
        }

        var dialog = new InputModel(Window.WindowId)
        {
            Watermark1 = LanguageUtils.Get("SettingWindow.Tab8.Info2"),
            Text1 = _controlObj.Name
        };
        var res = await Window.ShowDialogWait(dialog);
        if (res is not true || string.IsNullOrWhiteSpace(dialog.Text1))
        {
            return;
        }

        _controlObj.Name = dialog.Text1;
        var last = SelectConfig;
        var now = NowConfig == last;
        Configs[SelectConfig] = dialog.Text1;
        SelectConfig = last;
        if (now)
        {
            NowConfig = last;
        }

        ConfigBinding.SaveInputConfig(_controlObj);
    }
    /// <summary>
    /// 新建手柄设置
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task NewInputConfig()
    {
        var dialog = new InputModel(Window.WindowId)
        {
            Watermark1 = LanguageUtils.Get("SettingWindow.Tab8.Info2")
        };
        var res = await Window.ShowDialogWait(dialog);
        if (res is not true || string.IsNullOrWhiteSpace(dialog.Text1))
        {
            return;
        }

        var obj = ConfigBinding.NewInput(dialog.Text1);
        _controlUUIDs.Add(obj.UUID);
        Configs.Add(obj.Name);

        SelectConfig = Configs.Count - 1;
    }
    /// <summary>
    /// 添加摇杆输入
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddAxisInput()
    {
        if (_controlObj == null)
        {
            return;
        }
        using var cannel = new CancellationTokenSource();
        var dialog = new ChoiceModel(Window.WindowId)
        {
            Text = LanguageUtils.Get("SettingWindow.Tab8.Info3"),
            ChoiceText = LanguageUtils.Get("Button.Cancel"),
            ChoiceVisiable = true,
            ChoiceCall = cannel.Cancel
        };
        Window.ShowDialog(dialog);
        var key = await WaitAxis(cannel.Token);
        Window.CloseDialog(dialog);
        if (key == null)
        {
            return;
        }
        dialog = new ChoiceModel(Window.WindowId)
        {
            Text = LanguageUtils.Get("SettingWindow.Tab8.Info4"),
            ChoiceText = LanguageUtils.Get("Button.Cancel"),
            ChoiceVisiable = true,
            ChoiceCall = cannel.Cancel
        };
        Window.ShowDialog(dialog);
        var key2 = await WaitKey(cannel.Token);
        Window.CloseDialog(dialog);
        if (key2 == null)
        {
            return;
        }
        var item1 = new InputAxisButtonModel(this)
        {
            UUID = Guid.NewGuid().ToString().ToLower()[..8],
            InputKey = key.Key,
            Obj = key2,
            Start = key.Positive ? (short)2000 : (short)-2000,
            End = key.Positive ? short.MaxValue : short.MinValue
        };
        InputAxisList.Add(item1);
        ConfigBinding.AddAxisInput(_controlObj, item1.UUID, item1.GenObj());
        Window.Notify(LanguageUtils.Get("SettingWindow.Tab8.Info5"));
    }
    /// <summary>
    /// 添加按钮输入
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddInput()
    {
        if (_controlObj == null)
        {
            return;
        }

        using var cannel = new CancellationTokenSource();
        var dialog = new ChoiceModel(Window.WindowId)
        {
            Text = LanguageUtils.Get("SettingWindow.Tab8.Info6"),
            ChoiceText = LanguageUtils.Get("Button.Cancel"),
            ChoiceVisiable = true,
            ChoiceCall = cannel.Cancel
        };
        Window.ShowDialog(dialog);
        var key = await WaitInput(cannel.Token);
        Window.CloseDialog(dialog);
        if (key == null)
        {
            return;
        }
        var key1 = (byte)key;
        dialog = new ChoiceModel(Window.WindowId)
        {
            Text = LanguageUtils.Get("SettingWindow.Tab8.Info4"),
            ChoiceText = LanguageUtils.Get("Button.Cancel"),
            ChoiceVisiable = true,
            ChoiceCall = cannel.Cancel
        };
        Window.ShowDialog(dialog);
        var key2 = await WaitKey(cannel.Token);
        Window.CloseDialog(dialog);
        if (key2 == null)
        {
            return;
        }
        foreach (var item in InputList)
        {
            if (item.InputKey == key1)
            {
                InputList.Remove(item);
                break;
            }
        }
        var item1 = new InputButtonModel(this)
        {
            InputKey = key1,
            Obj = key2
        };
        InputList.Add(item1);
        ConfigBinding.AddInput(_controlObj, item1.InputKey, item1.Obj);
        Window.Notify(LanguageUtils.Get("SettingWindow.Tab8.Info7"));
    }
    /// <summary>
    /// 设置按钮
    /// </summary>
    /// <param name="right"></param>
    /// <returns></returns>
    [RelayCommand]
    public async Task SetItemButton(object? right)
    {
        using var cannel = new CancellationTokenSource();
        var dialog = new ChoiceModel(Window.WindowId)
        {
            Text = LanguageUtils.Get("SettingWindow.Tab8.Info6"),
            ChoiceText = LanguageUtils.Get("Button.Cancel"),
            ChoiceVisiable = true,
            ChoiceCall = cannel.Cancel
        };
        Window.ShowDialog(dialog);
        var key = await WaitInput(cannel.Token);
        Window.CloseDialog(dialog);
        if (key == null)
        {
            return;
        }
        var key1 = (byte)key;

        if (right is bool value && value)
        {
            ItemCycleRight = key1;
        }
        else
        {
            ItemCycleLeft = key1;
        }
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    /// <param name="model"></param>
    public void InputSave(InputAxisButtonModel model)
    {
        if (_controlObj == null)
        {
            return;
        }

        ConfigBinding.AddAxisInput(_controlObj, model.UUID, model.GenObj());
    }
    /// <summary>
    /// 开始读取手柄
    /// </summary>
    private void StartRead()
    {
        if (GuiConfigUtils.Config.Input.Disable)
        {
            return;
        }

        JoystickInput.OnEvent += InputControl_OnEvent;
    }
    /// <summary>
    /// 更新摇杆值
    /// </summary>
    private void UpdateType1()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (InputCursorAxis == 0)
            {
                NowAxis1 = Math.Max(_leftX, _leftY);
            }
            else
            {
                NowAxis1 = Math.Max(_rightX, _rightY);
            }
        });
    }
    /// <summary>
    /// 更新摇杆值
    /// </summary>
    private void UpdateType2()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (InputRotateAxis == 0)
            {
                NowAxis2 = Math.Max(_leftX, _leftY);
            }
            else
            {
                NowAxis2 = Math.Max(_rightX, _rightY);
            }
        });
    }

    /// <summary>
    /// 开始读取配置
    /// </summary>
    public void LoadInput()
    {
        _isInputLoad = true;
        Configs.Clear();
        _controlUUIDs.Clear();

        InputEnable = GuiConfigUtils.Config.Input.Enable;
        InputDisable = GuiConfigUtils.Config.Input.Disable;

        if (SystemInfo.Os is OsType.Windows && !InputDisable)
        {
            IsInputEnable = true;
        }
        else
        {
            IsInputEnable = false;
        }

        foreach (var item in JoystickConfig.Configs)
        {
            _controlUUIDs.Add(item.Key);
            Configs.Add(item.Value.Name);
        }

        if (GuiConfigUtils.Config.Input.NowConfig != null)
        {
            NowConfig = _controlUUIDs.IndexOf(GuiConfigUtils.Config.Input.NowConfig);
        }

        _isInputLoad = false;

        if (Configs.Count > 0)
        {
            SelectConfig = 0;
        }
    }

    /// <summary>
    /// 加载手柄设置
    /// </summary>
    /// <param name="config">手柄设置</param>
    private void LoadInputConfig(InputControlObj config)
    {
        _isInputConfigLoad = true;
        InputList.Clear();
        InputAxisList.Clear();

        foreach (var item in config.Keys)
        {
            InputList.Add(new(this)
            {
                InputKey = item.Key,
                Obj = item.Value
            });
        }

        foreach (var item in config.AxisKeys)
        {
            InputAxisList.Add(new(this)
            {
                InputKey = item.Value.InputKey,
                UUID = item.Key,
                Obj = item.Value,
                Start = item.Value.Start,
                End = item.Value.End
            });
        }

        InputCursorAxis = config.CursorAxis;
        InputRotateAxis = config.RotateAxis;

        CursorDeath = config.CursorDeath;
        RotateDeath = config.RotateDeath;

        CursorRate = config.CursorRate;
        RotateRate = config.RotateRate;
        DownRate = config.DownRate;
        ToBackValue = config.ToBackValue;

        ItemCycle = config.ItemCycle;
        ItemCycleLeft = config.ItemCycleLeft;
        ItemCycleRight = config.ItemCycleRight;

        _isInputConfigLoad = false;
    }

    /// <summary>
    /// 重载手柄列表
    /// </summary>
    public void ReloadInput()
    {
        if (!InputInit || GuiConfigUtils.Config.Input.Disable)
        {
            return;
        }
        InputNum = JoystickInput.Count;

        InputNames.Clear();
        JoystickInput.GetNames().ForEach(InputNames.Add);
        if (InputNames.Count != 0)
        {
            InputIndex = 0;
        }
        else
        {
            InputIndex = -1;
        }
    }

    /// <summary>
    /// 设置手柄配置按钮
    /// </summary>
    /// <param name="item"></param>
    public async void SetKeyButton(InputButtonModel item)
    {
        if (_controlObj == null)
        {
            return;
        }
        using var cannel = new CancellationTokenSource();
        var dialog = new ChoiceModel(Window.WindowId)
        {
            Text = LanguageUtils.Get("SettingWindow.Tab8.Info4"),
            ChoiceText = LanguageUtils.Get("Button.Cancel"),
            ChoiceVisiable = true,
            ChoiceCall = cannel.Cancel
        };
        Window.ShowDialog(dialog);
        var key2 = await WaitKey(cannel.Token);
        Window.CloseDialog(dialog);
        if (key2 == null)
        {
            return;
        }
        item.Obj = key2;
        item.Update();

        if (item is InputAxisButtonModel model)
        {
            ConfigBinding.AddAxisInput(_controlObj, model.UUID, model.GenObj());
        }
        else
        {
            ConfigBinding.AddInput(_controlObj, item.InputKey, item.Obj);
        }
        Window.Notify(LanguageUtils.Get("SettingWindow.Tab8.Info9"));
    }

    /// <summary>
    /// 删除手柄配置按钮
    /// </summary>
    /// <param name="item"></param>
    public void DeleteInput(InputButtonModel item)
    {
        if (_controlObj == null)
        {
            return;
        }
        if (item is InputAxisButtonModel model)
        {
            InputAxisList.Remove(model);
            ConfigBinding.DeleteAxisInput(_controlObj, model.UUID);
        }
        else
        {
            InputList.Remove(item);
            ConfigBinding.DeleteInput(_controlObj, item.InputKey);
        }
        Window.Notify(LanguageUtils.Get("SettingWindow.Tab8.Info10"));
    }

    /// <summary>
    /// 输入鼠标按钮
    /// </summary>
    /// <param name="modifiers"></param>
    /// <param name="properties"></param>
    public void InputMouse(KeyModifiers modifiers, PointerPointProperties properties)
    {
        if (_inputKey == null)
        {
            return;
        }

        if (properties.IsMiddleButtonPressed)
        {
            _inputKey.Invoke(new()
            {
                MouseButton = MouseButton.Middle,
                KeyModifiers = modifiers
            });
        }
        else if (properties.IsRightButtonPressed)
        {
            _inputKey.Invoke(new()
            {
                MouseButton = MouseButton.Right,
                KeyModifiers = modifiers
            });
        }
        else if (properties.IsLeftButtonPressed)
        {
            _inputKey.Invoke(new()
            {
                MouseButton = MouseButton.Left,
                KeyModifiers = modifiers
            });
        }
        else if (properties.IsXButton1Pressed)
        {
            _inputKey.Invoke(new()
            {
                MouseButton = MouseButton.XButton1,
                KeyModifiers = modifiers
            });
        }
        else if (properties.IsXButton2Pressed)
        {
            _inputKey.Invoke(new()
            {
                MouseButton = MouseButton.XButton2,
                KeyModifiers = modifiers
            });
        }
    }

    /// <summary>
    /// 输入键盘按钮
    /// </summary>
    /// <param name="modifiers"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool InputKey(KeyModifiers modifiers, Key key)
    {
        if (_inputKey == null)
        {
            return false;
        }

        if (key is Key.LeftShift or Key.RightShift && modifiers == KeyModifiers.Shift)
        {
            modifiers = KeyModifiers.None;
        }
        else if (key is Key.LeftCtrl or Key.RightCtrl && modifiers == KeyModifiers.Control)
        {
            modifiers = KeyModifiers.None;
        }
        else if (key is Key.LeftAlt or Key.RightAlt && modifiers == KeyModifiers.Alt)
        {
            modifiers = KeyModifiers.None;
        }

        _inputKey?.Invoke(new()
        {
            Key = key,
            KeyModifiers = modifiers
        });

        return true;
    }

    /// <summary>
    /// 等待用户输入按钮
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private Task<InputKeyObj?> WaitKey(CancellationToken token)
    {
        JoystickInput.IsEditMode = true;
        InputKeyObj? keys = null;
        bool output = false;
        _inputKey = (key) =>
        {
            _inputKey = null;
            keys = key;
            output = true;
        };
        return Task.Run(() =>
        {
            while (!output)
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }
                System.Threading.Thread.Sleep(100);

                JoystickInput.IsEditMode = false;
            }

            return keys;
        });
    }
    /// <summary>
    /// 等待用户输入按钮
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private Task<byte?> WaitInput(CancellationToken token)
    {
        byte? keys = null;
        bool output = false;
        _input = (key) =>
        {
            _input = null;
            keys = key;
            output = true;
        };
        return Task.Run(() =>
        {
            while (!output)
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }
                System.Threading.Thread.Sleep(100);
            }

            return keys;
        });
    }
    /// <summary>
    /// 等待用户摇杆输入
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<KeyRes?> WaitAxis(CancellationToken token)
    {
        byte keys = 0;
        bool output = false;
        bool positives = false;
        _inputAxis = (key, positive) =>
        {
            _inputAxis = null;
            positives = positive;
            keys = key;
            output = true;
        };
        return Task.Run(() =>
        {
            while (!output)
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }
                System.Threading.Thread.Sleep(100);
            }

            return new KeyRes
            { 
                Key = keys,
                Positive = positives
            };
        });
    }

    /// <summary>
    /// 手柄事件
    /// </summary>
    /// <param name="sdlEvent"></param>
    private void InputControl_OnEvent(Event sdlEvent)
    {
        EventType type = (EventType)sdlEvent.Type;
        if (type is EventType.Controllerdeviceadded
            or EventType.Controllerdeviceremoved)
        {
            ReloadInput();
            return;
        }

        if (sdlEvent.Cbutton.Which != _joystickID)
        {
            return;
        }

        if (type == EventType.Controlleraxismotion)
        {
            var axisEvent = sdlEvent.Caxis;
            var axisValue = axisEvent.Value;

            short axisFixValue;
            if (axisValue == short.MinValue)
            {
                axisFixValue = short.MaxValue;
            }
            else
            {
                axisFixValue = Math.Abs(axisValue);
            }

            if (axisEvent.Axis == (uint)GameControllerAxis.Leftx)
            {
                _leftX = axisFixValue;
                UpdateType1();
            }
            else if (axisEvent.Axis == (uint)GameControllerAxis.Lefty)
            {
                _leftY = axisFixValue;
                UpdateType1();
            }
            else if (axisEvent.Axis == (uint)GameControllerAxis.Rightx)
            {
                _rightX = axisFixValue;
                UpdateType2();
            }
            else if (axisEvent.Axis == (uint)GameControllerAxis.Righty)
            {
                _rightY = axisFixValue;
                UpdateType2();
            }

            if (axisFixValue > 2000)
            {
                _inputAxis?.Invoke(sdlEvent.Caxis.Axis, axisValue > 0);
            }

            Dispatcher.UIThread.Post(() =>
            {
                foreach (var item in InputAxisList)
                {
                    if (item.InputKey == axisEvent.Axis)
                    {
                        item.NowValue = axisValue;
                    }
                }
            });
        }
        else if (type == EventType.Controllerbuttondown)
        {
            _input?.Invoke(sdlEvent.Cbutton.Button);
        }
    }

    /// <summary>
    /// 关闭当前手柄
    /// </summary>
    private void InputClose()
    {
        _joystickID = 0;
        if (_controlPtr != IntPtr.Zero)
        {
            JoystickInput.Close(_controlPtr);
            _controlPtr = IntPtr.Zero;
        }
    }

    /// <summary>
    /// 停止获取手柄输入
    /// </summary>
    private void StopRead()
    {
        JoystickInput.OnEvent -= InputControl_OnEvent;
    }
}
