namespace ColorMC.Gui.SkinModel;

public record SteveTexture
{
    public float[] Head;
    public float[] Body;
    public float[] LeftArm;
    public float[] RightArm;
    public float[] LeftLeg;
    public float[] RightLeg;
}

public class Steve3DTexture
{
    public SteveTexture GetSteveTextureTop(SkinType type)
    {
        SteveTexture tex = new()
        {
            Head = GetTex(HeadTex, type, 32f, 0f)
        };

        if (type != SkinType.Old)
        {
            tex.Body = GetTex(BodyTex, type, 16f, 32f);
            tex.LeftArm = GetTex(type == SkinType.New_Slim ? SlimArmTex : LegArmTex, type, 48f, 48f);
            tex.RightArm = GetTex(type == SkinType.New_Slim ? SlimArmTex : LegArmTex, type, 40f, 32f);
            tex.LeftLeg = GetTex(LegArmTex, type, 0f, 48f);
            tex.RightLeg = GetTex(LegArmTex, type, 0f, 32f);
        };

        return tex;
    }

    public SteveTexture GetSteveTexture(SkinType type)
    {
        SteveTexture tex = new()
        {
            Head = GetTex(HeadTex, type),
            Body = GetTex(BodyTex, type, 16f, 16f),
        };

        if (type == SkinType.Old)
        {
            tex.LeftArm = GetTex(LegArmTex, type, 40f, 16f);
            tex.RightArm = GetTex(LegArmTex, type, 40f, 16f);
            tex.LeftLeg = GetTex(LegArmTex, type, 0f, 16f);
            tex.RightLeg = GetTex(LegArmTex, type, 0f, 16f);
        }
        else
        {
            tex.LeftArm = GetTex(type == SkinType.New_Slim ? SlimArmTex : LegArmTex, type, 32f, 48f);
            tex.RightArm = GetTex(type == SkinType.New_Slim ? SlimArmTex : LegArmTex, type, 40f, 16f);
            tex.LeftLeg = GetTex(LegArmTex, type, 0f, 16f);
            tex.RightLeg = GetTex(LegArmTex, type, 16f, 48f);
        }

        return tex;
    }

    private float[] HeadTex = new float[]
    {
        // back
        32f, 8f, 32f, 16f, 24f, 16f, 24f, 8f,
        // front
        8f, 8f, 8f, 16f, 16f, 16f, 16f, 8f,
        // left
        0f, 8f, 0f, 16f, 8f, 16f, 8f, 8f,
        // right
        16f, 8f, 16f, 16f, 24f, 16f, 24f, 8f,
        // top
        8f, 0f, 8f, 8f, 16f, 8f, 16f, 0f,
        // bottom
        24f, 0f, 24f, 8f, 16f, 8f, 16f, 0f
    };

    private float[] LegArmTex = new float[]
    {
        // back
        12f, 4f, 12f, 16f, 16f, 16f, 16f, 4f,
        // front
        4f, 4f, 4f, 16f, 8f, 16f, 8f, 4f,
        // left
        0f, 4f, 0f, 16f, 4f, 16f, 4f, 4f,
        // right
        8f, 4f, 8f, 16f, 12f, 16f, 12f, 4f,
        // top
        4f, 0f, 4f, 4f, 8f, 4f, 8f, 0f,
        // bottom
        12f, 0f, 12f, 4f, 8f, 4f, 8f, 0f,
    };

    private float[] SlimArmTex = new float[]
    {
        // back
        11f, 4f, 11f, 16f, 14f, 16f, 14f, 4f,
        // front
        4f, 4f, 4f, 16f, 7f, 16f, 7f, 4f,
        // left
        0f, 4f, 0f, 16f, 4f, 16f, 4f, 4f,
        // right
        7f, 4f, 7f, 16f, 10f, 16f, 10f, 4f,
        // top
        4f, 0f, 4f, 4f, 7f, 4f, 7f, 0f,
        // bottom
        10f, 0f, 10f, 4f, 7f, 4f, 7f, 0f,
    };

    private float[] BodyTex = new float[]
    {
        // back
        24f, 4f, 24f, 16f, 16f, 16f, 16f, 4f,
        // front
        4f, 4f, 4f, 16f, 12f, 16f, 12f, 4f,
        // left
        0f, 4f, 0f, 16f, 4f, 16f, 4f, 4f,
        // right
        12f, 4f, 12f, 16f, 16f, 16f, 16f, 4f,
        // top
        4f, 0f, 4f, 4f, 12f, 4f, 12f, 0f,
        // bottom
        20f, 0f, 20f, 4f, 12f, 4f, 12f, 0f
    };

    public float[] GetTex(float[] input, SkinType type,
        float offsetU = 0f,
        float offsetV = 0f)
    {
        var temp = new float[input.Length];
        for (int a = 0; a < input.Length; a++)
        {
            if (a % 2 == 0)
            {
                temp[a] = input[a] + offsetU;
            }
            else
            {
                temp[a] = input[a] + offsetV;
            }

            if (a % 2 != 0 && type == SkinType.Old)
            {
                temp[a] /= 32f;
            }
            else
            {
                temp[a] /= 64f;
            }
        }

        return temp;
    }
}
