namespace ColorMC.Core.Objs.Login;

public record RefreshObj
{
    public record SelectedProfile
    {
        public string name { get; set; }
        public string id { get; set; }
    }

    public string accessToken { get; set; }
    public string clientToken { get; set; }
    public bool requestUser { get; set; }
    public SelectedProfile selectedProfile { get; set; }
}
