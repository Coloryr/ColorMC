using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Http.Login;

public enum AuthState
{
    OAuth, XBox, XSTS, Checking, Profile,
    Error
}

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

public record OAuth2Obj
{
    public string access_token { get; set; }
    public int expires_in { get; set; }
    public string id_token { get; set; }
    public string refresh_token { get; set; }
}

public class OAuth
{
    private const string Url1 = "https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode";
    private const string Url2 = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";

    public static Dictionary<string, string> Arg1 = new()
    {
        { "client_id", "aa0dd576-d717-4950-b257-a478d2c20968"},
        { "scope", "XboxLive.signin offline_access"}
    };

    public static Dictionary<string, string> Arg2 = new()
    {
        { "client_id", "aa0dd576-d717-4950-b257-a478d2c20968"},
        { "grant_type", "urn:ietf:params:oauth:grant-type:device_code"},
        { "code", "" }
    };

    public static Dictionary<string, string> Arg3 = new()
    {
        { "client_id", "aa0dd576-d717-4950-b257-a478d2c20968"},
        { "grant_type", "refresh_token"},
        { "refresh_token", "" }
    };

    public AuthState state { get; private set; }
    public string code { get; private set; }
    public string url { get; private set; }
    public async Task StartLogin()
    {
        state = AuthState.OAuth;
        try
        {
            var oauth = await GoOAuth();
            if (oauth == null)
                return;
            var XBL = await AuthenticateXBLAsync(oauth.access_token);
        }
        catch (Exception e)
        {

        }
    }

    public async Task<OAuth1Obj> GoOAuth()
    {
        var data = await BaseClient.PostString(Url1, Arg1);
        var obj1 = JObject.Parse(data);
        if (obj1.ContainsKey("error"))
        {
            state = AuthState.Error;
            return null;
        }
        var obj2 = obj1.ToObject<OAuthObj>();
        code = obj2.user_code;
        url = obj2.verification_uri;
        Console.WriteLine(code);
        Console.WriteLine(url);
        Arg2["code"] = obj2.device_code;
        long startTime = DateTime.Now.Ticks;
        int delay = 5;
        OAuth1Obj obj4;
        do
        {
            Thread.Sleep(delay * 1000);
            long estimatedTime = DateTime.Now.Ticks - startTime;
            long sec = estimatedTime / 10000000;
            if (sec > obj2.expires_in)
            {
                state = AuthState.Error;
                return null;
            }
            data = await BaseClient.PostString(Url2, Arg2);
            var obj3 = JObject.Parse(data);
            if (obj3.ContainsKey("error"))
            {
                string error = obj3["error"].ToString();
                if (error == "authorization_pending")
                {
                    continue;
                }
                else if (error == "slow_down")
                {
                    delay += 5;
                }
                else if (error == "expired_token")
                {
                    state = AuthState.Error;
                    return null;
                }
            }
            else
            {
                obj4 = obj3.ToObject<OAuth1Obj>();
                return obj4;
            }
        } while (true);
    }

    //#region Exposed properties

    ///// <summary>
    ///// If you don't specify your azure id, Mojang's will be used as default
    ///// </summary>
    //public string ClientId { get; set; } = "00000000402b5328";

    ///// <summary>
    ///// If you don't specify your redirect url, a default one will be used
    ///// </summary>
    //public string RedirectUrl { get; set; } = "https://login.live.com/oauth20_desktop.srf";

    ///// <summary>
    ///// The code from the browser
    ///// <example>M.R3_BAY.415395f4-181b-8f6e-3ec7-b4749c13c742</example>
    ///// <remarks>Check the documentation if you don't really know what is this</remarks>
    ///// </summary>
    //public string Code { get; set; }

    //#endregion

    //#region Consts

    ///// <summary>
    ///// Microsoft login url, this procedure needs to be done in browser/webview
    ///// </summary>
    //public string LoginUrl =>
    //    $"https://login.live.com/oauth20_authorize.srf?client_id={ClientId}&response_type=code&redirect_uri=https://login.live.com/oauth20_desktop.srf&scope=XboxLive.signin%20offline_access";

    //#endregion

    ///// <summary>
    ///// Exchange code for authorization token
    ///// </summary>
    ///// <returns></returns>
    ///// <exception cref="FailedAuthenticationException"></exception>
    //private async Task<(string AccessToken, string RefreshToken)> GetAuthorizationTokenAsync(string? token = null)
    //{
    //    var endpoint = "https://login.live.com/oauth20_token.srf";
    //    object payload = string.IsNullOrWhiteSpace(token)
    //        ? new
    //        {
    //            client_id = ClientId,
    //            code = Code,
    //            grant_type = "authorization_code",
    //            redirect_uri = RedirectUrl,
    //        }
    //        : new
    //        {
    //            client_id = ClientId,
    //            refresh_token = token,
    //            grant_type = "refresh_token",
    //            redirect_uri = RedirectUrl
    //        };

