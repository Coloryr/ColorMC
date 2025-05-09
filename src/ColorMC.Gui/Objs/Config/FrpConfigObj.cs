using System.Collections.Generic;

namespace ColorMC.Gui.Objs.Config;

/// <summary>
/// 樱花映射配置
/// </summary>
public record FrpObj
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
    public FrpObj SakuraFrp { get; set; }

    /// <summary>
    /// 樱花映射
    /// </summary>
    public FrpObj OpenFrp { get; set; }
    /// <summary>
    /// 自定义Frp
    /// </summary>
    public List<FrpSelfObj> SelfFrp { get; set; }
}

/// <summary>
/// 私有Frp
/// </summary>
public record FrpSelfObj
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 远程地址
    /// </summary>
    public string IP { get; set; }
    /// <summary>
    /// 远程端口
    /// </summary>
    public int Port { get; set; }
    /// <summary>
    /// 远程密钥
    /// </summary>
    public string Key { get; set; }
    /// <summary>
    /// 远程用户
    /// </summary>
    public string User { get; set; }
    /// <summary>
    /// 映射端口
    /// </summary>
    public int NetPort { get; set; }
    /// <summary>
    /// 映射名字
    /// </summary>
    public string RName { get; set; }

    public bool IsSame(FrpSelfObj obj)
    {
        return Name == obj.Name
            && IP == obj.IP
            && Port == obj.Port
            && User == obj.User
            && Key == obj.Key
            && RName == obj.RName
            && NetPort == obj.NetPort;
    }
}