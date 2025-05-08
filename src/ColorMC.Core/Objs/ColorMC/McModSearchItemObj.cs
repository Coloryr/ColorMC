using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.ColorMC;

public record McModSearchItemObj
{
    [JsonPropertyName("mcmod_id")]
    public int McmodId { get; set; }
    [JsonPropertyName("mcmod_icon")]
    public string McmodIcon { get; set; }
    [JsonPropertyName("mcmod_name")]
    public string McmodName { get; set; }
    [JsonPropertyName("mcmod_author")]
    public string McmodAuthor { get; set; }
    [JsonPropertyName("mcmod_text")]
    public string McmodText { get; set; }
    [JsonPropertyName("mcmod_type")]
    public int McmodType { get; set; }
    [JsonPropertyName("mcmod_mod_type")]
    public string McmodModType { get; set; }
    [JsonPropertyName("mcmod_mod_subtype")]
    public string McmodModSubtype { get; set; }
    [JsonPropertyName("mcmod_game_version")]
    public string McmodGameVersion { get; set; }
    [JsonPropertyName("mcmod_create_time")]
    public DateTime McmodCreateTime { get; set; }
    [JsonPropertyName("mcmod_update_time")]
    public DateTime McmodUpdateTime { get; set; }
    [JsonPropertyName("mcmod_re_time")]
    public DateTime McmodReTime { get; set; }
    [JsonPropertyName("curseforge_url")]
    public string? CurseforgeUrl { get; set; }
    [JsonPropertyName("curseforge_id")]
    public string? CurseforgeId { get; set; }
    [JsonPropertyName("modrinth_url")]
    public string? ModrinthUrl { get; set; }
    [JsonPropertyName("modrinth_id")]
    public string? ModrinthId { get; set; }
}
