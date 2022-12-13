namespace ColorMC.Core.Objs.Login;

public record RefreshObj
{
    public string accessToken { get; set; }
    public string clientToken { get; set; }
    public bool requestUser { get; set; }
}
