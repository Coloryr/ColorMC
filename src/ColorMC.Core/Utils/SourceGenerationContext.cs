using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ColorMC;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Java;
using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Mclo;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Objs.OptiFine;
using ColorMC.Core.Objs.OtherLaunch;
using ColorMC.Core.Objs.ServerPack;

namespace ColorMC.Core.Utils;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ConfigObj))]
[JsonSerializable(typeof(GameArgObj))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(GameArgObj.GameArgumentsObj.GameJvmObj))]
[JsonSerializable(typeof(LangObj))]
[JsonSerializable(typeof(MMCObj))]
[JsonSerializable(typeof(OfficialObj))]
[JsonSerializable(typeof(GameSettingObj))]
[JsonSerializable(typeof(HMCLObj))]
[JsonSerializable(typeof(CurseForgePackObj))]
[JsonSerializable(typeof(AuthlibInjectorMetaObj))]
[JsonSerializable(typeof(AuthlibInjectorObj))]
[JsonSerializable(typeof(ForgeLaunchObj))]
[JsonSerializable(typeof(Dictionary<string, OptifineObj>))]
[JsonSerializable(typeof(VersionObj))]
[JsonSerializable(typeof(ForgeInstallObj))]
[JsonSerializable(typeof(FabricLoaderObj))]
[JsonSerializable(typeof(QuiltLoaderObj))]
[JsonSerializable(typeof(ForgeInstallOldObj))]
[JsonSerializable(typeof(ModrinthPackObj))]
[JsonSerializable(typeof(AssetsObj))]
[JsonSerializable(typeof(Dictionary<string, Objs.ModInfoObj>))]
[JsonSerializable(typeof(LaunchDataObj))]
[JsonSerializable(typeof(CustomGameArgObj))]
[JsonSerializable(typeof(AdoptiumJavaVersionObj))]
[JsonSerializable(typeof(List<AdoptiumObj>))]
[JsonSerializable(typeof(McModSearchObj))]
[JsonSerializable(typeof(McModSearchResObj))]
[JsonSerializable(typeof(McModTypsResObj))]
[JsonSerializable(typeof(CurseForgeListObj))]
[JsonSerializable(typeof(CurseForgeModObj))]
[JsonSerializable(typeof(CurseForgeGetFilesObj))]
[JsonSerializable(typeof(CurseForgeGetFilesResObj))]
[JsonSerializable(typeof(CurseForgeCategoriesObj))]
[JsonSerializable(typeof(CurseForgeVersionObj))]
[JsonSerializable(typeof(CurseForgeVersionTypeObj))]
[JsonSerializable(typeof(CurseForgeObj))]
[JsonSerializable(typeof(CurseForgeModsInfoObj))]
[JsonSerializable(typeof(CurseForgeFileObj))]
[JsonSerializable(typeof(DragonwellObj))]
[JsonSerializable(typeof(FabricMetaObj))]
[JsonSerializable(typeof(List<FabricLoaderVersionObj>))]
[JsonSerializable(typeof(List<FabricMetaObj.GameObj>))]
[JsonSerializable(typeof(List<NeoForgeVersionBmclApiObj>))]
[JsonSerializable(typeof(List<ForgeVersionBmclApiObj>))]
[JsonSerializable(typeof(McloResObj))]
[JsonSerializable(typeof(ProfileNameObj))]
[JsonSerializable(typeof(MinecraftProfileObj))]
[JsonSerializable(typeof(UserProfileObj))]
[JsonSerializable(typeof(MinecraftNewObj))]
[JsonSerializable(typeof(ModrinthSearchObj))]
[JsonSerializable(typeof(ModrinthProjectObj))]
[JsonSerializable(typeof(List<ModrinthVersionObj>))]
[JsonSerializable(typeof(List<ModrinthGameVersionObj>))]
[JsonSerializable(typeof(List<ModrinthCategoriesObj>))]
[JsonSerializable(typeof(OAuthObj))]
[JsonSerializable(typeof(OAuthGetCodeObj))]
[JsonSerializable(typeof(OAuthLoginObj))]
[JsonSerializable(typeof(OAuthLogin1Obj))]
[JsonSerializable(typeof(OpenJ9Obj))]
[JsonSerializable(typeof(OpenJ9FileObj))]
[JsonSerializable(typeof(List<OptifineListObj>))]
[JsonSerializable(typeof(MinecraftTexturesObj))]
[JsonSerializable(typeof(List<QuiltMetaObj.QuiltGameObj>))]
[JsonSerializable(typeof(QuiltMetaObj))]
[JsonSerializable(typeof(List<ZuluObj>))]
[JsonSerializable(typeof(AuthenticateObj))]
[JsonSerializable(typeof(AuthenticateResObj))]
[JsonSerializable(typeof(RefreshObj))]
[JsonSerializable(typeof(ChatObj))]
[JsonSerializable(typeof(ServerMotdObj.ServerVersionInfoObj))]
[JsonSerializable(typeof(ServerMotdObj.ServerPlayerInfoObj))]
[JsonSerializable(typeof(ServerMotdObj.ServerMotdModInfoObj))]
[JsonSerializable(typeof(List<LoginObj>))]
[JsonSerializable(typeof(Dictionary<string, MavenItemObj>))]
[JsonSerializable(typeof(ServerPackObj))]
public partial class SourceGenerationContext : JsonSerializerContext
{

}

