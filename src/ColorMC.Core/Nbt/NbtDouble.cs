namespace ColorMC.Core.Nbt;

public class NbtDouble : NbtBase
{
    public const byte Type = 6;

    public double Value { get; set; }

    public NbtDouble()
    {
        NbtType = NbtType.NbtDouble;
    }

    public override NbtDouble Read(DataInputStream stream)
    {
        Value = stream.ReadDouble();
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
