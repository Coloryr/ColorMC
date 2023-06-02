using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Gui.Objs;

/// <summary>
/// �ṹ�ļ���ʾ
/// </summary>
public record SchematicDisplayObj
{
    /// <summary>
    /// ����
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// ·��
    /// </summary>
    public string Local { get; set; }
    /// <summary>
    /// ��
    /// </summary>
    public int Height { get; set; }
    /// <summary>
    /// ��
    /// </summary>
    public int Length { get; set; }
    /// <summary>
    /// ��
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// ����
    /// </summary>
    public string Author { get; set; }
    /// <summary>
    /// ����
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// ����
    /// </summary>
    public SchematicObj Schematic;
}
