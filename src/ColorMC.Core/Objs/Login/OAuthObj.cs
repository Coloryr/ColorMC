using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Login;

public record OAuthObj
{
    [JsonProperty("user_code")]
    public string UserCode { get; set; }
    [JsonProperty("device_code")]
    public string DeviceCode { get; set; }
    [JsonProperty("verification_uri")]
    public string VerificationUri { get; set; }
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
    //public int interval { get; set; }
    //public string message { get; set; }
}

public record OAuthGetCodeObj
{
    //public string token_type { get; set; }
    //public string scope { get; set; }
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
    //public int expires_in { get; set; }
    //public int ext_expires_in { get; set; }
    //public string id_token { get; set; }
    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
}