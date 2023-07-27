using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using System.Collections.Generic;

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

    /// <summary>
    /// 获取过滤器选项
    /// </summary>
    /// <returns>选项</returns>
    public static List<string> GetFilterName() => new()
    {
        App.GetLanguage("BaseBinding.Filter.Item1"),
        App.GetLanguage("BaseBinding.Filter.Item2"),
        App.GetLanguage("BaseBinding.Filter.Item3")
    };

    public static List<string> GetSkinType()
    {
        var list = new List<string>()
        {
            SkinType.Old.GetName(),
            SkinType.New.GetName(),
            SkinType.NewSlim.GetName(),
            SkinType.Unkonw.GetName(),
        };

        return list;
    }
    /// <summary>
    /// 获取旋转选项
    /// </summary>
    /// <returns>选项</returns>
    public static List<string> GetSkinRotateName() => new()
    {
        App.GetLanguage("BaseBinding.SkinRotate.Item1"),
        App.GetLanguage("BaseBinding.SkinRotate.Item2"),
        App.GetLanguage("BaseBinding.SkinRotate.Item3")
    };

    /// <summary>
    /// 获取下载源选项
    /// </summary>
    /// <returns>选项</returns>
    public static List<string> GetDownloadSources()
    {
        var list = new List<string>
        {
            SourceLocal.Offical.GetName(),
            SourceLocal.BMCLAPI.GetName(),
            SourceLocal.MCBBS.GetName()
        };

        return list;
    }

    /// <summary>
    /// 获取窗口透明选项
    /// </summary>
    /// <returns>选项</returns>
    public static List<string> GetWindowTranTypes()
    {
        return new()
        {
            App.GetLanguage("TranTypes.Item1"),
            App.GetLanguage("TranTypes.Item2"),
            App.GetLanguage("TranTypes.Item3"),
            App.GetLanguage("TranTypes.Item4"),
            App.GetLanguage("TranTypes.Item5")
        };
    }
    /// <summary>
    /// 获取语言选项
    /// </summary>
    /// <returns>选项</returns>
    public static List<string> GetLanguages()
    {
        var list = new List<string>
        {
            LanguageType.zh_cn.GetName(),
            LanguageType.en_us.GetName()
        };

        return list;
    }
}
