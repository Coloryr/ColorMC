using ColorMC.Core.Helpers;
using ColorMC.Core.Nbt;
using ColorMC.Core.Objs;
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
    /// <param name="data">区块数据</param>
    /// <param name="file">文件名</param>
    public static async Task SaveAsync(this ChunkDataObj data, string file)
    {
        await Task.Run(() =>
        {
            using var stream = new MemoryStream();
            WriteChunk(data, stream);
            WriteHead(data, stream);
            stream.Seek(0, SeekOrigin.Begin);
            PathHelper.WriteBytes(file, stream);
        });
    }

    /// <summary>
    /// 写区块数据到流中
    /// </summary>
    /// <param name="data">区块数据</param>
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
        int time = FunctionUtils.GetTime();

        //写数据
        foreach (var item in data.Nbt.Cast<ChunkNbt>())
        {
            using var stream1 = new MemoryStream();
            item.Save(stream1);
            var data1 = stream1.ToArray();
            int len = data1.Length + 5;
            int pos = ChunkUtils.ChunkToHeadPos(new PointStruct(item.X, item.Z)); //区块坐标
            var chunk = data.Pos[pos];
            chunk.Time = time;
            chunk.Pos = now / 4096;
            chunk.Count = (byte)Math.Ceiling((double)len / 4096); //区块大小
            var array = BitConverter.GetBytes(data1.Length + 1); //区块数据长度
            Array.Reverse(array);
            stream.Write(array); //数据大小
            stream.WriteByte((byte)item.ZipType); //压缩类型
            stream.Write(data1); //数据
            now += len;
            var last = len % 4096;
            //填充区块剩余空间
            if (last == 0)
            {
                continue;
            }

            var size1 = 4096 - last;
            stream.Write(new byte[size1]);
            now += size1;
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
                //区块为空填充0
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
                //区块为空填充0
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
    public static async Task<ChunkDataObj?> ReadAsync(string file)
    {
        return await ReadAsync(PathHelper.OpenRead(file)!);
    }

    /// <summary>
    /// 从流中读区块文件
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns>区块数据</returns>
    private static async Task<ChunkDataObj?> ReadAsync(Stream stream)
    {
        var data = await ReadHeadAsync(stream);
        if (data == null)
        {
            return null;
        }
        await ReadChunkAsync(data, stream);

        return data;
    }

    /// <summary>
    /// 从流中读区块数据
    /// </summary>
    /// <param name="data">区块数据</param>
    /// <param name="stream">要读取的流</param>
    private static async Task ReadChunkAsync(ChunkDataObj data, Stream stream)
    {
        var list = new NbtList
        {
            InNbtType = NbtType.NbtCompound
        };
        var list1 = new ChunkNbt?[data.Pos.Length];
        await Parallel.ForAsync(0, data.Pos.Length, async (a, cancel) =>
        {
            var item = data.Pos[a];
            //区块为空
            if (item.Count == 0)
            {
                return;
            }
            byte[] buffer;
            //获取区块位置
            lock (stream)
            {
                stream.Seek(item.Pos, SeekOrigin.Begin);
                var temp = new byte[5];
                stream.ReadExactly(temp);
                //区块大小
                item.Size = temp[0] << 24 | temp[1] << 16 | temp[2] << 8 | temp[3];
                buffer = new byte[item.Size - 1];
                stream.ReadExactly(buffer);
            }
            using var stream1 = new MemoryStream(buffer);
            //读NBT标签
            var nbt = await NbtBase.ReadAsync(stream1, true, cancel) as ChunkNbt;

            //获取区块位置
            if (nbt!.TryGet("xPos") is NbtInt value)
            {
                nbt.X = value.ValueInt;
            }
            if (nbt.TryGet("zPos") is NbtInt value1)
            {
                nbt.Z = value1.ValueInt;
            }
            if (nbt.TryGet("Position") is NbtIntArray value2)
            {
                nbt.X = value2.ValueIntArray[0];
                nbt.Z = value2.ValueIntArray[1];
            }

            list1[a] = nbt;
        });
        foreach (var item in list1)
        {
            if (item != null)
            {
                list.Add(item);
            }
        }
        data.Nbt = list;
    }

    /// <summary>
    /// 从流中读区块头
    /// </summary>
    /// <param name="stream">要读取的流</param>
    /// <returns>区块数据</returns>
    private static async Task<ChunkDataObj?> ReadHeadAsync(Stream stream)
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

            data.Pos[a] = new ChunkPosStruct(po * 4096, temp[(a * 4) + 3], time, a);
        }

        return data;
    }
}