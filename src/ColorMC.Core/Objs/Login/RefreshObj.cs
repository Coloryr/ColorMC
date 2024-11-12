using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Login;

public record RefreshObj
{
    public record SelectedProfileObj
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    [JsonProperty("accessToken")]
    public string AccessToken { get; set; }
    [JsonProperty("clientToken")]
    public string ClientToken { get; set; }
    //public bool requestUser { get; set; }
    [JsonProperty("selectedProfile")]
    public SelectedProfileObj SelectedProfile { get; set; }
}
