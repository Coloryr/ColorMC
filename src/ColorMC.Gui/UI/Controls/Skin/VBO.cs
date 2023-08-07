using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Skin;

internal record VAOItem
{
    public int VertexBufferObject;
    public int IndexBufferObject;
    public int VertexArrayObject;
}

internal record ModelVAO
{
    public VAOItem Head = new();
    public VAOItem Body = new();
    public VAOItem LeftArm = new();
    public VAOItem RightArm = new();
    public VAOItem LeftLeg = new();
    public VAOItem RightLeg = new();
    public VAOItem Cape = new();
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct Vertex
{
    public Vector3 Position;
    public Vector2 UV;
    public Vector3 Normal;
}