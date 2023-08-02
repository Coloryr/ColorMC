using Avalonia.Media.Imaging;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;

namespace ColorMC.Gui.Objs;

public record WorldDisplayObj
{
    public string Name => World.LevelName;
    public string Mode => LanguageHelper.GetNameWithGameType(World.GameType);
    public string Time => Funtions.MillisecondsToDataTime(World.LastPlayed).ToString();
    public string Local => World.Local;
    public string Difficulty => LanguageHelper.GetNameWithDifficulty(World.Difficulty);
    public bool Hardcore => World.Hardcore == 1;
    public Bitmap? Pic;

    public required WorldObj World;
}
