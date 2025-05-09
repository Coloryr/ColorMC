using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Login;

public record RefreshObj
{
    public record RefreshSelectedProfileObj
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }
    [JsonPropertyName("clientToken")]
    public string ClientToken { get; set; }
    //public bool requestUser { get; set; }
    [JsonPropertyName("selectedProfile")]
    public RefreshSelectedProfileObj SelectedProfile { get; set; }
}
