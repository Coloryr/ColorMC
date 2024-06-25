namespace ColorMC.Core.Objs.McMod;

public record McModSearchItemObj
{
    public int mcmod_id { get; set; }
    public string mcmod_icon { get; set; }
    public string mcmod_name { get; set; }
    public string mcmod_author { get; set; }
    public string mcmod_text { get; set; }
    public int mcmod_type { get; set; }
    public string mcmod_mod_type { get; set; }
    public string mcmod_mod_subtype { get; set; }
    public string mcmod_game_version { get; set; }
    public DateTime mcmod_create_time { get; set; }
    public DateTime mcmod_update_time { get; set; }
    public DateTime mcmod_re_time { get; set; }
    public string? curseforge_url { get; set; }
    public string? curseforge_id { get; set; }
    public string? modrinth_url { get; set; }
    public string? modrinth_id { get; set; }
}
