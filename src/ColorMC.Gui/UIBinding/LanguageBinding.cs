using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;

namespace ColorMC.Gui.UIBinding;

public static class LanguageBinding
{
    public static string[] GetAxisTypeName()
    {
        return
        [
            App.Lang("AxisType.Item1"),
            App.Lang("AxisType.Item2")
        ];
    }

    /// <summary>
    /// 获取过滤器选项
    /// </summary>
    /// <returns>选项</returns>
    public static string[] GetFilterName()
    {
        return
        [
            App.Lang("Text.Name"),
            App.Lang("Text.FileName"),
            App.Lang("Text.Author"),
            App.Lang("ModFilter.Item4")
        ];
    }

    public static string[] GetExportName()
    {
        return
        [
            App.Lang("ExportPack.Item1"),
            App.Lang("ExportPack.Item2"),
            App.Lang("ExportPack.Item3"),
            //App.GetLanguage("ExportPack.Item4"),
            //App.GetLanguage("ExportPack.Item5")
        ];
    }

    public static string[] GetSkinType()
    {
        return
        [
            App.Lang("SkinType.Old"),
            App.Lang("SkinType.New"),
            App.Lang("SkinType.New_Slim")
        ];
    }
    /// <summary>
    /// 获取旋转选项
    /// </summary>
    /// <returns>选项</returns>
    public static string[] GetSkinRotateName()
    {
        return
        [
            App.Lang("SkinRotate.Item1"),
            App.Lang("SkinRotate.Item2"),
            App.Lang("SkinRotate.Item3")
        ];
    }

    /// <summary>
    /// 获取下载源选项
    /// </summary>
    /// <returns>选项</returns>
    public static string[] GetDownloadSources()
    {
        return
        [
            SourceLocal.Offical.GetName(),
            SourceLocal.BMCLAPI.GetName()
        ];
    }

    /// <summary>
    /// 获取窗口透明选项
    /// </summary>
    /// <returns>选项</returns>
    public static string[] GetWindowTranTypes()
    {
        return
        [
            App.Lang("TranTypes.Item1"),
            App.Lang("TranTypes.Item2"),
            App.Lang("TranTypes.Item3"),
            App.Lang("TranTypes.Item4"),
            App.Lang("TranTypes.Item5")
        ];
    }
    /// <summary>
    /// 获取语言选项
    /// </summary>
    /// <returns>选项</returns>
    public static string[] GetLanguages()
    {
        return
        [
            LanguageType.zh_cn.GetName(),
            LanguageType.en_us.GetName()
        ];
    }

    public static string[] GetCurseForgeSortTypes()
    {
        return
        [
            CurseForgeSortField.Featured.GetName(),
            CurseForgeSortField.Popularity.GetName(),
            CurseForgeSortField.LastUpdated.GetName(),
            CurseForgeSortField.Name.GetName(),
            CurseForgeSortField.TotalDownloads.GetName()
        ];
    }

    public static string[] GetModrinthSortTypes()
    {
        return
        [
            MSortingObj.Relevance.GetName(),
            MSortingObj.Downloads.GetName(),
            MSortingObj.Follows.GetName(),
            MSortingObj.Newest.GetName(),
            MSortingObj.Updated.GetName()
        ];
    }

    public static string[] GetSortOrder()
    {
        return
        [
            App.Lang("SortOrder.Item1"),
            App.Lang("SortOrder.Item2")
        ];
    }

    public static string[] GetSourceList()
    {
        return
        [
            SourceType.CurseForge.GetName(),
            SourceType.Modrinth.GetName()
        ];
    }

    public static string[] GetPackType()
    {
        return
        [
            PackType.ColorMC.GetName(),
            PackType.CurseForge.GetName(),
            PackType.Modrinth.GetName(),
            PackType.MMC.GetName(),
            PackType.HMCL.GetName(),
            PackType.ZipPack.GetName()
        ];
    }

    public static string[] GetAddType()
    {
        return
        [
            FileType.Mod.GetName(),
            FileType.World.GetName(),
            FileType.Shaderpack.GetName(),
            FileType.Resourcepack.GetName(),
            FileType.DataPacks.GetName(),
            FileType.Optifne.GetName()
        ];
    }

    public static string[] GetNbtName()
    {
        return
        [
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
        ];
    }

    public static string[] GetFuntionList()
    {
        return
        [
            App.Lang("ServerPackWindow.Tab4.Item1"),
            App.Lang("ServerPackWindow.Tab4.Item2")
        ];
    }

    public static string[] GetVersionType()
    {
        return
        [
            App.Lang("VersionType.Item1"),
            App.Lang("VersionType.Item2"),
            App.Lang("VersionType.Item3")
        ];
    }

    public static string[] GetPos()
    {
        return
        [
            App.Lang("Postion.Item1"),
            App.Lang("Postion.Item2"),
            App.Lang("Postion.Item3"),
            App.Lang("Postion.Item4"),
            App.Lang("Postion.Item5"),
            App.Lang("Postion.Item6"),
            App.Lang("Postion.Item7"),
            App.Lang("Postion.Item8"),
            App.Lang("Postion.Item9"),
        ];
    }
}
