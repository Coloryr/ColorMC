using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Frp;

namespace ColorMC.Gui.Utils;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(InputControlObj))]
[JsonSerializable(typeof(UiBackConfigObj))]
[JsonSerializable(typeof(UiColorConfigObj))]
[JsonSerializable(typeof(UiOtherConfigObj))]
[JsonSerializable(typeof(LaunchCheckConfigObj))]
[JsonSerializable(typeof(RunArgObj))]
[JsonSerializable(typeof(WindowSettingObj))]
[JsonSerializable(typeof(ServerOptConfigObj))]
[JsonSerializable(typeof(ServerLockConfigObj))]
[JsonSerializable(typeof(ServerUiConfigObj))]
[JsonSerializable(typeof(ServerMusicConfigObj))]
[JsonSerializable(typeof(List<JvmConfigObj>))]
[JsonSerializable(typeof(Dictionary<string, WindowStateObj>))]
[JsonSerializable(typeof(Dictionary<string, FrpDownloadObj>))]
[JsonSerializable(typeof(Dictionary<string, HdiffDownloadObj>))]
[JsonSerializable(typeof(PutCloudServerObj))]
[JsonSerializable(typeof(List<CloundListObj>))]
[JsonSerializable(typeof(List<CloudWorldObj>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(OpenFrpChannelObj))]
[JsonSerializable(typeof(OpenFrpChannelInfoObj))]
[JsonSerializable(typeof(OpenFrpDownloadObj))]
[JsonSerializable(typeof(List<SakuraFrpChannelObj>))]
[JsonSerializable(typeof(SakuraFrpGetChannelObj))]
[JsonSerializable(typeof(SakuraFrpDownloadObj))]
[JsonSerializable(typeof(GuiConfigObj))]
[JsonSerializable(typeof(List<UpdateObj>))]
[JsonSerializable(typeof(CollectObj))]
[JsonSerializable(typeof(FrpConfigObj))]
[JsonSerializable(typeof(Dictionary<string, CloudDataObj>))]
[JsonSerializable(typeof(GameGuiSettingObj))]
[JsonSerializable(typeof(List<FrpCloudObj>))]
public partial class SourceGenerationContext : JsonSerializerContext
{

}

public static class JsonGuiType
{
    public static JsonTypeInfo<InputControlObj> InputControlObj => SourceGenerationContext.Default.InputControlObj;
    public static JsonTypeInfo<UiBackConfigObj> UiBackConfigObj => SourceGenerationContext.Default.UiBackConfigObj;
    public static JsonTypeInfo<UiColorConfigObj> UiColorConfigObj => SourceGenerationContext.Default.UiColorConfigObj;
    public static JsonTypeInfo<UiOtherConfigObj> UiOtherConfigObj => SourceGenerationContext.Default.UiOtherConfigObj;
    public static JsonTypeInfo<LaunchCheckConfigObj> LaunchCheckConfigObj => SourceGenerationContext.Default.LaunchCheckConfigObj;
    public static JsonTypeInfo<RunArgObj> RunArgObj => SourceGenerationContext.Default.RunArgObj;
    public static JsonTypeInfo<WindowSettingObj> WindowSettingObj => SourceGenerationContext.Default.WindowSettingObj;
    public static JsonTypeInfo<ServerOptConfigObj> ServerOptConfigObj => SourceGenerationContext.Default.ServerOptConfigObj;
    public static JsonTypeInfo<ServerLockConfigObj> ServerLockConfigObj => SourceGenerationContext.Default.ServerLockConfigObj;
    public static JsonTypeInfo<ServerUiConfigObj> ServerUiConfigObj => SourceGenerationContext.Default.ServerUiConfigObj;
    public static JsonTypeInfo<ServerMusicConfigObj> ServerMusicConfigObj => SourceGenerationContext.Default.ServerMusicConfigObj;
    public static JsonTypeInfo<List<JvmConfigObj>> ListJvmConfigObj => SourceGenerationContext.Default.ListJvmConfigObj;
    public static JsonTypeInfo<Dictionary<string, WindowStateObj>> DictionaryStringWindowStateObj => SourceGenerationContext.Default.DictionaryStringWindowStateObj;
    public static JsonTypeInfo<Dictionary<string, FrpDownloadObj>> DictionaryStringFrpDownloadObj => SourceGenerationContext.Default.DictionaryStringFrpDownloadObj;
    public static JsonTypeInfo<Dictionary<string, HdiffDownloadObj>> DictionaryStringHdiffDownloadObj => SourceGenerationContext.Default.DictionaryStringHdiffDownloadObj;
    public static JsonTypeInfo<PutCloudServerObj> PutCloudServerObj => SourceGenerationContext.Default.PutCloudServerObj;
    public static JsonTypeInfo<List<CloundListObj>> ListCloundListObj => SourceGenerationContext.Default.ListCloundListObj;
    public static JsonTypeInfo<List<CloudWorldObj>> ListCloudWorldObj => SourceGenerationContext.Default.ListCloudWorldObj;
    public static JsonTypeInfo<Dictionary<string, string>> DictionaryStringString => SourceGenerationContext.Default.DictionaryStringString;
    public static JsonTypeInfo<OpenFrpChannelObj> OpenFrpChannelObj => SourceGenerationContext.Default.OpenFrpChannelObj;
    public static JsonTypeInfo<OpenFrpChannelInfoObj> OpenFrpChannelInfoObj => SourceGenerationContext.Default.OpenFrpChannelInfoObj;
    public static JsonTypeInfo<OpenFrpDownloadObj> OpenFrpDownloadObj => SourceGenerationContext.Default.OpenFrpDownloadObj;
    public static JsonTypeInfo<List<SakuraFrpChannelObj>> ListSakuraFrpChannelObj => SourceGenerationContext.Default.ListSakuraFrpChannelObj;
    public static JsonTypeInfo<SakuraFrpGetChannelObj> SakuraFrpGetChannelObj => SourceGenerationContext.Default.SakuraFrpGetChannelObj;
    public static JsonTypeInfo<SakuraFrpDownloadObj> SakuraFrpDownloadObj => SourceGenerationContext.Default.SakuraFrpDownloadObj;
    public static JsonTypeInfo<GuiConfigObj> GuiConfigObj => SourceGenerationContext.Default.GuiConfigObj;
    public static JsonTypeInfo<List<UpdateObj>> ListUpdateObj => SourceGenerationContext.Default.ListUpdateObj;
    public static JsonTypeInfo<CollectObj> CollectObj => SourceGenerationContext.Default.CollectObj;
    public static JsonTypeInfo<FrpConfigObj> FrpConfigObj => SourceGenerationContext.Default.FrpConfigObj;
    public static JsonTypeInfo<Dictionary<string, CloudDataObj>> DictionaryStringCloudDataObj => SourceGenerationContext.Default.DictionaryStringCloudDataObj;
    public static JsonTypeInfo<GameGuiSettingObj> GameGuiSettingObj => SourceGenerationContext.Default.GameGuiSettingObj;
    public static JsonTypeInfo<List<FrpCloudObj>> ListFrpCloudObj => SourceGenerationContext.Default.ListFrpCloudObj;
}
