using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Gui.Utils;

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
            LanguageUtils.Get("Type.DisplayType.Item1"),
            LanguageUtils.Get("Type.DisplayType.Item2"),
            LanguageUtils.Get("Type.DisplayType.Item3"),
            LanguageUtils.Get("Type.DisplayType.Item4"),
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
            GCType.Auto.GetName(),
            GCType.G1GC.GetName(),
            GCType.ZGC.GetName(),
            GCType.None.GetName()
        ];
    }

    public static string[] GetAxisTypeName()
    {
        return
        [
            LanguageUtils.Get("Type.AxisType.Item1"),
            LanguageUtils.Get("Type.AxisType.Item2")
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
            LanguageUtils.Get("Text.Name"),
            LanguageUtils.Get("Text.FileName"),
            LanguageUtils.Get("Text.Author"),
            "modid",
            LanguageUtils.Get("GameEditWindow.Tab4.Text16"),
            LanguageUtils.Get("GameEditWindow.Tab4.Text17"),
            LanguageUtils.Get("GameEditWindow.Tab4.Text18"),
        ];
    }

    public static string[] GetExportName()
    {
        return
        [
            LanguageUtils.Get("Type.ExportPack.Item1"),
            LanguageUtils.Get("Type.ExportPack.Item2"),
            LanguageUtils.Get("Type.ExportPack.Item3"),
            //App.GetLanguage("Type.ExportPack.Item4"),
            //App.GetLanguage("Type.ExportPack.Item5")
        ];
    }

    public static string[] GetSkinType()
    {
        return
        [
            LanguageUtils.Get("Type.SkinType.Old"),
            LanguageUtils.Get("Type.SkinType.New"),
            LanguageUtils.Get("Type.SkinType.New_Slim")
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
            LanguageUtils.Get("Type.SkinRotate.Item1"),
            LanguageUtils.Get("Type.SkinRotate.Item2"),
            LanguageUtils.Get("Type.SkinRotate.Item3")
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
            LanguageUtils.Get("Type.Dns.Item1"),
            LanguageUtils.Get("Type.Dns.Item2"),
            LanguageUtils.Get("Type.Dns.Item3")
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
            LanguageUtils.Get("Type.TranTypes.Item1"),
            LanguageUtils.Get("Type.TranTypes.Item2"),
            LanguageUtils.Get("Type.TranTypes.Item3"),
            LanguageUtils.Get("Type.TranTypes.Item4"),
            LanguageUtils.Get("Type.TranTypes.Item5")
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
            LanguageUtils.Get("Type.SortOrder.Item1"),
            LanguageUtils.Get("Type.SortOrder.Item2")
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
            LanguageUtils.Get("ServerPackWindow.Tab4.Text5"),
            LanguageUtils.Get("ServerPackWindow.Tab4.Text6")
        ];
    }

    public static string[] GetVersionType()
    {
        return
        [
            LanguageUtils.Get("Type.VersionType.Item1"),
            LanguageUtils.Get("Type.VersionType.Item2"),
            LanguageUtils.Get("Type.VersionType.Item3")
        ];
    }

    public static string[] GetPos()
    {
        return
        [
            LanguageUtils.Get("Type.Postion.Item1"),
            LanguageUtils.Get("Type.Postion.Item2"),
            LanguageUtils.Get("Type.Postion.Item3"),
            LanguageUtils.Get("Type.Postion.Item4"),
            LanguageUtils.Get("Type.Postion.Item5"),
            LanguageUtils.Get("Type.Postion.Item6"),
            LanguageUtils.Get("Type.Postion.Item7"),
            LanguageUtils.Get("Type.Postion.Item8"),
            LanguageUtils.Get("Type.Postion.Item9"),
        ];
    }

    public static string[] GetGuide()
    {
        return
        [
            LanguageUtils.Get("Type.Guide.Item1"),
            LanguageUtils.Get("Type.Guide.Item2"),
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
