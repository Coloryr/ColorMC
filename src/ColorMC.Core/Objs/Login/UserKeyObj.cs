namespace ColorMC.Core.Objs.Login;

public record UserKeyObj
{
    public required string UUID { get; init; }
    public required AuthType Type { get; init; }
}
