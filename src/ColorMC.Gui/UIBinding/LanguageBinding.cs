using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UIBinding;

public static class LanguageBinding
{
    /// <summary>
    /// 获取过滤器选项
    /// </summary>
    /// <returns>选项</returns>
    public static string[] GetFilterName()
    {
        return new[]
        {
            App.Lang("Text.Name"),
            App.Lang("Text.FileName"),
            App.Lang("BaseBinding.Filter.Item3"),
            App.Lang("BaseBinding.Filter.Item4")
        };
    }

    public static string[] GetExportName()
    {
        return new[]
         {
            App.Lang("BaseBinding.Export.Item1"),
            App.Lang("BaseBinding.Export.Item2"),
            App.Lang("BaseBinding.Export.Item3"),
            //App.GetLanguage("BaseBinding.Export.Item4"),
            //App.GetLanguage("BaseBinding.Export.Item5")
        };
    }

    public static string[] GetSkinType()
    {
        return new[]
        {
            App.Lang("SkinType.Old"),
            App.Lang("SkinType.New"),
            App.Lang("SkinType.New_Slim")
        };
    }
    /// <summary>
    /// 获取旋转选项
    /// </summary>
    /// <returns>选项</returns>
    public static string[] GetSkinRotateName()
    {
        return new[]
        {
            App.Lang("BaseBinding.SkinRotate.Item1"),
            App.Lang("BaseBinding.SkinRotate.Item2"),
            App.Lang("BaseBinding.SkinRotate.Item3")
        };
    }

    /// <summary>
    /// 获取下载源选项
    /// </summary>
    /// <returns>选项</returns>
    public static string[] GetDownloadSources()
    {
        return new[]
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
    public static string[] GetWindowTranTypes()
    {
        return new[]
        {
            App.Lang("TranTypes.Item1"),
            App.Lang("TranTypes.Item2"),
            App.Lang("TranTypes.Item3"),
            App.Lang("TranTypes.Item4"),
            App.Lang("TranTypes.Item5")
        };
    }
    /// <summary>
    /// 获取语言选项
    /// </summary>
    /// <returns>选项</returns>
    public static string[] GetLanguages()
    {
        return new[]
        {
            LanguageType.zh_cn.GetName(),
            LanguageType.en_us.GetName()
        };
    }

    public static string[] GetCurseForgeSortTypes()
    {
        return new[]
        {
            CurseForgeSortField.Featured.GetName(),
            CurseForgeSortField.Popularity.GetName(),
            CurseForgeSortField.LastUpdated.GetName(),
            CurseForgeSortField.Name.GetName(),
            CurseForgeSortField.TotalDownloads.GetName()
        };
    }

    public static string[] GetModrinthSortTypes()
    {
        return new[]
        {
            MSortingObj.Relevance.GetName(),
            MSortingObj.Downloads.GetName(),
            MSortingObj.Follows.GetName(),
            MSortingObj.Newest.GetName(),
            MSortingObj.Updated.GetName()
        };
    }

    public static string[] GetSortOrder()
    {
        return new[]
        {
            App.Lang("GameBinding.SortOrder.Item1"),
            App.Lang("GameBinding.SortOrder.Item2")
        };
    }

    public static string[] GetSourceList()
    {
        return new[]
        {
            SourceType.CurseForge.GetName(),
            SourceType.Modrinth.GetName()
        };
    }

    public static string[] GetPackType()
    {
        return new[]
        {
            PackType.ColorMC.GetName(),
            PackType.CurseForge.GetName(),
            PackType.Modrinth.GetName(),
            PackType.MMC.GetName(),
            PackType.HMCL.GetName(),
            PackType.ZipPack.GetName()
        };
    }

    public static string[] GetAddType()
    {
        return new[]
        {
            FileType.Mod.GetName(),
            FileType.World.GetName(),
            FileType.Shaderpack.GetName(),
            FileType.Resourcepack.GetName(),
            FileType.DataPacks.GetName(),
            FileType.Optifne.GetName()
        };
    }

    public static string[] GetNbtName()
    {
        return new[]
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

    public static string[] GetFuntionList()
    {
        return new[]
        {
            App.Lang("ServerPackWindow.Tab4.Item1"),
            App.Lang("ServerPackWindow.Tab4.Item2")
        };
    }

    public static string[] GetVersionType()
    {
        return new[]
        {
            App.Lang("VersionType.Item1"),
            App.Lang("VersionType.Item2"),
            App.Lang("VersionType.Item3")
        };
    }

    public static string[] GetPos()
    {
        return new[]
        {
            App.Lang("Postion.Item1"),
            App.Lang("Postion.Item2"),
            App.Lang("Postion.Item3"),
            App.Lang("Postion.Item4"),
            App.Lang("Postion.Item5"),
            App.Lang("Postion.Item6"),
            App.Lang("Postion.Item7"),
            App.Lang("Postion.Item8"),
            App.Lang("Postion.Item9"),
        };
    }
}
