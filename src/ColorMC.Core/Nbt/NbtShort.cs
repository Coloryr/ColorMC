namespace ColorMC.Core.Nbt;

public class NbtShort : NbtBase
{
    public const byte Type = 2;

    public new short Value { get; set; }

    public NbtShort()
    {
        NbtType = NbtType.NbtShort;
    }

    public override NbtShort Read(DataInputStream stream)
    {
        Value = stream.ReadShort();
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
