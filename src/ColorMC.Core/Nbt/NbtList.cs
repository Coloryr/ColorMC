using System.Collections;

namespace ColorMC.Core.Nbt;

public class NbtList : NbtBase, IEnumerable<NbtBase>
{
    public const byte Type = 9;

    private new readonly List<NbtBase> Value = new();
    public NbtType InNbtType { get; set; }

    public NbtBase this[int index]
    {
        get => Value[index];
        set => Value[index] = value;
    }

    public int Count => Value.Count;

    public NbtList()
    {
        NbtType = NbtType.NbtList;
        InNbtType = NbtType.NbtEnd;
    }

    public void Add(NbtBase nbt)
    {
        if (nbt.NbtType != InNbtType)
        {
            return;
        }

        Value.Add(nbt);
    }

    public void RemoveAt(int index)
    {
        Value.RemoveAt(index);
    }

    public void Remove(NbtBase item)
    {
        Value.Remove(item);
    }

    public override NbtList Read(DataInputStream stream)
    {
        var type = stream.ReadByte();
        InNbtType = (NbtType)type;
        int length = stream.ReadInt();
        if (NbtType == 0 && length > 0)
        {
            throw new Exception("Missing type on ListTag");
        }

        for (int a = 0; a < length; a++)
        {
            var nbt = ById(type);
            nbt.Read(stream);
            Value.Add(nbt);
        }

        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        if (Value.Count == 0)
        {
            InNbtType = NbtType.NbtEnd;
        }
        else
        {
            InNbtType = Value.First().NbtType;
        }

        stream.Write((byte)InNbtType);
        stream.Write(Value.Count);

        foreach (var item in Value)
        {
            item.Write(stream);
        }
    }

    public IEnumerator<NbtBase> GetEnumerator()
    {
        return Value.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Value.GetEnumerator();
    }
}
