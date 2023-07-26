using System.Collections;

namespace ColorMC.Core.Nbt;

/// <summary>
/// 复合类型的NBT标签
/// </summary>
public class NbtCompound : NbtBase, IEnumerable<KeyValuePair<string, NbtBase>>
{
    /// <summary>
    /// NBT码
    /// </summary>
    public const NbtType Type = NbtType.NbtCompound;

    /// <summary>
    /// NBT数量
    /// </summary>
    public int Count => Entries.Count;

    /// <summary>
    /// 数据操作
    /// </summary>
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

    private readonly Dictionary<string, NbtBase> Entries = new();

    public NbtCompound()
    {
        NbtType = NbtType.NbtCompound;
    }

    /// <summary>
    /// 尝试获取NBT标签
    /// </summary>
    /// <param name="key">键</param>
    /// <returns>NBT标签</returns>
    public NbtBase? TryGet(string key)
    {
        if (Entries.TryGetValue(key, out var value))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// 添加标签
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="nbt">标签</param>
    public void Add(string key, NbtBase nbt)
    {
        Entries.Add(key, nbt);
    }

    /// <summary>
    /// 删除标签
    /// </summary>
    /// <param name="key">键</param>
    public void Remove(string key)
    {
        Entries.Remove(key);
    }

    /// <summary>
    /// 删除所有标签
    /// </summary>
    public void Clear()
    {
        Entries.Clear();
    }

    /// <summary>
    /// 获取键列表
    /// </summary>
    public List<string> GetKeys()
    {
        return new(Entries.Keys.ToList());
    }

    /// <summary>
    /// 获取值列表
    /// </summary>
    public List<NbtBase> GetValues()
    {
        return new(Entries.Values.ToList());
    }

    /// <summary>
    /// 是否存在键
    /// </summary>
    /// <param name="key">键</param>
    public bool HaveKey(string key)
    {
        return Entries.ContainsKey(key);
    }

    /// <summary>
    /// 修改键名
    /// </summary>
    /// <param name="old">旧名</param>
    /// <param name="now">新名</param>
    public void EditKey(string old, string now)
    {
        if (Entries.Remove(old, out var value))
        {
            Entries.Add(now, value);
        }
    }

    internal override NbtCompound Read(DataInputStream stream)
    {
        byte type;
        while ((type = stream.ReadByte()) != 0)
        {
            string key = stream.ReadString();
            if (type > 12)
            {
                throw new Exception($"type out {key}:{type}");
            }
            var nbt = ById((NbtType)type);
            nbt.Read(stream);
            Entries.Add(key, nbt);
        }

        return this;
    }

    internal override void Write(DataOutputStream stream)
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

    public IEnumerator<KeyValuePair<string, NbtBase>> GetEnumerator()
    {
        return Entries.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Entries.GetEnumerator();
    }
}
