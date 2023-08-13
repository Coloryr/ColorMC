namespace ColorMC.Gui.Skin;

public record ModelItem
{
    public float[] Model;
    public ushort[] Point;
}

public record SteveModel
{
    public ModelItem Head;
    public ModelItem Body;
    public ModelItem LeftArm;
    public ModelItem RightArm;
    public ModelItem LeftLeg;
    public ModelItem RightLeg;
    public ModelItem Cape;
}

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