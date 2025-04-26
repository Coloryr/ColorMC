using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

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

    public GameItemModel(BaseModel model, string? group) : base(model, new() { })
    {
        _group = group;
        _isNew = true;
    }

    public GameItemModel(BaseModel model, IMainTop? top, GameSettingObj obj) : base(model, obj)
    {
        _top = top;
        _group = obj.GroupName;
        LoadIcon();
        IsStar = GameManager.IsStar(obj);
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
#if Phone
        IsOver = value;
#endif

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
    /// <summary>
    /// 启动
    /// </summary>
    [RelayCommand]
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
    /// 编辑
    /// </summary>
    [RelayCommand]
    public void EditGame()
    {
        WindowManager.ShowGameEdit(Obj);
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
    /// 加载图标
    /// </summary>
    public void LoadIcon()
    {
        Pic = GetImage();
    }

    /// <summary>
    /// 重载图标
    /// </summary>
    public void ReloadIcon()
    {
        Pic = ReloadImage();
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
        Tips = string.Format(App.Lang("ToolTip.Text125"),
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
#if !Phone
        if (ShowCheck)
        {
            return;
        }
        var dragData = new DataObject();
        dragData.Set(BaseBinding.DrapType, Obj.UUID);
        IsDrop = true;
        var files = new List<IStorageFolder>();
        if (top == null)
        {
            return;
        }
        var item = await top.StorageProvider.TryGetFolderFromPathAsync(Obj.GetBasePath());
        files.Add(item!);
        dragData.Set(DataFormats.Files, files);
        Dispatcher.UIThread.Post(() =>
        {
            DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move | DragDropEffects.Link | DragDropEffects.Copy);
        });
#endif
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
        _ = new MainFlyout(con, this);
    }

    /// <summary>
    /// 获取图标
    /// </summary>
    /// <returns></returns>
    private Bitmap GetImage()
    {
        return ImageManager.GetGameIcon(Obj) ?? ImageManager.GameIcon;
    }

    /// <summary>
    /// 重载图标
    /// </summary>
    /// <returns></returns>
    private Bitmap ReloadImage()
    {
        return ImageManager.ReloadImage(Obj) ?? ImageManager.GameIcon;
    }

    /// <summary>
    /// 重命名
    /// </summary>
    public async void Rename()
    {
        var res = await Model.Input(App.Lang("MainWindow.Info23"), Obj.Name);
        if (res.Cancel)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(res.Text1))
        {
            Model.Show(App.Lang("MainWindow.Error3"));
            return;
        }

        GameBinding.SetGameName(Obj, res.Text1);
        OnPropertyChanged(nameof(Name));
    }

    /// <summary>
    /// 设置手柄
    /// </summary>
    public async void SetJoystick()
    {
        if (GameJoystick.NowGameJoystick.TryGetValue(Obj.UUID, out var value))
        {
            var model = value.MakeModel();
            var res = await DialogHost.Show(model, "MainCon");
            if (res is true)
            {
                value.ChangeConfig(model);
                Model.Notify(App.Lang("MainWindow.Info39"));
            }
        }
    }

    /// <summary>
    /// 复制副本
    /// </summary>
    public async void Copy()
    {
        var res = await Model.Input(App.Lang("MainWindow.Info23"),
            Obj.Name + App.Lang("MainWindow.Info24"));
        if (res.Cancel)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(res.Text1))
        {
            Model.Show(App.Lang("MainWindow.Error3"));
            return;
        }

        var res1 = await GameBinding.CopyGame(Obj, res.Text1, Model.ShowAsync, GameOverwirte);
        if (!res1)
        {
            Model.Show(App.Lang("MainWindow.Error5"));
            return;
        }
        else
        {
            Model.Notify(App.Lang("MainWindow.Info25"));
        }
    }

    /// <summary>
    /// 删除游戏实例
    /// </summary>
    public async void DeleteGame()
    {
        var res = await Model.ShowAsync(string.Format(App.Lang("MainWindow.Info19"), Obj.Name));
        if (!res)
        {
            return;
        }

        Model.Progress(App.Lang("GameEditWindow.Tab1.Info11"));
        res = await GameBinding.DeleteGame(Obj);
        Model.ProgressClose();
        Model.InputClose();
        if (!res)
        {
            Model.Show(App.Lang("MainWindow.Info37"));
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
    /// 请求覆盖
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        return await Model.ShowAsync(
            string.Format(App.Lang("AddGameWindow.Info2"), obj.Name));
    }

    /// <summary>
    /// 导出启动参数
    /// </summary>
    public void ExportCmd()
    {
        _top?.ExportCmd(Obj);
    }

    public override void Close()
    {

    }
}
