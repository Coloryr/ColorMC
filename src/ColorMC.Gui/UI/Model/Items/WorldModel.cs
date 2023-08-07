using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class WorldModel : BaseModel
{
    public WorldDisplayObj World { get; }

    private readonly ILoadFuntion<WorldModel> _top;

    [ObservableProperty]
    private bool _isSelect;

    public string Name => World.Name;
    public string Mode => World.Mode;
    public string Time => World.Time;
    public string Local => World.Local;
    public string Difficulty => World.Difficulty;
    public string Hardcore => World.Hardcore.ToString();
    public Bitmap Pic => World.Pic ?? App.GameIcon;

    public WorldModel(IUserControl con, ILoadFuntion<WorldModel> top,
        WorldDisplayObj world) : base(con)
    {
        _top = top;
        World = world;
    }

    public void Select()
    {
        _top.SetSelect(this);
    }

    public void Flyout(Control con)
    {
        _ = new GameEditFlyout2(con, this);
    }

    public async void Delete(WorldDisplayObj obj)
    {
        var res = await ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab5.Info1"), obj.Name));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteWorld(obj.World);
        Notify(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        await _top.Load();
    }

    public async void Export(WorldDisplayObj obj)
    {
        Progress(App.GetLanguage("GameEditWindow.Tab5.Info4"));
        var file = await PathBinding.SaveFile(Window, FileType.World, new object[]
            { obj });
        ProgressClose();
        if (file == null)
            return;

        if (file == false)
        {
            Show(App.GetLanguage("GameEditWindow.Tab5.Error1"));
        }
        else
        {
            Notify(App.GetLanguage("GameEditWindow.Tab5.Info3"));
        }
    }

    public async void Backup(WorldModel obj)
    {
        Progress(App.GetLanguage("GameEditWindow.Tab5.Info7"));
        var res = await GameBinding.BackupWorld(obj.World.World);
        ProgressClose();
        if (res)
        {
            Notify(App.GetLanguage("GameEditWindow.Tab5.Info8"));
        }
        else
        {
            Show(App.GetLanguage("GameEditWindow.Tab5.Error3"));
        }
    }

    public async void Launch(WorldDisplayObj world)
    {
        if (BaseBinding.IsGameRun(world.World.Game))
        {
            return;
        }

        var res = await GameBinding.Launch(Window, world.World.Game, world.World);
        if (!res.Item1)
        {
            Show(res.Item2!);
        }
    }
}
