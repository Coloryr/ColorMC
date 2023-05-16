namespace ColorMC.Core.Nbt;

public class NbtFloat : NbtBase
{
    public const byte Type = 5;

    public float Value { get; set; }

    public NbtFloat()
    {
        NbtType = NbtType.NbtFloat;
    }

    public override NbtFloat Read(DataInputStream stream)
    {
        Value = stream.ReadFloat();
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
