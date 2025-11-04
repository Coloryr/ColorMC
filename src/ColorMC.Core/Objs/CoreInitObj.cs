namespace ColorMC.Core.Objs;

/// <summary>
/// 核心初始化参数
/// </summary>
public record CoreInitObj
{
    /// <summary>
    /// 运行的路径
    /// </summary>
    public required string Local;
    /// <summary>
    /// OAuth客户端密钥
    /// </summary>
    public string? OAuthKey;
    /// <summary>
    /// CurseForge客户端密钥
    /// </summary>
    public string? CurseForgeKey;
}
