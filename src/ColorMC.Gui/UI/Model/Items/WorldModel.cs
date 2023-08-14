using Avalonia.Media.Imaging;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.GameEdit;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class WorldModel : ObservableObject
{
    public readonly WorldObj World;
    public readonly GameEditTab5Model Top;

    [ObservableProperty]
    private bool _isSelect;

    public string Name => World.LevelName;
    public string Mode => LanguageHelper.GetNameWithGameType(World.GameType);
    public string Time => FuntionUtils.MillisecondsToDataTime(World.LastPlayed).ToString();
    public string Local => World.Local;
    public string Difficulty => LanguageHelper.GetNameWithDifficulty(World.Difficulty);
    public string Hardcore => World.Hardcore == 1 ? "True" : "False";
    public Bitmap Pic { get; }

    public WorldModel(GameEditTab5Model top, WorldObj world)
    {
        Top = top;
        World = world;
        Pic = World.Icon != null ? new Bitmap(World.Icon) : App.GameIcon;
    }

    public void Close()
    {
        if (Pic != App.GameIcon)
        {
            Pic.Dispose();
        }
    }

    public void Select()
    {
        Top.SetSelect(this);
    }
}
