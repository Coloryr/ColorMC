using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Login;

public record AuthenticateObj
{
    public record AgentObj
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("version")]
        public int Version { get; set; }
    }
    [JsonPropertyName("agent")]
    public AgentObj Agent { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("password")]
    public string Password { get; set; }
    [JsonPropertyName("clientToken")]
    public string ClientToken { get; set; }
    //public bool requestUser { get; set; }
}

public record AuthenticateResObj
{
    public record AuthenticateResSelectedProfileObj
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
    //public record User
    //{
    //    public string id { get; set; }
    //}
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }
    [JsonPropertyName("clientToken")]
    public string ClientToken { get; set; }
    [JsonPropertyName("selectedProfile")]
    public AuthenticateResSelectedProfileObj SelectedProfile { get; set; }
    [JsonPropertyName("availableProfiles")]
    public List<AuthenticateResSelectedProfileObj> AvailableProfiles { get; set; }
    //public User user { get; set; }
    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; }
    [JsonPropertyName("error")]
    public object Error { get; set; }
}
