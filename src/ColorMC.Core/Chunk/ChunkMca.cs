using ColorMC.Core.Downloader;
using ColorMC.Core.Nbt;
using ColorMC.Core.Utils;
using System;
using System.Buffers;
using System.IO.Compression;

namespace ColorMC.Core.Chunk;

public static class ChunkMca
{
    public static void Write(ChunkData data, string file)
    {
        using var stream = new MemoryStream();
        WriteChunk(data, stream);
        WriteHead(data, stream);
        File.WriteAllBytes(file, stream.ToArray());
    }

    public static void WriteChunk(ChunkData data, Stream stream)
    {
        stream.Seek(8192, SeekOrigin.Begin);
        int now = 8192;
        int time = Funtcions.GetTime();
        foreach (NbtCompound item in data.Nbt.Cast<NbtCompound>())
        {
            int x = (item.TryGet("xPos") as NbtInt)!.Value;
            int z = (item.TryGet("zPos") as NbtInt)!.Value;
            using var stream1 = new MemoryStream();
            item.Save(stream1);
            int pos = ChunkUtils.ChunkPosToHeadPos(x, z);
            var temp = (data.Pos[pos] ??= new ChunkPos(0, 0, 0));
            var data1 = stream1.ToArray();
            int len = data1.Length + 5;
            temp.Time = time;
            temp.Pos = now / 4096;
            temp.Count = (byte)Math.Ceiling((double)len / 4096);
            var array = BitConverter.GetBytes(data1.Length + 1);
            Array.Reverse(array);
            stream.Write(array);
            stream.WriteByte(2);
            stream.Write(data1);
            now += len;
            var last = len % 4096;
            if (last != 0)
            {
                var size1 = 4096 - last;
                stream.Write(new byte[size1]);
                now += size1;
            }
        }
    }

    public static void WriteHead(ChunkData data, Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        foreach (var item in data.Pos)
        {
            if (item.Count == 0)
            {
                stream.Write(new byte[4]);
            }
            else
            {
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
                var array = BitConverter.GetBytes(item.Time);
                Array.Reverse(array);
                stream.Write(array);
            }
        }
    }

    public static async Task<ChunkData> Read(string file)
    {
        return await Read(File.Open(file, FileMode.Open,
            FileAccess.Read, FileShare.ReadWrite));
    }

    public static async Task<ChunkData> Read(Stream stream)
    {
        var data = new ChunkData();
        await ReadHead(data, stream);
        await ReadChunk(data, stream);

        return data;
    }

    private static async Task ReadChunk(ChunkData data, Stream stream)
    {
        var list = new NbtList()
        {
             InNbtType= NbtType.NbtCompound
        };
        var temp = new byte[5];
        foreach (var item in data.Pos)
        {
            if (item.Count == 0)
                continue;
            stream.Seek(item.Pos, SeekOrigin.Begin);
            stream.ReadExactly(temp);
            int size = temp[0] << 24 | temp[1] << 16
                | temp[2] << 8 | temp[3];
            byte type = temp[4];
            var buffer = new byte[item.Count * 4096];
            await stream.ReadExactlyAsync(buffer);
            using var stream1 = new MemoryStream(buffer);
            list.Add((await NbtBase.Read(stream1) as NbtCompound)!);
        }
        data.Nbt = list;
    }

    private static async Task ReadHead(ChunkData data, Stream stream)
    {
        byte[] temp = new byte[8192];
        await stream.ReadExactlyAsync(temp);

        for (int a = 0; a < 1024; a++)
        {
            int po = temp[a * 4] << 16 | temp[(a * 4) + 1] << 8
                | temp[(a * 4) + 2];

            data.Pos[a] = new ChunkPos(po * 4096, temp[(a * 4) + 3],
                temp[(a * 4) + 4096] << 24 | temp[(a * 4) + 4097] << 16
                | temp[(a * 4) + 4098] << 8 | temp[(a * 4) + 4099]);
        }
    }
}