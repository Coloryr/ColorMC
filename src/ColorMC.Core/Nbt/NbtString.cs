namespace ColorMC.Core.Nbt;

public class NbtString : NbtBase
{
    public const byte Type = 8;

    public new string Value { get; set; }

    public NbtString()
    {
        NbtType = NbtType.NbtString;
        Value ??= "";
    }

    public override NbtString Read(DataInputStream stream)
    {
        Value = stream.ReadString();
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