public static class JsonType
{
    public static JsonTypeInfo<ConfigObj> ConfigObj => SourceGenerationContext.Default.ConfigObj;
    public static JsonTypeInfo<GameArgObj> GameArgObj => SourceGenerationContext.Default.GameArgObj;
    public static JsonTypeInfo<LangObj> LangObj => SourceGenerationContext.Default.LangObj;
    public static JsonTypeInfo<MMCObj> MMCObj => SourceGenerationContext.Default.MMCObj;
    public static JsonTypeInfo<OfficialObj> OfficialObj => SourceGenerationContext.Default.OfficialObj;
    public static JsonTypeInfo<GameSettingObj> GameSettingObj => SourceGenerationContext.Default.GameSettingObj;
    public static JsonTypeInfo<HMCLObj> HMCLObj => SourceGenerationContext.Default.HMCLObj;
    public static JsonTypeInfo<CurseForgePackObj> CurseForgePackObj => SourceGenerationContext.Default.CurseForgePackObj;
    public static JsonTypeInfo<AuthlibInjectorMetaObj> AuthlibInjectorMetaObj => SourceGenerationContext.Default.AuthlibInjectorMetaObj;
    public static JsonTypeInfo<AuthlibInjectorObj> AuthlibInjectorObj => SourceGenerationContext.Default.AuthlibInjectorObj;
    public static JsonTypeInfo<ForgeLaunchObj> ForgeLaunchObj => SourceGenerationContext.Default.ForgeLaunchObj;
    public static JsonTypeInfo<Dictionary<string, OptifineObj>> DictionaryStringOptifineObj => SourceGenerationContext.Default.DictionaryStringOptifineObj;
    public static JsonTypeInfo<VersionObj> VersionObj => SourceGenerationContext.Default.VersionObj;
    public static JsonTypeInfo<ForgeInstallObj> ForgeInstallObj => SourceGenerationContext.Default.ForgeInstallObj;
    public static JsonTypeInfo<FabricLoaderObj> FabricLoaderObj => SourceGenerationContext.Default.FabricLoaderObj;
    public static JsonTypeInfo<QuiltLoaderObj> QuiltLoaderObj => SourceGenerationContext.Default.QuiltLoaderObj;
    public static JsonTypeInfo<ForgeInstallOldObj> ForgeInstallOldObj => SourceGenerationContext.Default.ForgeInstallOldObj;
    public static JsonTypeInfo<ModrinthPackObj> ModrinthPackObj => SourceGenerationContext.Default.ModrinthPackObj;
    public static JsonTypeInfo<AssetsObj> AssetsObj => SourceGenerationContext.Default.AssetsObj;
    public static JsonTypeInfo<Dictionary<string, Objs.ModInfoObj>> DictionaryStringModInfoObj => SourceGenerationContext.Default.DictionaryStringModInfoObj;
    public static JsonTypeInfo<LaunchDataObj> LaunchDataObj => SourceGenerationContext.Default.LaunchDataObj;
    public static JsonTypeInfo<CustomGameArgObj> CustomGameArgObj => SourceGenerationContext.Default.CustomGameArgObj;
    public static JsonTypeInfo<AdoptiumJavaVersionObj> AdoptiumJavaVersionObj => SourceGenerationContext.Default.AdoptiumJavaVersionObj;
    public static JsonTypeInfo<List<AdoptiumObj>> ListAdoptiumObj => SourceGenerationContext.Default.ListAdoptiumObj;
    public static JsonTypeInfo<McModSearchObj> McModSearchObj => SourceGenerationContext.Default.McModSearchObj;
    public static JsonTypeInfo<McModSearchResObj> McModSearchResObj => SourceGenerationContext.Default.McModSearchResObj;
    public static JsonTypeInfo<McModTypsResObj> McModTypsResObj => SourceGenerationContext.Default.McModTypsResObj;
    public static JsonTypeInfo<CurseForgeListObj> CurseForgeListObj => SourceGenerationContext.Default.CurseForgeListObj;
    public static JsonTypeInfo<CurseForgeModObj> CurseForgeModObj => SourceGenerationContext.Default.CurseForgeModObj;
    public static JsonTypeInfo<CurseForgeGetFilesObj> CurseForgeGetFilesObj => SourceGenerationContext.Default.CurseForgeGetFilesObj;
    public static JsonTypeInfo<CurseForgeGetFilesResObj> CurseForgeGetFilesResObj => SourceGenerationContext.Default.CurseForgeGetFilesResObj;
    public static JsonTypeInfo<CurseForgeCategoriesObj> CurseForgeCategoriesObj => SourceGenerationContext.Default.CurseForgeCategoriesObj;
    public static JsonTypeInfo<CurseForgeVersionObj> CurseForgeVersionObj => SourceGenerationContext.Default.CurseForgeVersionObj;
    public static JsonTypeInfo<CurseForgeVersionTypeObj> CurseForgeVersionTypeObj => SourceGenerationContext.Default.CurseForgeVersionTypeObj;
    public static JsonTypeInfo<CurseForgeObj> CurseForgeObj => SourceGenerationContext.Default.CurseForgeObj;
    public static JsonTypeInfo<CurseForgeModsInfoObj> CurseForgeModsInfoObj => SourceGenerationContext.Default.CurseForgeModsInfoObj;
    public static JsonTypeInfo<CurseForgeFileObj> CurseForgeFileObj => SourceGenerationContext.Default.CurseForgeFileObj;
    public static JsonTypeInfo<DragonwellObj> DragonwellObj => SourceGenerationContext.Default.DragonwellObj;
    public static JsonTypeInfo<FabricMetaObj> FabricMetaObj => SourceGenerationContext.Default.FabricMetaObj;
    public static JsonTypeInfo<List<FabricLoaderVersionObj>> ListFabricLoaderVersionObj => SourceGenerationContext.Default.ListFabricLoaderVersionObj;
    public static JsonTypeInfo<List<FabricMetaObj.GameObj>> ListGameObj => SourceGenerationContext.Default.ListGameObj;
    public static JsonTypeInfo<List<string>> ListString => SourceGenerationContext.Default.ListString;
    public static JsonTypeInfo<List<NeoForgeVersionBmclApiObj>> ListNeoForgeVersionBmclApiObj => SourceGenerationContext.Default.ListNeoForgeVersionBmclApiObj;
    public static JsonTypeInfo<List<ForgeVersionBmclApiObj>> ListForgeVersionBmclApiObj => SourceGenerationContext.Default.ListForgeVersionBmclApiObj;
    public static JsonTypeInfo<McloResObj> McloResObj => SourceGenerationContext.Default.McloResObj;
    public static JsonTypeInfo<ProfileNameObj> ProfileNameObj => SourceGenerationContext.Default.ProfileNameObj;
    public static JsonTypeInfo<MinecraftProfileObj> MinecraftProfileObj => SourceGenerationContext.Default.MinecraftProfileObj;
    public static JsonTypeInfo<UserProfileObj> UserProfileObj => SourceGenerationContext.Default.UserProfileObj;
    public static JsonTypeInfo<MinecraftNewObj> MinecraftNewObj => SourceGenerationContext.Default.MinecraftNewObj;
    public static JsonTypeInfo<ModrinthSearchObj> ModrinthSearchObj => SourceGenerationContext.Default.ModrinthSearchObj;
    public static JsonTypeInfo<ModrinthVersionObj> ModrinthVersionObj => SourceGenerationContext.Default.ModrinthVersionObj;
    public static JsonTypeInfo<ModrinthProjectObj> ModrinthProjectObj => SourceGenerationContext.Default.ModrinthProjectObj;
    public static JsonTypeInfo<List<ModrinthVersionObj>> ListModrinthVersionObj => SourceGenerationContext.Default.ListModrinthVersionObj;
    public static JsonTypeInfo<List<ModrinthGameVersionObj>> ListModrinthGameVersionObj => SourceGenerationContext.Default.ListModrinthGameVersionObj;
    public static JsonTypeInfo<List<ModrinthCategoriesObj>> ListModrinthCategoriesObj => SourceGenerationContext.Default.ListModrinthCategoriesObj;
    public static JsonTypeInfo<OAuthObj> OAuthObj => SourceGenerationContext.Default.OAuthObj;
    public static JsonTypeInfo<OAuthGetCodeObj> OAuthGetCodeObj => SourceGenerationContext.Default.OAuthGetCodeObj;
    public static JsonTypeInfo<OAuthLoginObj> OAuthLoginObj => SourceGenerationContext.Default.OAuthLoginObj;
    public static JsonTypeInfo<OAuthLogin1Obj> OAuthLogin1Obj => SourceGenerationContext.Default.OAuthLogin1Obj;
    public static JsonTypeInfo<OpenJ9Obj> OpenJ9Obj => SourceGenerationContext.Default.OpenJ9Obj;
    public static JsonTypeInfo<OpenJ9FileObj> OpenJ9FileObj => SourceGenerationContext.Default.OpenJ9FileObj;
    public static JsonTypeInfo<List<OptifineListObj>> ListOptifineListObj => SourceGenerationContext.Default.ListOptifineListObj;
    public static JsonTypeInfo<MinecraftTexturesObj> MinecraftTexturesObj => SourceGenerationContext.Default.MinecraftTexturesObj;
    public static JsonTypeInfo<List<QuiltMetaObj.QuiltGameObj>> ListQuiltGameObj => SourceGenerationContext.Default.ListQuiltGameObj;
    public static JsonTypeInfo<QuiltMetaObj> QuiltMetaObj => SourceGenerationContext.Default.QuiltMetaObj;
    public static JsonTypeInfo<List<ZuluObj>> ListZuluObj => SourceGenerationContext.Default.ListZuluObj;
    public static JsonTypeInfo<AuthenticateObj> AuthenticateObj => SourceGenerationContext.Default.AuthenticateObj;
    public static JsonTypeInfo<AuthenticateResObj> AuthenticateResObj => SourceGenerationContext.Default.AuthenticateResObj;
    public static JsonTypeInfo<RefreshObj> RefreshObj => SourceGenerationContext.Default.RefreshObj;
    public static JsonTypeInfo<ChatObj> ChatObj => SourceGenerationContext.Default.ChatObj;
    public static JsonTypeInfo<ServerMotdObj.ServerVersionInfoObj> ServerVersionInfoObj => SourceGenerationContext.Default.ServerVersionInfoObj;
    public static JsonTypeInfo<ServerMotdObj.ServerPlayerInfoObj> ServerPlayerInfoObj => SourceGenerationContext.Default.ServerPlayerInfoObj;
    public static JsonTypeInfo<ServerMotdObj.ServerMotdModInfoObj> ServerMotdModInfoObj => SourceGenerationContext.Default.ServerMotdModInfoObj;
    public static JsonTypeInfo<List<LoginObj>> ListLoginObj => SourceGenerationContext.Default.ListLoginObj;
    public static JsonTypeInfo<Dictionary<string, MavenItemObj>> DictionaryStringMavenItemObj => SourceGenerationContext.Default.DictionaryStringMavenItemObj;
    public static JsonTypeInfo<ServerPackObj> ServerPackObj => SourceGenerationContext.Default.ServerPackObj;
    public static JsonTypeInfo<GameArgObj.GameArgumentsObj.GameJvmObj> GameJvmObj => SourceGenerationContext.Default.GameJvmObj;
}
