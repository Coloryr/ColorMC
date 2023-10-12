namespace ColorMC.Gui.Objs;

/// <summary>
/// 一个方块模型数据
/// </summary>
public record CubeModelItem
{
    public float[] Model;
    public ushort[] Point;
}

/// <summary>
/// 一个史蒂夫模型数据
/// </summary>
public record SteveModel
{
    public CubeModelItem Head;
    public CubeModelItem Body;
    public CubeModelItem LeftArm;
    public CubeModelItem RightArm;
    public CubeModelItem LeftLeg;
    public CubeModelItem RightLeg;
    public CubeModelItem Cape;
}

/// <summary>
/// 模型贴图数据
/// </summary>
public record SteveTexture
{
    public float[] Head;
    public float[] Body;
    public float[] LeftArm;
    public float[] RightArm;
    public float[] LeftLeg;
    public float[] RightLeg;
    public float[] Cape;
}