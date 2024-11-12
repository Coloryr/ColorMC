using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Login;

public record AuthenticateObj
{
    public record AgentObj
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
    }
    [JsonProperty("agent")]
    public AgentObj Agent { get; set; }
    [JsonProperty("username")]
    public string Username { get; set; }
    [JsonProperty("password")]
    public string Password { get; set; }
    [JsonProperty("clientToken")]
    public string ClientToken { get; set; }
    //public bool requestUser { get; set; }
}

public record AuthenticateResObj
{
    public record SelectedProfileObj
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
    }
    //public record User
    //{
    //    public string id { get; set; }
    //}
    [JsonProperty("accessToken")]
    public string AccessToken { get; set; }
    [JsonProperty("clientToken")]
    public string ClientToken { get; set; }
    [JsonProperty("selectedProfile")]
    public SelectedProfileObj SelectedProfile { get; set; }
    [JsonProperty("availableProfiles")]
    public List<SelectedProfileObj> AvailableProfiles { get; set; }
    //public User user { get; set; }
}
