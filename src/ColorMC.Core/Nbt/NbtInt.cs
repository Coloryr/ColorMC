namespace ColorMC.Core.Nbt;

public class NbtInt : NbtBase
{
    public const byte Type = 3;

    public int Value { get; set; }

    public NbtInt()
    {
        NbtType = NbtType.NbtInt;
    }

    public override NbtInt Read(DataInputStream stream)
    {
        Value = stream.ReadInt();
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
