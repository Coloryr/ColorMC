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
    /// ����
    /// </summary>
    public required object Obj;
    /// <summary>
    /// ·��
    /// </summary>
    public required string File;
}
