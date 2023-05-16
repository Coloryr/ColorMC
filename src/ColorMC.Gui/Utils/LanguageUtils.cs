using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.Utils;

public static class LanguageUtils
{
    public static string GetName(this SkinType type)
    {
        return type switch
        {
            SkinType.Old => App.GetLanguage("SkinType.Old"),
            SkinType.New => App.GetLanguage("SkinType.New"),
            SkinType.NewSlim => App.GetLanguage("SkinType.New_Slim"),
            _ => App.GetLanguage("SkinType.Other")
        };
    }

    public static string GetName(this FTBType type)
    {
        return type switch
        {
            FTBType.All => App.GetLanguage("FTBType.All"),
            FTBType.Featured => App.GetLanguage("FTBType.Featured"),
            FTBType.Popular => App.GetLanguage("FTBType.Popular"),
            FTBType.Installs => App.GetLanguage("FTBType.Installs"),
            FTBType.Search => App.GetLanguage("FTBType.Search"),
            _ => App.GetLanguage("FTBType.Other")
        };
    }
}
