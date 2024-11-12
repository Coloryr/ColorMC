using Newtonsoft.Json;

namespace ColorMC.Core.Objs.McMod;

public record McModSearchItemObj
{
    [JsonProperty("mcmod_id")]
    public int McmodId { get; set; }
    [JsonProperty("mcmod_icon")]
    public string McmodIcon { get; set; }
    [JsonProperty("mcmod_name")]
    public string McmodName { get; set; }
    [JsonProperty("mcmod_author")]
    public string McmodAuthor { get; set; }
    [JsonProperty("mcmod_text")]
    public string McmodText { get; set; }
    [JsonProperty("mcmod_type")]
    public int McmodType { get; set; }
    [JsonProperty("mcmod_mod_type")]
    public string McmodModType { get; set; }
    [JsonProperty("mcmod_mod_subtype")]
    public string McmodModSubtype { get; set; }
    [JsonProperty("mcmod_game_version")]
    public string McmodGameVersion { get; set; }
    [JsonProperty("mcmod_create_time")]
    public DateTime McmodCreateTime { get; set; }
    [JsonProperty("mcmod_update_time")]
    public DateTime McmodUpdateTime { get; set; }
    [JsonProperty("mcmod_re_time")]
    public DateTime McmodReTime { get; set; }
    [JsonProperty("curseforge_url")]
    public string? CurseforgeUrl { get; set; }
    [JsonProperty("curseforge_id")]
    public string? CurseforgeId { get; set; }
    [JsonProperty("modrinth_url")]
    public string? ModrinthUrl { get; set; }
    [JsonProperty("modrinth_id")]
    public string? ModrinthId { get; set; }
}