    //    var response = await endpoint.PostUrlEncodedAsync(payload);

    //    try
    //    {
    //        var responseJson = await response.GetStringAsync();
    //        var accessToken = responseJson.Fetch("access_token");
    //        var refreshToken = responseJson.Fetch("refresh_token");

    //        if (string.IsNullOrWhiteSpace(responseJson) || string.IsNullOrWhiteSpace(accessToken) ||
    //            string.IsNullOrWhiteSpace(refreshToken))
    //        {
    //            throw new FailedAuthenticationException("Response JSON is invalid");
    //        }

    //        return (accessToken, refreshToken);
    //    }
    //    catch (Exception e)
    //    {
    //        throw new FailedAuthenticationException("Failed to authorize", e);
    //    }
    //}

    /// <summary>
    /// Get Xbox live token & userhash
    /// </summary>
    /// <returns></returns>
    private async Task<(string XNLToken, string XBLUhs)> AuthenticateXBLAsync(string token)
    {
        try
        {
            var endpoint = "https://user.auth.xboxlive.com/user/authenticate";
            var rpsTicket = $"d={token}";

            var payload = new
            {
                Properties = new
                {
                    AuthMethod = "RPS",
                    SiteName = "user.auth.xboxlive.com",
                    RpsTicket = rpsTicket
                },
                RelyingParty = "http://auth.xboxlive.com",
                TokenType = "JWT"
            };

            var json = await BaseClient.PostObj(endpoint, payload);
            var xblToken = json.GetValue("Token").ToString();
            var list = json.GetValue("DisplayClaims.xui") as JArray;
            var xblUhs = (list.First() as JObject).GetValue("uhs").ToString();

            if (string.IsNullOrWhiteSpace(xblToken) ||
                string.IsNullOrWhiteSpace(xblUhs))
            {
                throw new Exception("Invalid JSON response");
            }

            return (xblToken, xblUhs);
        }
        catch (Exception e)
        {
            throw new Exception("Failed to process Xbox live authentication", e);
        }
    }

    ///// <summary>
    ///// Get Xbox security token service token & userhash
    ///// </summary>
    ///// <returns></returns>
    ///// <exception cref="FailedAuthenticationException"></exception>
    //private async Task<(string XSTSToken, string XSTSUhs)> AuthenticateXSTSAsync(string token)
    //{
    //    try
    //    {
    //        var xblAuth = await AuthenticateXBLAsync(token);
    //        var endpoint = "https://xsts.auth.xboxlive.com/xsts/authorize";
    //        var payload = new
    //        {
    //            Properties = new
    //            {
    //                SandboxId = "RETAIL",
    //                UserTokens = new[]
    //                {
    //                    xblAuth.XNLToken
    //                }
    //            },
    //            RelyingParty = "rp://api.minecraftservices.com/",
    //            TokenType = "JWT"
    //        };

    //        var response = await endpoint.PostJsonAsync(payload);
    //        var json = await response.GetStringAsync();
    //        var xstsToken = json.Fetch("Token");
    //        var xstsUhs = json.FetchJToken("DisplayClaims.xui")?.First?.Fetch("uhs");

    //        if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(xstsToken) ||
    //            string.IsNullOrWhiteSpace(xstsUhs))
    //        {
    //            if (json.IsValidJson())
    //            {
    //                HandleXSTSErrorCode(json);
    //            }

    //            throw new FailedAuthenticationException("Invalid JSON response");
    //        }

    //        return (xstsToken, xstsUhs);
    //    }
    //    catch (Exception e)
    //    {
    //        Console.WriteLine(e.Message);
    //        throw;
    //    }
    //}

    //private async Task<(string AccessToken, TimeSpan ExpiresIn)> AuthenticateMinecraftAsync(string token)
    //{
    //    try
    //    {
    //        var xstsAuth = await AuthenticateXSTSAsync(token);
    //        var endpoint = "https://api.minecraftservices.com/authentication/login_with_xbox";
    //        var payload = new
    //        {
    //            identityToken = $"XBL3.0 x={xstsAuth.XSTSUhs};{xstsAuth.XSTSToken}"
    //        };

    //        var response = await endpoint.PostJsonAsync(payload);
    //        var json = await response.GetStringAsync();

    //        var accessToken = json.Fetch("access_token");
    //        var expireTime = json.Fetch("expires_in");

