using System.Text.Json.Serialization.Metadata;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Objs.Config;

/// <summary>
/// �����ļ�������Ŀ
/// </summary>
public record ConfigSaveObj
{
    /// <summary>
    /// ����
    /// </summary>
    public required string Name;
    /// <summary>
    /// �ļ���
    /// </summary>
    public required string File;
    /// <summary>
    /// ִ�з�ʽ
    /// </summary>
    public required Func<string> Run;

    /// <summary>
    /// ����������Ŀ
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
