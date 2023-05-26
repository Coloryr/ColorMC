using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.IO;

namespace ColorMC.Gui.UI.Model.Main;

public partial class GameModel : ObservableObject
{
    public readonly IUserControl Con;
    private readonly IMainTop Top;

    public GameSettingObj Obj { get; }

    [ObservableProperty]
    private bool isSelect;
    [ObservableProperty]
    private bool isLaunch;
    [ObservableProperty]
    private bool isLoad;
    [ObservableProperty]
    private bool isDrop;

    [ObservableProperty]
    private string tips;

    [ObservableProperty]
    private TextWrapping wrap;

    public string Name => Obj.Name;
    public Bitmap Pic => GetImage();

    public GameModel(IUserControl con, IMainTop top, GameSettingObj obj)
    {
        Top = top;
        Con = con;
        Obj = obj;
    }

    partial void OnIsSelectChanged(bool value)
    {
        Wrap = value ? TextWrapping.Wrap : TextWrapping.NoWrap;
    }

    public void Reload()
    {
        IsLaunch = BaseBinding.IsGameRun(Obj);

        SetTips();
    }

    public void SetTips()
    {
        Tips = string.Format(App.GetLanguage("Tips.Text1"),
            Obj.LaunchData.AddTime.Ticks == 0 ? "" : Obj.LaunchData.AddTime.ToString(),
            Obj.LaunchData.LastTime.Ticks == 0 ? "" : Obj.LaunchData.LastTime.ToString(),
            Obj.LaunchData.GameTime.Ticks == 0 ? "" : Obj.LaunchData.GameTime.ToString(@"d\.hh\:mm\:ss"));
    }

    public async void Move(PointerEventArgs e)
    {
        var dragData = new DataObject();
        dragData.Set(BaseBinding.DrapType, Obj);

        if (Con.Window is TopLevel top)
        {
            List<IStorageFolder> files = new();
            var item = await top.StorageProvider
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
        Top.Select(this);
    }

    public void Flyout(Control con)
    {
        _ = new MainFlyout(con, this);
    }

    public void Launch(bool funtion)
    {
        Top.Launch(this, funtion);
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
        var window = Con.Window;
        await window.InputInfo.ShowEdit(App.GetLanguage("MainWindow.Info23"), Obj.Name);
        if (window.InputInfo.Cancel)
            return;
        var data = window.InputInfo.Read().Item1;
        if (string.IsNullOrWhiteSpace(data))
        {
            window.OkInfo.Show(App.GetLanguage("MainWindow.Error3"));
            return;
        }

        GameBinding.SetGameName(Obj, data);
    }

    public async void Copy()
    {
        var window = Con.Window;
        await window.InputInfo.ShowEdit(App.GetLanguage("MainWindow.Info23"),
            Obj.Name + App.GetLanguage("MainWindow.Info24"));
        if (window.InputInfo.Cancel)
            return;
        var data = window.InputInfo.Read().Item1;
        if (string.IsNullOrWhiteSpace(data))
        {
            window.OkInfo.Show(App.GetLanguage("MainWindow.Error3"));
            return;
        }

        var res = await GameBinding.CopyGame(Obj, data);
        if (!res)
        {
            window.OkInfo.Show(App.GetLanguage("MainWindow.Error5"));
            return;
        }
        else
        {
            window.NotifyInfo.Show(App.GetLanguage("MainWindow.Info25"));
        }
    }

    public async void DeleteGame()
    {
        var window = Con.Window;
        var res = await window.OkInfo.ShowWait(
                 string.Format(App.GetLanguage("MainWindow.Info19"), Obj.Name));
        if (!res)
            return;

        res = await GameBinding.DeleteGame(Obj);
        if (!res)
        {
            window.OkInfo.Show(App.GetLanguage("MainWindow.Info37"));
        }
    }

    public void EditGroup()
    {
        Top.EditGroup(this);
    }
}
