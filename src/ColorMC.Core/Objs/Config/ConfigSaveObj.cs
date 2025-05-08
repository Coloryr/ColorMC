using System.Text.Json.Serialization.Metadata;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Objs.Config;

/// <summary>
/// 配置文件保存项目
/// </summary>
public record ConfigSaveObj
{
    /// <summary>
    /// 名字
    /// </summary>
    public required string Name;
    /// <summary>
    /// 文件名
    /// </summary>
    public required string File;
    /// <summary>
    /// 执行方式
    /// </summary>
    public required Func<string> Run;

    /// <summary>
    /// 创建保存项目
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="data"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public static ConfigSaveObj Build<T>(string name, string file, T data, JsonTypeInfo<T> info)
    {
        return new()
        {
            Name = name,
            File = file,
            Run = () => 
            {
                return JsonUtils.ToString(data, info);
            }
        };
    }
}
