using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Login;

public record OAuthObj
{
    [JsonPropertyName("user_code")]
    public string UserCode { get; set; }
    [JsonPropertyName("error")]
    public string Error { get; set; }
    [JsonPropertyName("device_code")]
    public string DeviceCode { get; set; }
    [JsonPropertyName("verification_uri")]
    public string VerificationUri { get; set; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    //public int interval { get; set; }
    //public string message { get; set; }
}

public record OAuthGetCodeObj
{
    [JsonPropertyName("error")]
    public string Error { get; set; }
    //public string token_type { get; set; }
    //public string scope { get; set; }
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    //public int expires_in { get; set; }
    //public int ext_expires_in { get; set; }
    //public string id_token { get; set; }
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }
}