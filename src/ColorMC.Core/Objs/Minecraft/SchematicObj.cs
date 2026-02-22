namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 结构文件
/// </summary>
public record SchematicObj
{
    public SchematicType Type { get; set; }
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    public string Local { get; set; }
    /// <summary>
    /// 高
    /// </summary>
    public int Height { get; set; }
    /// <summary>
    /// 长
    /// </summary>
    public int Length { get; set; }
    /// <summary>
    /// 宽
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// 作者
    /// </summary>
    public string Author { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// 方块总数
    /// </summary>
    public int BlockCount { get; set; }
    /// <summary>
    /// 方块种类
    /// </summary>
    public int BlockTypes { get; set; }
    /// <summary>
    /// 方块数量
    /// </summary>
    public Dictionary<string, int> Blocks;

    /// <summary>
    /// 是否已损坏
    /// </summary>
    public bool Broken;
}
