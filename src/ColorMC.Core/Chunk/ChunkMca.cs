using ColorMC.Core.Helpers;
using ColorMC.Core.Nbt;
using ColorMC.Core.Objs.Chunk;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Chunk;

/// <summary>
/// 区块Mca文件处理
/// </summary>
public static class ChunkMca
{
    /// <summary>
    /// 保存区块文件
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="file">文件</param>
    public static void Save(this ChunkDataObj data, string file)
    {
        using var stream = new MemoryStream();
        WriteChunk(data, stream);
        WriteHead(data, stream);
        using var stream1 = PathHelper.OpenWrite(file);
        stream.Seek(0, SeekOrigin.Begin);
        stream.CopyTo(stream1);
    }

    /// <summary>
    /// 写区块数据到流中
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="stream">保存用的流</param>
    private static void WriteChunk(ChunkDataObj data, Stream stream)
    {
        if (data.Nbt.Count == 0)
        {
            return;
        }
        //跳过文件头
        stream.Seek(8192, SeekOrigin.Begin);
        int now = 8192;
        int time = FuntionUtils.GetTime();
        foreach (var item in data.Pos)
        {
            if (item == null)
            {
                continue;
            }

            item.Pos = 0;
            item.Count = 0;
            item.Size = 0;
            item.Time = 0;
        }

        //写数据
        foreach (ChunkNbt item in data.Nbt.Cast<ChunkNbt>())
        {
            using var stream1 = new MemoryStream();
            item.Save(stream1);
            var data1 = stream1.ToArray();
            int len = data1.Length + 5;
            int pos = ChunkUtils.ChunkToHeadPos(item.X, item.Z);
            var temp = data.Pos[pos];
            temp.Time = time;
            temp.Pos = now / 4096;
            temp.Count = (byte)Math.Ceiling((double)len / 4096); //区块大小
            var array = BitConverter.GetBytes(data1.Length + 1);
            Array.Reverse(array);
            stream.Write(array); //数据大小
            stream.WriteByte(2); //压缩类型
            stream.Write(data1); //数据
            now += len;
            var last = len % 4096;
            //填充区块剩余空间
            if (last != 0)
            {
                var size1 = 4096 - last;
                stream.Write(new byte[size1]);
                now += size1;
            }
        }
    }

    /// <summary>
    /// 写区块头到流中
    /// </summary>
    /// <param name="data">区块数据</param>
    /// <param name="stream">保存用的流</param>
    private static void WriteHead(ChunkDataObj data, Stream stream)
    {
        //回到文件头
        stream.Seek(0, SeekOrigin.Begin);
        foreach (var item in data.Pos)
        {
            if (item.Count == 0)
            {
                stream.Write(new byte[4]);
            }
            else
            {
                //区块位置
                var array = BitConverter.GetBytes(item.Pos);
                var array1 = new byte[4];
                array1[0] = array[2];
                array1[1] = array[1];
                array1[2] = array[0];
                array1[3] = item.Count;
                stream.Write(array1);
            }
        }
        foreach (var item in data.Pos)
        {
            if (item.Count == 0)
            {
                stream.Write(new byte[4]);
            }
            else
            {
                //区块编辑时间
                var array = BitConverter.GetBytes(item.Time);
                Array.Reverse(array);
                stream.Write(array);
            }
        }
    }

    /// <summary>
    /// 读区块数据
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>区块数据</returns>
    public static async Task<ChunkDataObj?> Read(string file)
    {
        return await Read(PathHelper.OpenRead(file)!);
    }

    /// <summary>
    /// 从流中读区块文件
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns>区块数据</returns>
    private static async Task<ChunkDataObj?> Read(Stream stream)
    {
        var data = await ReadHead(stream);
        if (data == null)
        {
            return null;
        }
        await ReadChunk(data, stream);

        return data;
    }

    /// <summary>
    /// 从流中读区块数据
    /// </summary>
    /// <param name="data">区块数据</param>
    /// <param name="stream">流</param>
    private static async Task ReadChunk(ChunkDataObj data, Stream stream)
    {
        var list = new NbtList()
        {
            InNbtType = NbtType.NbtCompound
        };
        var temp = new byte[5];
        foreach (var item in data.Pos)
        {
            if (item == null || item.Count == 0)
            {
                continue;
            }
            //获取区块位置
            stream.Seek(item.Pos, SeekOrigin.Begin);
            stream.ReadExactly(temp);
            //区块大小
            item.Size = temp[0] << 24 | temp[1] << 16
                | temp[2] << 8 | temp[3];
            byte type = temp[4];
            var buffer = new byte[item.Size - 1];
            await stream.ReadExactlyAsync(buffer);
            using var stream1 = new MemoryStream(buffer);
            //读NBT标签
            var nbt = (await NbtBase.Read(stream1, true) as ChunkNbt)!;

            //获取区块位置
            if (nbt.TryGet("xPos") is NbtInt value)
            {
                nbt.X = value.Value;
            }
            if (nbt.TryGet("zPos") is NbtInt value1)
            {
                nbt.Z = value1.Value;
            }
            if (nbt.TryGet("Position") is NbtIntArray value2)
            {
                nbt.X = value2.Value[0];
                nbt.Z = value2.Value[1];
            }

            list.Add(nbt);
        }
        data.Nbt = list;
    }

    /// <summary>
    /// 从流中读区块头
    /// </summary>
    /// <param name="stream">文件流</param>
    /// <returns>区块数据</returns>
    private static async Task<ChunkDataObj?> ReadHead(Stream stream)
    {
        //文件过小
        if (stream.Length < 8192)
        {
            return null;
        }
        var temp = new byte[8192];
        await stream.ReadExactlyAsync(temp);

        var data = new ChunkDataObj();

        //获取区块坐标
        for (int a = 0; a < 1024; a++)
        {
            int po = temp[a * 4] << 16 | temp[(a * 4) + 1] << 8
                | temp[(a * 4) + 2];
            int time = temp[(a * 4) + 4096] << 24 | temp[(a * 4) + 4097] << 16
                | temp[(a * 4) + 4098] << 8 | temp[(a * 4) + 4099];

            data.Pos[a] = new ChunkPosObj(po * 4096, temp[(a * 4) + 3], time, a);
        }

        return data;
    }
}