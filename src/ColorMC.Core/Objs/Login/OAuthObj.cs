namespace ColorMC.Core.Objs.Login;

public record OAuthObj
{
    public string user_code { get; set; }
    public string device_code { get; set; }
    public string verification_uri { get; set; }
    public int expires_in { get; set; }
    public int interval { get; set; }
    public string message { get; set; }
}

public record OAuth1Obj
{
    public string token_type { get; set; }
    public string scope { get; set; }
    public string access_token { get; set; }
    public int expires_in { get; set; }
    public int ext_expires_in { get; set; }
    public string id_token { get; set; }
    public string refresh_token { get; set; }
}