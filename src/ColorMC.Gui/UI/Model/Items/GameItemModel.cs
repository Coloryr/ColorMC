using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 游戏项目
/// </summary>
public partial class GameItemModel : GameModel
{
    /// <summary>
    /// 是否选中
    /// </summary>
    [ObservableProperty]
    private bool _isSelect;
    /// <summary>
    /// 是否已启动
    /// </summary>
    [ObservableProperty]
    private bool _isLaunch;
    /// <summary>
    /// 是否在启动中
    /// </summary>
    [ObservableProperty]
    private bool _isLaunching;
    /// <summary>
    /// 是否已经加载
    /// </summary>
    [ObservableProperty]
    private bool _isLoad;
    /// <summary>
    /// 是否拖拽
    /// </summary>
    [ObservableProperty]
    private bool _isDrop;
    /// <summary>
    /// 是否鼠标在上面
    /// </summary>
    [ObservableProperty]
    private bool _isOver;
    /// <summary>
    /// 是否为新建实例
    /// </summary>
    [ObservableProperty]
    private bool _isNew;
    /// <summary>
    /// 是否勾选
    /// </summary>
    [ObservableProperty]
    private bool _isCheck;
    /// <summary>
    /// 是否显示按钮
    /// </summary>
    [ObservableProperty]
    private bool _buttonShow;
    /// <summary>
    /// 是否显示勾选
    /// </summary>
    [ObservableProperty]
    private bool _showCheck;
    /// <summary>
    /// 是否显示
    /// </summary>
    [ObservableProperty]
    private bool _isDisplay = true;
    /// <summary>
    /// 是否星标
    /// </summary>
    [ObservableProperty]
    private bool _isStar;
    /// <summary>
    /// 是否显示星标
    /// </summary>
    [ObservableProperty]
    private bool _starVis;
    /// <summary>
    /// 是否为锁定模式
    /// </summary>
    [ObservableProperty]
    private bool _oneGame;

    /// <summary>
    /// 悬浮提示
    /// </summary>
    [ObservableProperty]
    private string _tips;
    /// <summary>
    /// 星标
    /// </summary>
    [ObservableProperty]
    private string _star = ImageManager.Stars[1];

    /// <summary>
    /// 字体换行
    /// </summary>
    [ObservableProperty]
    private TextWrapping _wrap = TextWrapping.NoWrap;
    /// <summary>
    /// 字体裁剪
    /// </summary>
    [ObservableProperty]
    private TextTrimming _trim = TextTrimming.CharacterEllipsis;

    /// <summary>
    /// 主窗口
    /// </summary>
    private readonly IMainTop? _top;

    /// <summary>
    /// 名字
    /// </summary>
    public string Name => Obj.Name;
    /// <summary>
    /// UUID
    /// </summary>
    public string UUID => Obj.UUID;

    /// <summary>
    /// 图标
    /// </summary>
    [ObservableProperty]
    private Bitmap _pic;

    /// <summary>
    /// 游戏分组
    /// </summary>
    private readonly string? _group;

