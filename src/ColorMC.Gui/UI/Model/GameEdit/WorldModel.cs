using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class WorldModel : ObservableObject
{
    public WorldDisplayObj World { get; }

    private readonly IWorldFuntion Top;
    private readonly IUserControl Con;

    [ObservableProperty]
    private bool isSelect;

    public string Name => World.Name;
    public string Mode => World.Mode;
    public string Time => World.Time;
    public string Local => World.Local;
    public string Difficulty => World.Difficulty;
    public string Hardcore => World.Hardcore.ToString();
    public Bitmap Pic => World.Pic ?? App.GameIcon;

    public WorldModel(IUserControl con, IWorldFuntion top, WorldDisplayObj world)
    {
        Con = con;
        Top = top;
        World = world;
    }

    public void Select()
    {
        Top.SetSelect(this);
    }

    public void Flyout(Control con)
    {
        _ = new GameEditFlyout2(con, this);
    }

    public async void Delete(WorldDisplayObj obj)
    {
        var window = Con.Window;
        var res = await window!.OkInfo.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab5.Info1"), obj.Name));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteWorld(obj.World);
        window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        Top.Load();
    }

    public async void Export(WorldDisplayObj obj)
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab5.Info4"));
        var file = await BaseBinding.SaveFile(window as TopLevel, FileType.World, new object[]
            { obj });
        window.ProgressInfo.Close();
        if (file == null)
            return;

        if (file == false)
        {
            window.OkInfo.Show(App.GetLanguage("GameEditWindow.Tab5.Error1"));
        }
        else
        {
            window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab5.Info3"));
        }
    }

    public async void Backup(WorldModel obj)
    {
        var Window = Con.Window;
        Window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab5.Info7"));
        var res = await GameBinding.BackupWorld(obj.World.World);
        Window.ProgressInfo.Close();
        if (res)
        {
            Window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab5.Info8"));
        }
        else
        {
            Window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab5.Error3"));
        }
    }
}
