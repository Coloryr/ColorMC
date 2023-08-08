using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using System.Collections.Generic;

namespace ColorMC.Gui.UIBinding;

public static class LanguageBinding
{
    /// <summary>
    /// 获取过滤器选项
    /// </summary>
    /// <returns>选项</returns>
    public static List<string> GetFilterName()
    { 
        return new()
        {
            App.GetLanguage("Text.Name"),
            App.GetLanguage("Text.FileName"),
            App.GetLanguage("BaseBinding.Filter.Item3")
        };
    }

    public static List<string> GetExportName()
    { 
        return new()
        {
            App.GetLanguage("BaseBinding.Export.Item1"),
            App.GetLanguage("BaseBinding.Export.Item2"),
            App.GetLanguage("BaseBinding.Export.Item3"),
            //App.GetLanguage("BaseBinding.Export.Item4"),
            //App.GetLanguage("BaseBinding.Export.Item5")
        };
    }

    public static List<string> GetSkinType()
    {
        var list = new List<string>()
        {
            App.GetLanguage("SkinType.Old"),
            App.GetLanguage("SkinType.New"),
            App.GetLanguage("SkinType.New_Slim")
        };

        return list;
    }
    /// <summary>
    /// 获取旋转选项
    /// </summary>
    /// <returns>选项</returns>
    public static List<string> GetSkinRotateName()
    { 
        return new()
        {
            App.GetLanguage("BaseBinding.SkinRotate.Item1"),
            App.GetLanguage("BaseBinding.SkinRotate.Item2"),
            App.GetLanguage("BaseBinding.SkinRotate.Item3")
        };
    }

    /// <summary>
    /// 获取下载源选项
    /// </summary>
    /// <returns>选项</returns>
    public static List<string> GetDownloadSources()
    {
        return new()
        {
            SourceLocal.Offical.GetName(),
            SourceLocal.BMCLAPI.GetName(),
            SourceLocal.MCBBS.GetName()
        };
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
        return new()
        {
            LanguageType.zh_cn.GetName(),
            LanguageType.en_us.GetName()
        };
    }

    public static List<string> GetCurseForgeSortTypes()
    {
        return new()
        {
            CurseForgeSortField.Featured.GetName(),
            CurseForgeSortField.Popularity.GetName(),
            CurseForgeSortField.LastUpdated.GetName(),
            CurseForgeSortField.Name.GetName(),
            CurseForgeSortField.TotalDownloads.GetName()
        };
    }

    public static List<string> GetModrinthSortTypes()
    {
        return new()
        {
            MSortingObj.Relevance.GetName(),
            MSortingObj.Downloads.GetName(),
            MSortingObj.Follows.GetName(),
            MSortingObj.Newest.GetName(),
            MSortingObj.Updated.GetName()
        };
    }

    public static List<string> GetSortOrder()
    {
        return new()
        {
            App.GetLanguage("GameBinding.SortOrder.Item1"),
            App.GetLanguage("GameBinding.SortOrder.Item2")
        };
    }

    public static List<string> GetSourceList()
    {
        return new()
        {
            SourceType.CurseForge.GetName(),
            SourceType.Modrinth.GetName()
        };
    }

    public static List<string> GetPackType()
    {
        return new()
        {
            PackType.ColorMC.GetName(),
            PackType.CurseForge.GetName(),
            PackType.Modrinth.GetName(),
            PackType.MMC.GetName(),
            PackType.HMCL.GetName(),
        };
    }

    public static List<string> GetAddType()
    {
        return new()
        {
            FileType.Mod.GetName(),
            FileType.World.GetName(),
            FileType.Shaderpack.GetName(),
            FileType.Resourcepack.GetName(),
            FileType.DataPacks.GetName(),
            FileType.Optifne.GetName()
        };
    }

    public static List<string> GetNbtName()
    {
        return new()
        {
            "NbtEnd",
            "NbtByte",
            "NbtShort",
            "NbtInt",
            "NbtLong",
            "NbtFloat",
            "NbtDouble",
            "NbtByteArray",
            "NbtString",
            "NbtList",
            "NbtCompound",
            "NbtIntArray",
            "NbtLongArray",
        };
    }

    public static List<string> GetFontName()
    {
        return new()
        {
            App.GetLanguage("ServerPackWindow.Tab4.Item1"),
            App.GetLanguage("ServerPackWindow.Tab4.Item2")
        };
    }
}