    /// <summary>
    /// 标号
    /// </summary>
    public int Index
    {
        set
        {
            switch (value)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    {
                        using var asset = AssetLoader.Open(new Uri($"resm:ColorMC.Gui.Resource.Pic.{value}.png"));
                        Pic = new Bitmap(asset);
                    }
                    break;
            }
        }
    }

    public GameItemModel(WindowModel model, string? group) : base(model, new() { })
    {
        _group = group;
        _isNew = true;
    }

    public GameItemModel(WindowModel model, IMainTop? top, GameSettingObj obj) : base(model, obj)
    {
        _top = top;
        _group = obj.GroupName;
        Pic = ImageManager.GetGameIcon(Obj) ?? ImageManager.GameIcon;
        IsStar = GameManager.IsStar(obj);

        EventManager.GameIconChange += EventManager_GameIconChange;
        EventManager.GameNameChange += EventManager_GameNameChange;
    }

    private void EventManager_GameNameChange(object? sender, string uuid)
    {
        if (uuid != Obj.UUID)
        {
            return;
        }

        OnPropertyChanged(nameof(Name));
    }

    /// <summary>
    /// 星标修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsStarChanged(bool value)
    {
        Star = ImageManager.Stars[value ? 0 : 1];
        StarVis = (value || IsOver) && !IsNew;
    }
    /// <summary>
    /// 单游戏修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnOneGameChanged(bool value)
    {
        if (value)
        {
            IsSelect = false;
        }
    }
    /// <summary>
    /// 启动状态修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsLaunchChanged(bool value)
    {
        if (OneGame)
        {
            return;
        }
        if (value)
        {
            IsLaunching = true;
        }
        else
        {
            if (!IsLoad)
            {
                IsLaunching = false;
            }
        }
    }
    /// <summary>
    /// 加载状态修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsLoadChanged(bool value)
    {
        if (value)
        {
            IsLaunching = true;
        }
        else
        {
            if (!IsLoad)
            {
                IsLaunching = false;
            }
        }
    }
    /// <summary>
    /// 鼠标经过
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsOverChanged(bool value)
    {
        if (!ShowCheck)
        {
            StarVis = (value || IsStar) && !IsNew;
            ButtonShow = value || IsSelect;
        }
    }
    /// <summary>
    /// 是否选中
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsSelectChanged(bool value)
    {
        if (value == false && OneGame)
        {
            return;
        }
        Wrap = value ? TextWrapping.Wrap : TextWrapping.NoWrap;
        Trim = value ? TextTrimming.None : TextTrimming.CharacterEllipsis;
        IsDrop = false;

        ButtonShow = value || IsOver;
    }

    /// <summary>
    /// 标星
    /// </summary>
    [RelayCommand]
    public void DoStar()
    {
        if (ShowCheck)
        {
            return;
        }
        _top?.DoStar(this);
        IsOver = false;
    }
    /// <summary>
    /// 添加游戏实例
    /// </summary>
    [RelayCommand]
    public void AddGame()
    {
        if (ShowCheck)
        {
            return;
        }
        WindowManager.ShowAddGame(_group);
    }
    public void Launch()
    {
        if (ShowCheck)
        {
            return;
        }
        if (IsLaunch)
        {
            return;
        }

        _top?.Launch(this);
    }

    /// <summary>
    /// 开始多选
    /// </summary>
    public void StartCheck()
    {
        IsSelect = false;
        IsCheck = false;
        ShowCheck = true;
    }

    /// <summary>
    /// 结束多选
    /// </summary>
    public void StopCheck()
    {
        ShowCheck = false;
    }

    /// <summary>
    /// 选中
    /// </summary>
    public void Select()
    {
        if (OneGame)
        {
            return;
        }

        IsSelect = true;
    }

    /// <summary>
    /// 取消选中
    /// </summary>
    public void Unselect()
    {
        if (OneGame)
        {
            return;
        }

        IsSelect = false;
    }

    /// <summary>
    /// 更新悬浮
    /// </summary>
    public void SetTips()
    {
        if (IsNew)
        {
            return;
        }
        var time1 = Obj.LaunchData.AddTime;
        var time2 = Obj.LaunchData.LastTime;
        var time3 = Obj.LaunchData.LastPlay;
        var time4 = Obj.LaunchData.GameTime;
        Tips = string.Format(LanguageUtils.Get("ToolTip.Text125"),
            time1.Ticks == 0 ? "" : time1.ToString(),
            time2.Ticks == 0 ? "" : time2.ToString(),
            time3.Ticks == 0 ? "" :
            $"{time3.TotalHours:#}:{time3.Minutes:00}:{time3.Seconds:00}",
            time4.Ticks == 0 ? "" :
            $"{time4.TotalHours:#}:{time4.Minutes:00}:{time4.Seconds:00}");
    }

    /// <summary>
    /// 移动游戏实例
    /// </summary>
    /// <param name="top"></param>
    /// <param name="e"></param>
    public async void Move(TopLevel? top, PointerEventArgs e)
    {
        if (ShowCheck)
        {
            return;
        }
        var dragData = new DataTransfer();
        dragData.Add(DataTransferItem.Create(BaseBinding.DrapType, Obj.UUID));
        IsDrop = true;
        var files = new List<IStorageFolder>();
        if (top == null)
        {
            return;
        }
        var item = await top.StorageProvider.TryGetFolderFromPathAsync(Obj.GetBasePath());
        files.Add(item!);
        foreach (var file in files)
        {
            dragData.Add(DataTransferItem.CreateFile(file));
        }

        Dispatcher.UIThread.Post(() =>
        {
            DragDrop.DoDragDropAsync(e, dragData, DragDropEffects.Move | DragDropEffects.Link | DragDropEffects.Copy);
        });
    }

    /// <summary>
    /// 选中
    /// </summary>
    public void SetSelect()
    {
        _top?.Select(this);
    }

    /// <summary>
    /// 右键弹窗
    /// </summary>
    /// <param name="con"></param>
    public void Flyout(Control con)
    {
        MainFlyout.Show(con, this);
    }

    /// <summary>
    /// 重命名
    /// </summary>
    public async void Rename()
    {
        var dialog = new InputModel(Window.WindowId)
        {
            Watermark1 = LanguageUtils.Get("MainWindow.Text69"),
            Text1 = Obj.Name
        };
        var res = await Window.ShowDialogWait(dialog);
        if (res is not true)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(dialog.Text1))
        {
            Window.Show(LanguageUtils.Get("MainWindow.Text82"));
            return;
        }

        GameBinding.SetGameName(Obj, dialog.Text1);
        OnPropertyChanged(nameof(Name));
    }

    /// <summary>
    /// 设置手柄
    /// </summary>
    public async void SetJoystick()
    {
        if (GameJoystick.NowGameJoystick.TryGetValue(Obj.UUID, out var value))
        {
            var dialog = value.MakeModel(Window.WindowId);
            var res = await Window.ShowDialogWait(dialog);
            if (res is true)
            {
                value.ChangeConfig(dialog);
                Window.Notify(LanguageUtils.Get("MainWindow.Text76"));
            }
        }
    }

    /// <summary>
    /// 复制副本
    /// </summary>
    public async void Copy()
    {
        var dialog = new InputModel(Window.WindowId)
        {
            Watermark1 = LanguageUtils.Get("MainWindow.Text69"),
            Text1 = Obj.Name + LanguageUtils.Get("App.Text34")
        };
        var res = await Window.ShowDialogWait(dialog);
        if (res is not true)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(dialog.Text1))
        {
            Window.Show(LanguageUtils.Get("MainWindow.Text82"));
            return;
        }

        var res1 = await GameBinding.CopyGameAsync(Obj, dialog.Text1, new OverGameGui(Window));
        if (!res1)
        {
            Window.Show(LanguageUtils.Get("MainWindow.Text84"));
            return;
        }
        else
        {
            Window.Notify(LanguageUtils.Get("MainWindow.Text70"));
        }
    }

    /// <summary>
    /// 删除游戏实例
    /// </summary>
    public async void DeleteGame()
    {
        if (GameManager.IsAdd(Obj))
        {
            Window.Show(LanguageUtils.Get("GameEditWindow.Tab1.Text46"));
            return;
        }

        var res = await Window.ShowChoice(string.Format(LanguageUtils.Get("MainWindow.Text67"), Obj.Name));
        if (!res)
        {
            return;
        }

        var dialog = Window.ShowProgress(LanguageUtils.Get("GameEditWindow.Tab1.Text35"));
        res = await GameBinding.DeleteGameAsync(Obj);
        Window.CloseDialog(dialog);
        if (!res)
        {
            Window.Show(LanguageUtils.Get("MainWindow.Text75"));
        }
    }

    /// <summary>
    /// 编辑游戏分组
    /// </summary>
    public void EditGroup()
    {
        _top?.EditGroup(this);
    }

    /// <summary>
    /// 导出启动参数
    /// </summary>
    public void ExportCmd()
    {
        _top?.ExportCmd(Obj);
    }

    private void EventManager_GameIconChange(object? sender, string uuid)
    {
        if (uuid != UUID)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            Pic = ImageManager.GetGameIcon(Obj) ?? ImageManager.GameIcon;
        });
    }

    public override void Close()
    {
        EventManager.GameIconChange -= EventManager_GameIconChange;
        EventManager.GameNameChange -= EventManager_GameNameChange;
    }
}
