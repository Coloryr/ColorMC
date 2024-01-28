using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Event = Silk.NET.SDL.Event;
using EventType = Silk.NET.SDL.EventType;
using GameControllerAxis = Silk.NET.SDL.GameControllerAxis;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    public ObservableCollection<string> Configs { get; init; } = [];
    public ObservableCollection<InputButtonModel> InputList { get; init; } = [];
    public ObservableCollection<string> InputNames { get; init; } = [];

    public ObservableCollection<InputAxisButtonModel> InputAxisList { get; init; } = [];

    public string[] AxisType { get; init; } = LanguageBinding.GetAxisTypeName();

    [ObservableProperty]
    private InputButtonModel _inputItem;

    [ObservableProperty]
    private InputAxisButtonModel _inputAxisItem;

    [ObservableProperty]
    private bool _inputInit;
    [ObservableProperty]
    private bool _inputExist;
    [ObservableProperty]
    private bool _inputEnable;
    [ObservableProperty]
    private bool _itemCycle;

    [ObservableProperty]
    private int _inputNum;
    [ObservableProperty]
    private int _inputIndex = -1;
    [ObservableProperty]
    private int _inputRotateAxis = 0;
    [ObservableProperty]
    private int _cursorDeath;
    [ObservableProperty]
    private int _rotateDeath;
    [ObservableProperty]
    private int _inputCursorAxis = 0;
    [ObservableProperty]
    private int _toBackValue;

    [ObservableProperty]
    private int _nowConfig = -1;
    [ObservableProperty]
    private int _selectConfig = -1;

    [ObservableProperty]
    private int _nowAxis1;
    [ObservableProperty]
    private int _nowAxis2;

    [ObservableProperty]
    private byte _itemCycleLeft;
    [ObservableProperty]
    private byte _itemCycleRight;

    [ObservableProperty]
    private string _cycleLeftIcon;
    [ObservableProperty]
    private string _cycleRightIcon;

    [ObservableProperty]
    private float _rotateRate;
    [ObservableProperty]
    private float _cursorRate;

    private readonly List<string> UUIDs = [];
    private short leftX, leftY, rightX, rightY;

    private Action<byte>? input;
    private Action<byte, bool>? inputAxis;
    private Action<InputKeyObj>? inputKey;

    private IntPtr controlPtr;
    private int joystickID;

    private InputControlObj? Obj;

    private bool isInputConfigLoad;
    private bool isInputLoad;

    partial void OnNowConfigChanged(int value)
    {
        if (isInputLoad)
        {
            return;
        }
        else if (value == -1)
        {
            ConfigBinding.SaveNowInputConfig(null);
            return;
        }
        else if (UUIDs.Count <= value)
        {
            NowConfig = -1;
            return;
        }

        var uuid = UUIDs[value];

        ConfigBinding.SaveNowInputConfig(uuid);
    }

    partial void OnSelectConfigChanged(int value)
    {
        InputExist = value != -1;
        if (isInputLoad || value == -1)
        {
            return;
        }
        else if (UUIDs.Count <= value)
        {
            SelectConfig = -1;
            return;
        }

        string uuid = UUIDs[value];

        InputConfigUtils.Configs.TryGetValue(uuid, out Obj);
        if (Obj != null)
        {
            LoadInputConfig(Obj);
        }
    }

    partial void OnRotateDeathChanged(int value)
    {
        if (isInputConfigLoad || Obj == null)
        {
            return;
        }

        ConfigBinding.SetInputDeath(Obj, RotateDeath, CursorDeath);
    }

    partial void OnCursorDeathChanged(int value)
    {
        if (isInputConfigLoad || Obj == null)
        {
            return;
        }

        ConfigBinding.SetInputDeath(Obj, RotateDeath, CursorDeath);
    }

    partial void OnCursorRateChanged(float value)
    {
        if (isInputConfigLoad || Obj == null)
        {
            return;
        }

        ConfigBinding.SetInputRate(Obj, RotateRate, CursorRate);
    }

    partial void OnRotateRateChanged(float value)
    {
        if (isInputConfigLoad || Obj == null)
        {
            return;
        }

        ConfigBinding.SetInputRate(Obj, RotateRate, CursorRate);
    }

    partial void OnItemCycleChanged(bool value)
    {
        if (isInputConfigLoad || Obj == null)
        {
            return;
        }

        ConfigBinding.SaveInput(Obj, ItemCycle);
    }

    partial void OnItemCycleLeftChanged(byte value)
    {
        CycleLeftIcon = IconConverter.GetInputKeyIcon(ItemCycleLeft);

        if (isInputConfigLoad || Obj == null)
        {
            return;
        }

        ConfigBinding.SetItemCycle(Obj, ItemCycleLeft, ItemCycleRight);
    }

    partial void OnItemCycleRightChanged(byte value)
    {
        CycleRightIcon = IconConverter.GetInputKeyIcon(ItemCycleRight);

        if (isInputConfigLoad || Obj == null)
        {
            return;
        }

        ConfigBinding.SetItemCycle(Obj, ItemCycleLeft, ItemCycleRight);
    }

    partial void OnInputCursorAxisChanged(int value)
    {
        if (isInputConfigLoad || Obj == null)
        {
            return;
        }

        ConfigBinding.SetInputAxis(Obj, InputRotateAxis, InputCursorAxis);
    }

    partial void OnInputRotateAxisChanged(int value)
    {
        if (isInputConfigLoad || Obj == null)
        {
            return;
        }

        ConfigBinding.SetInputAxis(Obj, InputRotateAxis, InputCursorAxis);
    }

    partial void OnInputIndexChanged(int value)
    {
        InputClose();
        if (value != -1)
        {
            unsafe
            {
                controlPtr = new(InputControlUtils.Open(InputIndex));
            }
            if (controlPtr == IntPtr.Zero)
            {
                Model.Show(App.Lang("SettingWindow.Tab8.Error1"));
            }
            else
            {
                joystickID = InputControlUtils.GetJoystickID(controlPtr);
            }
        }
    }

    partial void OnInputEnableChanged(bool value)
    {
        if (isInputConfigLoad || Obj == null)
        {
            return;
        }

        ConfigBinding.SaveInputInfo(InputEnable);
    }

    [RelayCommand]
    public async Task ExportInputConfig()
    {
        if (Obj == null)
        {
            return;
        }
        var res = await PathBinding.SaveFile(FileType.InputConfig, [Obj.Name, Obj]);
        if (res == null)
        {
            return;
        }
        else if (res != true)
        {
            Model.Show(App.Lang("SettingWindow.Tab8.Error4"));
            return;
        }

        Model.Notify(App.Lang("SettingWindow.Tab8.Info14"));
    }

    [RelayCommand]
    public async Task ImportInputConfig()
    {
        var file = await PathBinding.SelectFile(FileType.InputConfig);
        if (file == null)
        {
            return;
        }

        var obj = InputConfigUtils.Load(file);
        if (obj == null)
        {
            Model.Show(App.Lang("SettingWindow.Tab8.Error2"));
            return;
        }

        obj.UUID = Guid.NewGuid().ToString().ToLower();

        ConfigBinding.SaveInputConfig(obj);
        Configs.Add(obj.Name);
        UUIDs.Add(obj.UUID);

        Model.Notify(App.Lang("SettingWindow.Tab8.Info15"));
    }

    [RelayCommand]
    public async Task DeleteInputConfig()
    {
        if (Obj == null)
        {
            return;
        }

        var res = await Model.ShowWait(string.Format(App.Lang("SettingWindow.Tab8.Info1"), Obj.Name));
        if (!res)
        {
            return;
        }

        ConfigBinding.RemoveInputConfig(Obj);

        UUIDs.Remove(Obj.UUID);
        Configs.Remove(Obj.Name);

        if (Configs.Count > 0)
        {
            SelectConfig = 0;
        }
    }

    [RelayCommand]
    public async Task RenameInputConfig()
    {
        if (Obj == null)
        {
            return;
        }

        var (Cancel, Text1) = await Model.ShowEdit(App.Lang("SettingWindow.Tab8.Info2"), Obj.Name);
        if (Cancel || string.IsNullOrWhiteSpace(Text1))
        {
            return;
        }

        Obj.Name = Text1;
        var last = SelectConfig;
        var now = NowConfig == last;
        Configs[SelectConfig] = Text1;
        SelectConfig = last;
        if (now)
        {
            NowConfig = last;
        }

        ConfigBinding.SaveInputConfig(Obj);
    }

    [RelayCommand]
    public async Task NewInputConfig()
    {
        var (Cancel, Text) = await Model.ShowInputOne(App.Lang("SettingWindow.Tab8.Info2"), false);
        if (Cancel || string.IsNullOrWhiteSpace(Text))
        {
            return;
        }

        var obj = ConfigBinding.NewInput(Text);
        UUIDs.Add(obj.UUID);
        Configs.Add(obj.Name);

        SelectConfig = Configs.Count - 1;
    }

    [RelayCommand]
    public async Task AddAxisInput()
    {
        if (Obj == null)
        {
            return;
        }
        using var cannel = new CancellationTokenSource();
        Model.ShowCancel(App.Lang("SettingWindow.Tab8.Info3"), () =>
        {
            cannel.Cancel();
        });
        var key = await WaitAxis(cannel.Token);
        Model.ShowClose();
        if (key == null)
        {
            return;
        }
        var key1 = ((byte, bool))key;
        Model.ShowCancel(App.Lang("SettingWindow.Tab8.Info4"), () =>
        {
            cannel.Cancel();
        });
        var key2 = await WaitKey(cannel.Token);
        Model.ShowClose();
        if (key2 == null)
        {
            return;
        }
        var item1 = new InputAxisButtonModel(this)
        {
            UUID = Guid.NewGuid().ToString().ToLower(),
            InputKey = key1.Item1,
            Obj = key2,
            Start = key1.Item2 ? (short)2000 : (short)-2000,
            End = key1.Item2 ? short.MaxValue : short.MinValue
        };
        InputAxisList.Add(item1);
        ConfigBinding.AddAxisInput(Obj, item1.UUID, item1.GenObj());
        Model.Notify(App.Lang("SettingWindow.Tab8.Info5"));
    }

    [RelayCommand]
    public async Task AddInput()
    {
        if (Obj == null)
        {
            return;
        }

        using var cannel = new CancellationTokenSource();
        Model.ShowCancel(App.Lang("SettingWindow.Tab8.Info6"), () =>
        {
            cannel.Cancel();
        });
        var key = await WaitInput(cannel.Token);
        Model.ShowClose();
        if (key == null)
        {
            return;
        }
        var key1 = (byte)key;
        Model.ShowCancel(App.Lang("SettingWindow.Tab8.Info4"), () =>
        {
            cannel.Cancel();
        });
        var key2 = await WaitKey(cannel.Token);
        Model.ShowClose();
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
        ConfigBinding.AddInput(Obj, item1.InputKey, item1.Obj);
        Model.Notify(App.Lang("SettingWindow.Tab8.Info7"));
    }

    [RelayCommand]
    public async Task SetItemButton(object? right)
    {
        using var cannel = new CancellationTokenSource();
        Model.ShowCancel(App.Lang("SettingWindow.Tab8.Info6"), () =>
        {
            cannel.Cancel();
        });
        var key = await WaitInput(cannel.Token);
        Model.ShowClose();
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

    public void InputSave(InputAxisButtonModel model)
    {
        if (Obj == null)
        {
            return;
        }

        ConfigBinding.AddAxisInput(Obj, model.UUID, model.GenObj());
    }

    private void StartRead()
    {
        if (InputControlUtils.IsInit)
        {
            InputControlUtils.OnEvent += InputControl_OnEvent;
        }
    }

    private void UpdateType1()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (InputCursorAxis == 0)
            {
                NowAxis1 = Math.Max(leftX, leftY);
            }
            else
            {
                NowAxis1 = Math.Max(rightX, rightY);
            }
        });
    }

    private void UpdateType2()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (InputRotateAxis == 0)
            {
                NowAxis2 = Math.Max(leftX, leftY);
            }
            else
            {
                NowAxis2 = Math.Max(rightX, rightY);
            }
        });
    }

    public void LoadInput()
    {
        isInputLoad = true;
        Configs.Clear();
        UUIDs.Clear();
        InputEnable = GuiConfigUtils.Config.Input.Enable;

        foreach (var item in InputConfigUtils.Configs)
        {
            UUIDs.Add(item.Key);
            Configs.Add(item.Value.Name);
        }

        if (GuiConfigUtils.Config.Input.NowConfig != null)
        {
            NowConfig = UUIDs.IndexOf(GuiConfigUtils.Config.Input.NowConfig);
        }

        isInputLoad = false;

        if (Configs.Count > 0)
        {
            SelectConfig = 0;
        }
    }

    private void LoadInputConfig(InputControlObj config)
    {
        isInputConfigLoad = true;
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
        ToBackValue = config.ToBackValue;

        ItemCycle = config.ItemCycle;
        ItemCycleLeft = config.ItemCycleLeft;
        ItemCycleRight = config.ItemCycleRight;

        isInputConfigLoad = false;
    }

    public void ReloadInput()
    {
        InputNum = InputControlUtils.Count;

        InputNames.Clear();
        InputControlUtils.GetNames().ForEach(InputNames.Add);
        if (InputNames.Count != 0)
        {
            InputIndex = 0;
        }
        else
        {
            InputIndex = -1;
        }
    }

    public void SetTab8Click()
    {
        Model.SetChoiseCall(_name, ReloadInput);
        Model.SetChoiseContent(_name, App.Lang("SettingWindow.Tab8.Info8"));
    }

    public async void SetKeyButton(InputButtonModel item)
    {
        if (Obj == null)
        {
            return;
        }
        using var cannel = new CancellationTokenSource();
        Model.ShowCancel(App.Lang("SettingWindow.Tab8.Info4"), () =>
        {
            cannel.Cancel();
        });
        var key2 = await WaitKey(cannel.Token);
        Model.ShowClose();
        if (key2 == null)
        {
            return;
        }
        item.Obj = key2;
        item.Update();

        if (item is InputAxisButtonModel model)
        {
            ConfigBinding.AddAxisInput(Obj, model.UUID, model.GenObj());
        }
        else
        {
            ConfigBinding.AddInput(Obj, item.InputKey, item.Obj);
        }
        Model.Notify(App.Lang("SettingWindow.Tab8.Info9"));
    }

    public void DeleteInput(InputButtonModel item)
    {
        if (Obj == null)
        {
            return;
        }
        if (item is InputAxisButtonModel model)
        {
            InputAxisList.Remove(model);
            ConfigBinding.DeleteAxisInput(Obj, model.UUID);
        }
        else
        {
            InputList.Remove(item);
            ConfigBinding.DeleteInput(Obj, item.InputKey);
        }
        Model.Notify(App.Lang("SettingWindow.Tab8.Info10"));
    }

    public void InputMouse(KeyModifiers modifiers, PointerPointProperties properties)
    {
        if (inputKey == null)
        {
            return;
        }

        if (properties.IsMiddleButtonPressed)
        {
            inputKey.Invoke(new()
            {
                MouseButton = MouseButton.Middle,
                KeyModifiers = modifiers
            });
        }
        else if (properties.IsRightButtonPressed)
        {
            inputKey.Invoke(new()
            {
                MouseButton = MouseButton.Right,
                KeyModifiers = modifiers
            });
        }
        else if (properties.IsLeftButtonPressed)
        {
            inputKey.Invoke(new()
            {
                MouseButton = MouseButton.Left,
                KeyModifiers = modifiers
            });
        }
        else if (properties.IsXButton1Pressed)
        {
            inputKey.Invoke(new()
            {
                MouseButton = MouseButton.XButton1,
                KeyModifiers = modifiers
            });
        }
        else if (properties.IsXButton2Pressed)
        {
            inputKey.Invoke(new()
            {
                MouseButton = MouseButton.XButton2,
                KeyModifiers = modifiers
            });
        }
    }

    public bool InputKey(KeyModifiers modifiers, Key key)
    {
        if (inputKey == null)
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

        inputKey?.Invoke(new()
        {
            Key = key,
            KeyModifiers = modifiers
        });

        return true;
    }

    private Task<InputKeyObj?> WaitKey(CancellationToken token)
    {
        InputKeyObj? keys = null;
        bool output = false;
        inputKey = (key) =>
        {
            inputKey = null;
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

    private Task<byte?> WaitInput(CancellationToken token)
    {
        byte? keys = null;
        bool output = false;
        input = (key) =>
        {
            input = null;
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

    public Task<(byte, bool)?> WaitAxis(CancellationToken token)
    {
        byte keys = 0;
        bool output = false;
        bool positives = false;
        inputAxis = (key, positive) =>
        {
            inputAxis = null;
            positives = positive;
            keys = key;
            output = true;
        };
        return Task.Run<(byte, bool)?>(() =>
        {
            while (!output)
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }
                System.Threading.Thread.Sleep(100);
            }

            return (keys, positives);
        });
    }

    private void InputControl_OnEvent(Event sdlEvent)
    {
        EventType type = (EventType)sdlEvent.Type;
        if (type is EventType.Controllerdeviceadded
            or EventType.Controllerdeviceremoved)
        {
            ReloadInput();
            return;
        }

        if (sdlEvent.Cbutton.Which != joystickID)
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
                leftX = axisFixValue;
                UpdateType1();
            }
            else if (axisEvent.Axis == (uint)GameControllerAxis.Lefty)
            {
                leftY = axisFixValue;
                UpdateType1();
            }
            else if (axisEvent.Axis == (uint)GameControllerAxis.Rightx)
            {
                rightX = axisFixValue;
                UpdateType2();
            }
            else if (axisEvent.Axis == (uint)GameControllerAxis.Righty)
            {
                rightY = axisFixValue;
                UpdateType2();
            }

            if (axisFixValue > 2000)
            {
                inputAxis?.Invoke(sdlEvent.Caxis.Axis, axisValue > 0);
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
            input?.Invoke(sdlEvent.Cbutton.Button);
        }
    }

    private void InputClose()
    {
        joystickID = 0;
        if (controlPtr != IntPtr.Zero)
        {
            InputControlUtils.Close(controlPtr);
            controlPtr = IntPtr.Zero;
        }
    }

    private void StopRead()
    {
        if (InputControlUtils.IsInit)
        {
            InputControlUtils.OnEvent -= InputControl_OnEvent;
        }
    }
}
