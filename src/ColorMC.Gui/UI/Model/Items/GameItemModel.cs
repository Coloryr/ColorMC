using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Items;

public partial class GameItemModel : GameModel
{
    [ObservableProperty]
    private bool _isSelect;
    [ObservableProperty]
    private bool _isLaunch;
    [ObservableProperty]
    private bool _isLaunching;
    [ObservableProperty]
    private bool _isLoad;
    [ObservableProperty]
    private bool _isDrop;
    [ObservableProperty]
    private bool _isOver;
    [ObservableProperty]
    private bool _isNew;

    [ObservableProperty]
    private bool _buttonShow;

    [ObservableProperty]
    private string _tips;

    [ObservableProperty]
    private TextWrapping _wrap = TextWrapping.NoWrap;
    [ObservableProperty]
    private TextTrimming _trim = TextTrimming.CharacterEllipsis;

    private readonly IMainTop _top;

    public string Name => Obj.Name;

    [ObservableProperty]
    private bool _oneGame;

    [ObservableProperty]
    private Bitmap _pic;

    private readonly string? _group;

    public GameItemModel(BaseModel model, string? group) : base(model, new() { })
    {
        _group = group;
        _isNew = true;
    }

    public GameItemModel(BaseModel model, IMainTop top, GameSettingObj obj) : base(model, obj)
    {
        _top = top;
        LoadIcon();
    }

    partial void OnOneGameChanged(bool value)
    {
        if (value)
        {
            IsSelect = true;
        }
    }

    partial void OnIsLaunchChanged(bool value)
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

    partial void OnIsOverChanged(bool value)
    {
        ButtonShow = value || IsSelect;
    }

    partial void OnIsSelectChanged(bool value)
    {
        if (value == false && OneGame)
        {
            return;
        }
        Wrap = value ? TextWrapping.Wrap : TextWrapping.NoWrap;
        Trim = value ? TextTrimming.None : TextTrimming.CharacterEllipsis;
        IsDrop = false;
        if (SystemInfo.Os == OsType.Android)
        {
            IsOver = value;
        }

        ButtonShow = value || IsOver;
    }

    [RelayCommand]
    public void AddGame()
    {
        App.ShowAddGame(_group);
    }

    [RelayCommand]
    public void Launch()
    {
        if (IsLaunch)
        {
            return;
        }

        _top.Launch(this);
    }

    [RelayCommand]
    public void EditGame()
    {
        App.ShowGameEdit(Obj);
    }

    public void LoadIcon()
    {
        if (Pic != null && Pic != App.GameIcon)
        {
            Pic.Dispose();
        }

        Pic = GetImage();
    }

    public void Reload()
    {
        IsLaunch = BaseBinding.IsGameRun(Obj);

        SetTips();
    }

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

    public async void Move(PointerEventArgs e)
    {
        var dragData = new DataObject();
        dragData.Set(BaseBinding.DrapType, this);
        IsDrop = true;

        if (SystemInfo.Os != OsType.Android)
        {
            var files = new List<IStorageFolder>();
            var item = await App.TopLevel!.StorageProvider
                   .TryGetFolderFromPathAsync(Obj.GetBasePath());
            files.Add(item!);
            dragData.Set(DataFormats.Files, files);
        }
        Dispatcher.UIThread.Post(() =>
        {
            DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move | DragDropEffects.Link | DragDropEffects.Copy);
        });
    }

    public void SetSelect()
    {
        _top.Select(this);
    }

    public void Flyout(Control con)
    {
        _ = new MainFlyout(con, this);
    }

    private Bitmap GetImage()
    {
        var file = Obj.GetIconFile();
        if (File.Exists(file))
        {
            return new Bitmap(file);
        }
        else
        {
            return App.GameIcon;
        }
    }

    public async void Rename()
    {
        var (Cancel, Text1) = await Model.ShowEdit(App.Lang("MainWindow.Info23"), Obj.Name);
        if (Cancel)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(Text1))
        {
            Model.Show(App.Lang("MainWindow.Error3"));
            return;
        }

        GameBinding.SetGameName(Obj, Text1);
    }

    public void SetJoystick()
    {
        if (GameJoystick.NowGameJoystick.TryGetValue(Obj.UUID, out var value))
        {
            var model = value.MakeConfig();
            model.TopCancel = () => { DialogHost.Close("MainCon"); };
            model.TopConfirm = () => { DialogHost.Close("MainCon"); value.ChangeConfig(model); };
            DialogHost.Show(model, "MainCon");
        }
    }

    public async void Copy()
    {
        var (Cancel, Text1) = await Model.ShowEdit(App.Lang("MainWindow.Info23"),
            Obj.Name + App.Lang("MainWindow.Info24"));
        if (Cancel)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(Text1))
        {
            Model.Show(App.Lang("MainWindow.Error3"));
            return;
        }

        var res = await GameBinding.CopyGame(Obj, Text1, Model.ShowWait, GameOverwirte);
        if (!res)
        {
            Model.Show(App.Lang("MainWindow.Error5"));
            return;
        }
        else
        {
            Model.Notify(App.Lang("MainWindow.Info25"));
        }
    }

    public async void DeleteGame()
    {
        var res = await Model.ShowWait(string.Format(App.Lang("MainWindow.Info19"), Obj.Name));
        if (!res)
        {
            return;
        }

        Model.Progress(App.Lang("Gui.Info34"));
        res = await GameBinding.DeleteGame(Obj, Model.ShowWait);
        Model.ProgressClose();
        Model.InputClose();
        if (!res)
        {
            Model.Show(App.Lang("MainWindow.Info37"));
        }
    }

    public void EditGroup()
    {
        _top.EditGroup(this);
    }

    /// <summary>
    /// 请求
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        return await Model.ShowWait(
            string.Format(App.Lang("AddGameWindow.Info2"), obj.Name));
    }

    protected override void Close()
    {
        if (Pic != App.GameIcon)
        {
            Pic?.Dispose();
        }
    }
}