    //        if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(accessToken) ||
    //            string.IsNullOrWhiteSpace(expireTime))
    //        {
    //            throw new FailedAuthenticationException("Invalid response JSON");
    //        }

    //        return (accessToken, TimeSpan.FromSeconds(double.Parse(expireTime)));
    //    }
    //    catch (Exception e)
    //    {
    //        throw new FailedAuthenticationException("Failed to authenticate with Minecraft", e);
    //    }
    //}

    ///// <summary>
    ///// Get player's minecraft profile
    ///// </summary>
    ///// <param name="accessToken">Minecraft access token</param>
    ///// <returns></returns>
    ///// <exception cref="FailedAuthenticationException">If authenticated user don't have minecraft, the exception will be thrown</exception>
    //public async Task<MinecraftProfile> GetMinecraftProfileAsync(string accessToken)
    //{
    //    var endpoint = "https://api.minecraftservices.com/minecraft/profile";
    //    var response = await endpoint
    //        .WithHeader("Authorization", $"Bearer {accessToken}")
    //        .GetStringAsync();

    //    var re = JsonConvert.DeserializeObject<MinecraftProfile>(response);
    //    if (re == null)
    //    {
    //        throw new FailedAuthenticationException("Failed to retrieve Minecraft profile");
    //    }

    //    return re;
    //}

    ///// <summary>
    ///// Check if authenticated user have minecraft
    ///// </summary>
    ///// <param name="accessToken"></param>
    ///// <returns></returns>
    //public async Task<bool> CheckOwnershipAsync(string accessToken)
    //{
    //    try
    //    {
    //        _ = await GetMinecraftProfileAsync(accessToken);
    //        return true;
    //    }
    //    catch (FailedAuthenticationException)
    //    {
    //        return false;
    //    }
    //}

    ///// <summary>
    ///// Authenticate minecraft
    ///// </summary>
    ///// <returns></returns>
    //public async Task<AuthenticateResult> AuthenticateAsync()
    //{
    //    var token = await GetAuthorizationTokenAsync();
    //    var mcAuth = await AuthenticateMinecraftAsync(token.AccessToken);
    //    var profile = await GetMinecraftProfileAsync(mcAuth.AccessToken);

    //    return new AuthenticateResult
    //    {
    //        Name = profile.Name,
    //        AccessToken = mcAuth.AccessToken,
    //        UUID = profile.Id,
    //        RefreshToken = token.RefreshToken,
    //        ExpireIn = mcAuth.ExpiresIn
    //    };
    //}

    ///// <summary>
    ///// Refresh authentication, getting a new access token after old one expired
    ///// </summary>
    ///// <param name="refreshToken"></param>
    ///// <returns></returns>
    //public async Task<AuthenticateResult> RefreshAuthenticateAsync(string refreshToken)
    //{
    //    var token = await GetAuthorizationTokenAsync(refreshToken);
    //    var mcAuth = await AuthenticateMinecraftAsync(token.AccessToken);
    //    var profile = await GetMinecraftProfileAsync(mcAuth.AccessToken);

    //    return new AuthenticateResult
    //    {
    //        Name = profile.Name,
    //        AccessToken = mcAuth.AccessToken,
    //        UUID = profile.Id,
    //        RefreshToken = token.RefreshToken,
    //        ExpireIn = mcAuth.ExpiresIn
    //    };
    //}

    //private void HandleXSTSErrorCode(string json)
    //{
    //    var errorCode = json.Fetch("XErr");
    //    if (string.IsNullOrWhiteSpace(errorCode))
    //    {
    //        throw new FailedAuthenticationException("Invalid JSON response");
    //    }

    //    switch (errorCode)
    //    {
    //        case "2148916233":
    //            throw new FailedAuthenticationException(
    //                "The account doesn't have an Xbox account. Once they sign up for one (or login through minecraft.net to create one) then they can proceed with the login. This shouldn't happen with accounts that have purchased Minecraft with a Microsoft account, as they would've already gone through that Xbox signup process.");
    //        case "2148916235":
    //            throw new FailedAuthenticationException(
    //                "The account is from a country where Xbox Live is not available/banned");
    //        case "2148916236":
    //        case "2148916237":
    //            throw new FailedAuthenticationException("The account needs adult verification on Xbox page.");
    //        case "2148916238":
    //            throw new FailedAuthenticationException(
    //                "The account is a child (under 18) and cannot proceed unless the account is added to a Family by an adult. This only seems to occur when using a custom Microsoft Azure application. When using the Minecraft launchers client id, this doesn't trigger.");
    //        default:
    //            throw new FailedAuthenticationException(
    //                $"Authenticate failed with code {errorCode}: {json.Fetch("Message")}");
    //    }
    //}
}
