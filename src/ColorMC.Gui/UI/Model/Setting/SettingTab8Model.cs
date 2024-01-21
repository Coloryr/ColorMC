using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Silk.NET.SDL;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Semaphore = System.Threading.Semaphore;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    public ObservableCollection<InputButtonModel> InputList { get; init; } =[];
    public ObservableCollection<string> InputNames { get; init; } = [];

    public ObservableCollection<InputAxisButtonModel> InputAxisList { get; init; } = [];

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
    private int _inputNum;
    [ObservableProperty]
    private int _inputIndex = -1;

    private readonly Semaphore semaphore = new(0, 2);

    private InputKeyObj _output;
    private bool isInput;

    private IntPtr ptr;
    private CancellationTokenSource? readCancel;

    private bool isInputLoad;

    partial void OnInputIndexChanged(int value)
    {
        InputExist = value != -1;
        InputClose();
        if (value != -1)
        {
            unsafe
            {
                ptr = new(InputControl.Open(InputIndex));
            }
            if (ptr == IntPtr.Zero)
            {
                Model.Show("手柄打开失败");
            }
            else
            {
                StartRead();
            }
        }
    }

    partial void OnInputEnableChanged(bool value)
    {
        if (isInputLoad)
        {
            return;
        }

        ConfigBinding.SaveInput(InputEnable);
    }

    [RelayCommand]
    public async Task AddAxisInput()
    {
        using var cannel = new CancellationTokenSource();
        Model.ShowCancel("请按下手柄扳机来绑定", () =>
        {
            cannel.Cancel();
        });
        var key = await InputControl.WaitAxis(ptr, cannel.Token);
        Model.ShowClose();
        if (key == null)
        {
            return;
        }
        var key1 = (byte)key;
        Model.ShowCancel("请按下键盘或鼠标按键来绑定", () =>
        {
            isInput = false;
            cannel.Cancel();
            semaphore.Release();
        });
        var key2 = await ReadKey(cannel.Token);
        Model.ShowClose();
        if (key2 == null)
        {
            return;
        }
        var item1 = new InputAxisButtonModel(this)
        {
            UUID = Guid.NewGuid().ToString().ToLower(),
            InputKey = key1,
            Obj = key2,
            Start = 0,
            End = short.MaxValue
        };
        InputAxisList.Add(item1);
        ConfigBinding.AddAxisInput(item1.UUID, item1.GenoObj());
        Model.Notify("已添加");
    }

    [RelayCommand]
    public async Task AddInput()
    {
        using var cannel = new CancellationTokenSource();
        Model.ShowCancel("请按下手柄按键来绑定", () => 
        {
            cannel.Cancel();
        });
        var key = await InputControl.WaitInput(ptr, cannel.Token);
        Model.ShowClose();
        if (key == null)
        {
            return;
        }
        var key1 = (byte)key;
        Model.ShowCancel("请按下键盘或鼠标按键来绑定", () =>
        {
            isInput = false;
            cannel.Cancel();
            semaphore.Release();
        });
        var key2 = await ReadKey(cannel.Token);
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
        ConfigBinding.AddInput(item1.InputKey, item1.Obj);
        Model.Notify("已添加");
    }

    private void StartRead()
    {
        readCancel = new();
        InputControl.StartRead(ReadData, ptr, readCancel.Token);
    }

    private void ReadData(Event sdlEvent)
    {
        if (sdlEvent.Type == (uint)EventType.Controlleraxismotion)
        {
            var axisEvent = sdlEvent.Caxis;
            var axisValue = axisEvent.Value;

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
    }

    public void LoadInput()
    {
        isInputLoad = true;

        var config = GuiConfigUtils.Config.Input;
        InputEnable = config.Enable;

        InputList.Clear();
        foreach (var item in config.Keys)
        {
            InputList.Add(new(this)
            {
                InputKey = item.Key,
                Obj = item.Value
            });
        }

        InputAxisList.Clear();
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


        isInputLoad = false;

        if (!InputControl.IsInit)
        {
            InputInit = false;
        }

        InputInit = true;

        InputNum = InputControl.Count;

        ReloadInput();
    }

    public void ReloadInput()
    {
        InputNames.Clear();
        InputControl.GetNames().ForEach(InputNames.Add);
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
        Model.SetChoiseContent(_name, "刷新手柄");
    }

    private async Task<InputKeyObj?> ReadKey(CancellationToken token)
    {
        await Task.Run(() =>
        {
            isInput = true;
            semaphore.WaitOne();
        }, token);

        if (token.IsCancellationRequested)
        {
            return null;
        }

        return _output;
    }

    public async void BindInput(InputButtonModel item)
    {
        using var cannel = new CancellationTokenSource();
        Model.ShowCancel("请按下键盘或鼠标按键来绑定", () =>
        {
            isInput = false;
            cannel.Cancel();
            semaphore.Release();
        });
        var key2 = await ReadKey(cannel.Token);
        Model.ShowClose();
        if (key2 == null)
        {
            return;
        }
        item.Obj = key2;
        item.Update();

        if (item is InputAxisButtonModel model)
        {
            ConfigBinding.AddAxisInput(model.UUID, model.GenoObj());
        }
        else
        {
            ConfigBinding.AddInput(item.InputKey, item.Obj);
        }
        Model.Notify("已修改");
    }

    public void DeleteInput(InputButtonModel item)
    {
        if (item is InputAxisButtonModel model)
        {
            InputAxisList.Remove(model);
            ConfigBinding.DeleteAxisInput(model.UUID);
        }
        else
        {
            InputList.Remove(item);
            ConfigBinding.DeleteInput(item.InputKey);
        }
        Model.Notify("已删除");
    }

    public void InputMouse(KeyModifiers modifiers, PointerPointProperties properties)
    {
        if (!isInput)
        {
            return;
        }
        isInput = false;

        if (properties.IsMiddleButtonPressed)
        {
            _output = new()
            {
                MouseButton = MouseButton.Middle,
                KeyModifiers = modifiers
            };
        }
        else if (properties.IsRightButtonPressed)
        {
            _output = new()
            {
                MouseButton = MouseButton.Right,
                KeyModifiers = modifiers
            };
        }
        else if (properties.IsLeftButtonPressed)
        {
            _output = new()
            {
                MouseButton = MouseButton.Left,
                KeyModifiers = modifiers
            };
        }
        else if (properties.IsXButton1Pressed)
        {
            _output = new()
            {
                MouseButton = MouseButton.XButton1,
                KeyModifiers = modifiers
            };
        }
        else if (properties.IsXButton2Pressed)
        {
            _output = new()
            {
                MouseButton = MouseButton.XButton2,
                KeyModifiers = modifiers
            };
        }

        semaphore.Release();
    }

    public bool InputKey(KeyModifiers modifiers, Key key)
    {
        if (!isInput)
        {
            return false;
        }
        isInput = false;

        _output = new()
        {
            Key = key,
            KeyModifiers = modifiers
        };

        semaphore.Release();

        return true;
    }

    private void InputClose()
    {
        readCancel?.Cancel();
        readCancel?.Dispose();

        readCancel = null;
        if (ptr != IntPtr.Zero)
        {
            unsafe
            {
                InputControl.Close(ptr);
            }
            ptr = IntPtr.Zero;
        }
    }
}
