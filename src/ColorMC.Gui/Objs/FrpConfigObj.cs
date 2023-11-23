namespace ColorMC.Gui.Objs;

/// <summary>
/// 樱花映射配置
/// </summary>
public record SakuraFrpObj
{
    /// <summary>
    /// 密钥
    /// </summary>
    public string Key { get; set; }
}

/// <summary>
/// 映射配置
/// </summary>
public record FrpConfigObj
{
    /// <summary>
    /// 樱花映射
    /// </summary>
    public SakuraFrpObj SakuraFrp { get; set; }
}
