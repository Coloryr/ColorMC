namespace ColorMC.Core.Nbt;

public class NbtEnd : NbtBase
{
    public const byte Type = 0;

    public NbtEnd()
    {
        NbtType = NbtType.NbtEnd;
    }

    public override NbtEnd Read(DataInputStream stream)
    {
        return this;
    }

    public override void Write(DataOutputStream stream)
    {

    }
}
