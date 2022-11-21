namespace ColorMC.Core.Objs.Game;

public record RefreshObj
{
    public string accessToken { get; set; }
    public string clientToken { get; set; }
    public bool requestUser { get; set; }
}
