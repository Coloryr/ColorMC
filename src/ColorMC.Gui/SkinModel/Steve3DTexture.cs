using ColorMC.Gui.Objs;

namespace ColorMC.Gui.SkinModel;

public class Steve3DTexture
{
    public SteveTexture GetSteveTextureTop(SkinType type)
    {
        SteveTexture tex = new()
        {
            Head = GetTex(HeadTex, type, 32f, 0f),
        };

        if (type != SkinType.Old)
        {
            tex.Body = GetTex(BodyTex, type, 16f, 32f);
            tex.LeftArm = GetTex(type == SkinType.NewSlim ? SlimArmTex : LegArmTex, type, 48f, 48f);
            tex.RightArm = GetTex(type == SkinType.NewSlim ? SlimArmTex : LegArmTex, type, 40f, 32f);
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
            Cape = GetTex1(CapeTex),
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
            tex.LeftArm = GetTex(type == SkinType.NewSlim ? SlimArmTex : LegArmTex, type, 32f, 48f);
            tex.RightArm = GetTex(type == SkinType.NewSlim ? SlimArmTex : LegArmTex, type, 40f, 16f);
            tex.LeftLeg = GetTex(LegArmTex, type, 0f, 16f);
            tex.RightLeg = GetTex(LegArmTex, type, 16f, 48f);
        }

        return tex;
    }

    private readonly float[] HeadTex = new float[]
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

    private readonly float[] LegArmTex = new float[]
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

    private readonly float[] SlimArmTex = new float[]
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

    private readonly float[] BodyTex = new float[]
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

    private readonly float[] CapeTex = new float[]
    {
        // back
        11f, 1f, 11f, 17f, 1f, 17f, 1f, 1f,
        // front
        12f, 1f, 12f, 17f, 22f, 17f, 22f, 1f,
        // left
        11f, 1f, 11f, 17f, 12f, 17f, 12f, 1f, 
        // right
        0f, 1f, 0f, 17f, 1f, 17f, 1f, 1f,
        // top
        1f, 0f,1f, 1f, 11f, 1f, 11f, 0f, 
        // bottom
        21f, 0f, 21f, 1f, 11f, 1f, 11f, 0f,
    };

    public static float[] GetTex(float[] input, SkinType type,
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

    public static float[] GetTex1(float[] input)
    {
        var temp = new float[input.Length];
        for (int a = 0; a < input.Length; a++)
        {
            temp[a] = input[a];
            if (a % 2 == 0)
            {
                temp[a] /= 64f;
            }
            else
            {
                temp[a] /= 32f;
            }
        }

        return temp;
    }
}
