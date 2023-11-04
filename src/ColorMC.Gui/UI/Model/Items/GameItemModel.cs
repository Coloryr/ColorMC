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
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.IO;

namespace ColorMC.Gui.UI.Model.Items;

public partial class GameItemModel : GameModel
{
    [ObservableProperty]
    private bool _isSelect;
    [ObservableProperty]
    private bool _isLaunch;
    [ObservableProperty]
    private bool _isLoad;
    [ObservableProperty]
    private bool _isDrop;

    [ObservableProperty]
    private string _tips;

    [ObservableProperty]
    private TextWrapping _wrap = TextWrapping.NoWrap;
    [ObservableProperty]
    private TextTrimming _trim = TextTrimming.CharacterEllipsis;

    private readonly IMainTop _top;

    public string Name => Obj.Name;

    [ObservableProperty]
    private Bitmap _pic;

    public GameItemModel(BaseModel model, IMainTop top, GameSettingObj obj) : base(model, obj)
    {
        _top = top;
        LoadIcon();
    }

    partial void OnIsSelectChanged(bool value)
    {
        Wrap = value ? TextWrapping.Wrap : TextWrapping.NoWrap;
        Trim = value ? TextTrimming.None : TextTrimming.CharacterEllipsis;
        IsDrop = false;
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
        Tips = string.Format(App.Lang("Tips.Text1"),
            Obj.LaunchData.AddTime.Ticks == 0 ? "" : Obj.LaunchData.AddTime.ToString(),
            Obj.LaunchData.LastTime.Ticks == 0 ? "" : Obj.LaunchData.LastTime.ToString(),
            Obj.LaunchData.LastPlay.Ticks == 0 ? "" :
            $"{Obj.LaunchData.LastPlay.TotalHours:#}:{Obj.LaunchData.LastPlay.Minutes:00}:{Obj.LaunchData.LastPlay.Seconds:00}",
            Obj.LaunchData.GameTime.Ticks == 0 ? "" :
            $"{Obj.LaunchData.GameTime.TotalHours:#}:{Obj.LaunchData.GameTime.Minutes:00}:{Obj.LaunchData.GameTime.Seconds:00}");
    }

    public async void Move(PointerEventArgs e)
    {
        var dragData = new DataObject();
        dragData.Set(BaseBinding.DrapType, this);
        IsDrop = true;

        if (SystemInfo.Os != OsType.Android)
        {
            List<IStorageFolder> files = new();
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

    public void Launch()
    {
        if (IsLaunch)
        {
            return;
        }

        _top.Launch(this);
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

        var res = await GameBinding.CopyGame(Obj, Text1);
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
        res = await GameBinding.DeleteGame(Model, Obj);
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

    protected override void Close()
    {
        if (Pic != App.GameIcon)
        {
            Pic.Dispose();
        }
    }
}
