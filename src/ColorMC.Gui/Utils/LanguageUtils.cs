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
}
