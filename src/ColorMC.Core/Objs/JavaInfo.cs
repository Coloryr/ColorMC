using ColorMC.Core.LaunchPath;

namespace ColorMC.Core.Objs;

/// <summary>
/// Java信息
/// </summary>
public record JavaInfo
{
    /// <summary>
    /// 路径
    /// </summary>
    public string Path { get; set; }
    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; set; }
    /// <summary>
    /// 主版本号
    /// </summary>
    public int MajorVersion
    {
        get
        {
            if (Version == JvmPath.Unknow)
            {
                return -1;
            }
            string[] vers = Version.Trim().Split('.', '_', '-', '+', 'u', 'U');
            if (vers[0] == "1")
            {
                if (int.TryParse(vers[1], out var data))
                {
                    return data;
                }

                return 0;
            }
            else
            {
                if (int.TryParse(vers[0], out var data))
                {
                    return data;
                }

                return 0;
            }
        }
    }
    /// <summary>
    /// Java类型
    /// </summary>
    public string Type { get; set; }
    /// <summary>
    /// 进制
    /// </summary>
    public ArchEnum Arch { get; set; }
}
