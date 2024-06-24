using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace ColorMC.Core.Net.Motd;

/// <summary>
/// Motd�ͻ���
/// </summary>
public class ProtocolHandler(TcpClient tcp)
{
    public Stopwatch PingWatcher = new();

    public void Receive(byte[] buffer, int start, int offset, SocketFlags f)
    {
        int read = 0;
        int count = 0;
        while (read < offset)
        {
            count++;
            if (count > 20)
            {
                throw new Exception("read fail");
            }
            if (PingWatcher.Elapsed == TimeSpan.Zero)
            {
                PingWatcher.Start();
            }
            read += tcp.Client.Receive(buffer, start + read, offset - read, f);
            if (PingWatcher.IsRunning)
            {
                PingWatcher.Stop();
            }
        }
    }

    /// <summary>
    /// Read some data from a cache of bytes and remove it from the cache
    /// </summary>
    /// <param name="offset">Amount of bytes to read</param>
    /// <param name="cache">Cache of bytes to read from</param>
    /// <returns>The data read from the cache as an array</returns>
    private static byte[] ReadData(int offset, List<byte> cache)
    {
        byte[] result = cache.Take(offset).ToArray();
        cache.RemoveRange(0, offset);
        return result;
    }

    /// <summary>
    /// Read some data directly from the network
    /// </summary>
    /// <param name="offset">Amount of bytes to read</param>
    /// <returns>The data read from the network as an array</returns>
    public byte[] ReadDataRAW(int offset)
    {
        if (offset > 0)
        {
            byte[] cache = new byte[offset];
            Receive(cache, 0, offset, SocketFlags.None);
            return cache;
        }
        return [];
    }

    /// <summary>
    /// Read an integer from the network
    /// </summary>
    /// <returns>The integer</returns>
    public int ReadNextVarIntRAW()
    {
        int i = 0;
        int j = 0;
        byte[] tmp = new byte[1];
        while (true)
        {
            Receive(tmp, 0, 1, SocketFlags.None);
            int k = tmp[0];
            i |= (k & 0x7F) << j++ * 7;
            if (j > 5) throw new OverflowException("VarInt too big");
            if ((k & 0x80) != 128) break;
        }
        return i;
    }

    /// <summary>
    /// Read a single byte from a cache of bytes and remove it from the cache
    /// </summary>
    /// <returns>The byte that was read</returns>
    public static byte ReadNextByte(List<byte> cache)
    {
        byte result = cache[0];
        cache.RemoveAt(0);
        return result;
    }

    /// <summary>
    /// Read an integer from a cache of bytes and remove it from the cache
    /// </summary>
    /// <param name="cache">Cache of bytes to read from</param>
    /// <returns>The integer</returns>
    public static int ReadNextVarInt(List<byte> cache)
    {
        int i = 0;
        int j = 0;
        while (true)
        {
            int k = ReadNextByte(cache);
            i |= (k & 0x7F) << j++ * 7;
            if (j > 5) throw new OverflowException("VarInt too big");
            if ((k & 0x80) != 128) break;
        }
        return i;
    }

    /// <summary>
    /// Read a string from a cache of bytes and remove it from the cache
    /// </summary>
    /// <param name="cache">Cache of bytes to read from</param>
    /// <returns>The string</returns>
    public static string ReadNextString(List<byte> cache)
    {
        int length = ReadNextVarInt(cache);
        if (length > 0)
        {
            return Encoding.UTF8.GetString(ReadData(length, cache));
        }
        else return "";
    }

    /// <summary>
    /// Build an integer for sending over the network
    /// </summary>
    /// <param name="paramInt">Integer to encode</param>
    /// <returns>Byte array for this integer</returns>
    public static byte[] GetVarInt(int paramInt)
    {
        List<byte> bytes = [];
        while ((paramInt & -128) != 0)
        {
            bytes.Add((byte)(paramInt & 127 | 128));
            paramInt = (int)((uint)paramInt >> 7);
        }
        bytes.Add((byte)paramInt);
        return [.. bytes];
    }

    /// <summary>
    /// Easily append several byte arrays
    /// </summary>
    /// <param name="bytes">Bytes to append</param>
    /// <returns>Array containing all the data</returns>
    public static byte[] ConcatBytes(params byte[][] bytes)
    {
        List<byte> result = [];
        foreach (byte[] array in bytes)
            result.AddRange(array);
        return [.. result];
    }
}
