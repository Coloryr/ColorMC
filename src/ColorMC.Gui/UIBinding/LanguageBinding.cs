using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;

namespace ColorMC.Gui.UIBinding;

/// <summary>
/// 获取文本
/// </summary>
public static class LanguageBinding
{
    public static string[] GetDisplayList()
    {
        return
        [
            App.Lang("Type.DisplayType.Item1"),
            App.Lang("Type.DisplayType.Item2"),
            App.Lang("Type.DisplayType.Item3"),
            App.Lang("Type.DisplayType.Item4"),
        ];
    }

    public static string[] GetLockLoginType()
    {
        return
        [
            AuthType.OAuth.GetName(),
            AuthType.Nide8.GetName(),
            AuthType.AuthlibInjector.GetName(),
            AuthType.LittleSkin.GetName(),
            AuthType.SelfLittleSkin.GetName()
        ];
    }

    public static string[] GetLoginUserType()
    {
        return
        [
            AuthType.Offline.GetName(),
            AuthType.OAuth.GetName(),
            AuthType.Nide8.GetName(),
            AuthType.AuthlibInjector.GetName(),
            AuthType.LittleSkin.GetName(),
            AuthType.SelfLittleSkin.GetName()
        ];
    }

    public static string[] GetDisplayUserTypes()
    {
        return
        [
            "",
            AuthType.Offline.GetName(),
            AuthType.OAuth.GetName(),
            AuthType.Nide8.GetName(),
            AuthType.AuthlibInjector.GetName(),
            AuthType.LittleSkin.GetName(),
            AuthType.SelfLittleSkin.GetName()
        ];
    }

    public static string[] GetGCTypes()
    {
        return
        [
            "",
            GCType.G1GC.GetName(),
            GCType.SerialGC.GetName(),
            GCType.ParallelGC.GetName(),
            GCType.CMSGC.GetName(),
            GCType.User.GetName()
        ];
    }

    public static string[] GetAxisTypeName()
    {
        return
        [
            App.Lang("Type.AxisType.Item1"),
            App.Lang("Type.AxisType.Item2")
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
            "modid",
            App.Lang("GameEditWindow.Tab4.Text16"),
            App.Lang("GameEditWindow.Tab4.Text17"),
            App.Lang("GameEditWindow.Tab4.Text18"),
        ];
    }

    public static string[] GetExportName()
    {
        return
        [
            App.Lang("Type.ExportPack.Item1"),
            App.Lang("Type.ExportPack.Item2"),
            App.Lang("Type.ExportPack.Item3"),
            //App.GetLanguage("Type.ExportPack.Item4"),
            //App.GetLanguage("Type.ExportPack.Item5")
        ];
    }

    public static string[] GetSkinType()
    {
        return
        [
            App.Lang("Type.SkinType.Old"),
            App.Lang("Type.SkinType.New"),
            App.Lang("Type.SkinType.New_Slim")
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
            App.Lang("Type.SkinRotate.Item1"),
            App.Lang("Type.SkinRotate.Item2"),
            App.Lang("Type.SkinRotate.Item3")
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

    public static string[] GetDns()
    {
        return
        [
            App.Lang("Type.Dns.Item1"),
            App.Lang("Type.Dns.Item2"),
            App.Lang("Type.Dns.Item3")
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
            App.Lang("Type.TranTypes.Item1"),
            App.Lang("Type.TranTypes.Item2"),
            App.Lang("Type.TranTypes.Item3"),
            App.Lang("Type.TranTypes.Item4"),
            App.Lang("Type.TranTypes.Item5")
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
            ModrinthHelper.Relevance.GetName(),
            ModrinthHelper.Downloads.GetName(),
            ModrinthHelper.Follows.GetName(),
            ModrinthHelper.Newest.GetName(),
            ModrinthHelper.Updated.GetName()
        ];
    }

    public static string[] GetSortOrder()
    {
        return
        [
            App.Lang("Type.SortOrder.Item1"),
            App.Lang("Type.SortOrder.Item2")
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
            FileType.Optifine.GetName()
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

    public static string[] GetPCJavaType()
    {
        return
        [
            "Adoptium",
            "Zulu",
            "Dragonwell",
            "OpenJ9", 
            //"Graalvm"
        ];
    }

    public static string[] GetFuntionList()
    {
        return
        [
            App.Lang("ServerPackWindow.Tab4.Text5"),
            App.Lang("ServerPackWindow.Tab4.Text6")
        ];
    }

    public static string[] GetVersionType()
    {
        return
        [
            App.Lang("Type.VersionType.Item1"),
            App.Lang("Type.VersionType.Item2"),
            App.Lang("Type.VersionType.Item3")
        ];
    }

    public static string[] GetPos()
    {
        return
        [
            App.Lang("Type.Postion.Item1"),
            App.Lang("Type.Postion.Item2"),
            App.Lang("Type.Postion.Item3"),
            App.Lang("Type.Postion.Item4"),
            App.Lang("Type.Postion.Item5"),
            App.Lang("Type.Postion.Item6"),
            App.Lang("Type.Postion.Item7"),
            App.Lang("Type.Postion.Item8"),
            App.Lang("Type.Postion.Item9"),
        ];
    }

    public static string[] GetGuide()
    {
        return
        [
            App.Lang("Type.Guide.Item1"),
            App.Lang("Type.Guide.Item2"),
        ];
    }

    public static string[] GetLoader()
    {
        return
        [
            Loaders.Forge.GetName(),
            Loaders.Fabric.GetName(),
            Loaders.Quilt.GetName(),
            Loaders.NeoForge.GetName(),
        ];
    }
}
