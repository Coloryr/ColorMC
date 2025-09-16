﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 游戏数据
/// </summary>
public record GameArgObj
{
    public record GameArgumentsObj
    {
        public record GameJvmObj
        {
            [JsonPropertyName("rules")]
            public List<GameLibrariesObj.GameRulesObj> Rules { get; set; }
            [JsonPropertyName("value")]
            public JsonElement Value { get; set; }
        }
        [JsonPropertyName("game")]
        public List<JsonElement> Game { get; set; }
        [JsonPropertyName("jvm")]
        public List<JsonElement> Jvm { get; set; }
    }
    public record GameAssetIndexObj
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        //public string sha1 { get; set; }
        //public long size { get; set; }
        //public long totalSize { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
    public record GameDownloadsObj
    {
        public record GameDownloadItemObj
        {
            [JsonPropertyName("sha1")]
            public string Sha1 { get; set; }
            //public long size { get; set; }
            [JsonPropertyName("url")]
            public string Url { get; set; }
        }
        [JsonPropertyName("client")]
        public GameDownloadItemObj Client { get; set; }
        //public Download client_mappings { get; set; }
        //public Download server { get; set; }
        //public Download server_mappings { get; set; }
    }
    public record GameJavaVersionObj
    {
        //public string component { get; set; }
        [JsonPropertyName("majorVersion")]
        public int MajorVersion { get; set; }
    }
    public record GameLibrariesObj
    {
        public record GameRulesObj
        {
            public record GameOsObj
            {
                [JsonPropertyName("name")]
                public string Name { get; set; }
                //public string version { get; set; }
                [JsonPropertyName("arch")]
                public string Arch { get; set; }
            }
            [JsonPropertyName("action")]
            public string Action { get; set; }
            [JsonPropertyName("os")]
            public GameOsObj Os { get; set; }
        }
        public record GameLibrariesDownloadsObj
        {
            public record ArtifactObj
            {
                [JsonPropertyName("path")]
                public string Path { get; set; }
                [JsonPropertyName("sha1")]
                public string Sha1 { get; set; }
                //public long size { get; set; }
                [JsonPropertyName("url")]
                public string Url { get; set; }
            }
            public record ClassifiersObj
            {
                [JsonPropertyName("natives-linux")]
                public ArtifactObj NativesLinux { get; set; }
                [JsonPropertyName("natives-osx")]
                public ArtifactObj NativesOsx { get; set; }
                [JsonPropertyName("natives-windows")]
                public ArtifactObj NativesWindows { get; set; }
                [JsonPropertyName("natives-windows-32")]
                public ArtifactObj NativesWindows32 { get; set; }
                [JsonPropertyName("natives-windows-64")]
                public ArtifactObj NativesWindows64 { get; set; }
            }
            [JsonPropertyName("classifiers")]
            public ClassifiersObj Classifiers { get; set; }
            [JsonPropertyName("artifact")]
            public ArtifactObj Artifact { get; set; }
        }
        //public record Natives
        //{
        //    public string linux { get; set; }
        //    public string osx { get; set; }
        //    public string windows { get; set; }
        //}
        //public Natives natives { get; set; }
        [JsonPropertyName("downloads")]
        public GameLibrariesDownloadsObj Downloads { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("rules")]
        public List<GameRulesObj> Rules { get; set; }
        //decode mmc pack use
        //[JsonProperty("MMC-hint")]
        //public string MMCHint { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
    public record LoggingObj
    {
        public record ClientObj
        {
            public record FileObj
            {
                //public string id { get; set; }
                [JsonPropertyName("sha1")]
                public string Sha1 { get; set; }
                //public long size { get; set; }
                [JsonPropertyName("url")]
                public string Url { get; set; }
            }
            [JsonPropertyName("argument")]
            public string Argument { get; set; }
            [JsonPropertyName("file")]
            public FileObj File { get; set; }
            //public string type { get; set; }
        }
        [JsonPropertyName("client")]
        public ClientObj Client { get; set; }
    }

    [JsonPropertyName("assetIndex")]
    public GameAssetIndexObj? AssetIndex { get; set; }
    //[JsonProperty("assets")]
    //public string Assets { get; set; }
    //public int complianceLevel { get; set; }
    [JsonPropertyName("downloads")]
    public GameDownloadsObj Downloads { get; set; }
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("javaVersion")]
    public GameJavaVersionObj? JavaVersion { get; set; }
    [JsonPropertyName("libraries")]
    public List<GameLibrariesObj>? Libraries { get; set; }
    [JsonPropertyName("logging")]
    public LoggingObj? Logging { get; set; }
    [JsonPropertyName("mainClass")]
    public string? MainClass { get; set; }
    [JsonPropertyName("minecraftArguments")]
    public string? MinecraftArguments { get; set; }
    [JsonPropertyName("minimumLauncherVersion")]
    public int MinimumLauncherVersion { get; set; }
    [JsonPropertyName("releaseTime")]
    public string ReleaseTime { get; set; }
    //public string time { get; set; }
    //public string type { get; set; }
    [JsonPropertyName("arguments")]
    public GameArgumentsObj? Arguments { get; set; }
}
