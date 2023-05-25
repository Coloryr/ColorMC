using System.Collections;

namespace ColorMC.Core.Nbt;

public class NbtCompound : NbtBase, IEnumerable<KeyValuePair<string, NbtBase>>
{
    public const int Type = 10;

    private readonly Dictionary<string, NbtBase> Entries = new();

    public int Count => Entries.Count;

    public NbtBase this[string name]
    {
        get
        {
            return Entries[name];
        }
        set
        {
            Entries[name] = value;
        }
    }

    public NbtCompound()
    {
        NbtType = NbtType.NbtCompound;
    }

    public NbtBase? TryGet(string key)
    {
        if (Entries.TryGetValue(key, out var value))
        {
            return value;
        }

        return null;
    }

    public void Add(string key, NbtBase nbt)
    {
        Entries.Add(key, nbt);
    }

    public void Remove(string key)
    {
        Entries.Remove(key);
    }

    public void Clear()
    {
        Entries.Clear();
    }

    public List<string> GetKeys()
    {
        return Entries.Keys.ToList();
    }

    public List<NbtBase> GetValues()
    {
        return Entries.Values.ToList();
    }

    public bool HaveKey(string key)
    {
        return Entries.ContainsKey(key);
    }

    public IEnumerator<KeyValuePair<string, NbtBase>> GetEnumerator()
    {
        return Entries.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Entries.GetEnumerator();
    }

    public override NbtCompound Read(DataInputStream stream)
    {
        byte type;
        while ((type = stream.ReadByte()) != 0)
        {
            string key = stream.ReadString();
            if (type > 12)
            {
                throw new Exception($"type out {key}:{type}");
            }
            var nbt = ById(type);
            nbt.Read(stream);
            Entries.Add(key, nbt);
        }

        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        foreach (var item in Entries)
        {
            stream.Write((byte)item.Value.NbtType);
            if (item.Value.NbtType != NbtType.NbtEnd)
            {
                stream.Write(item.Key);
                item.Value.Write(stream);
            }
        }

        stream.Write((byte)0);
    }

    public void EditKey(string old, string now)
    {
        if (Entries.Remove(old, out var value))
        {
            Entries.Add(now, value);
        }
    }
}
