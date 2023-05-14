using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Nbt;

public class FixedBuffer : Stream, IDisposable
{
    public override bool CanRead => baseStream.CanRead;

    public override bool CanSeek => baseStream.CanSeek;

    public override bool CanWrite => baseStream.CanWrite;

    public override long Length => baseStream.Length;

    public override long Position 
    {
        get => baseStream.Position; 
        set => baseStream.Position = value; 
    }

    private readonly Stream baseStream;

    private readonly byte[] buffer;
    private int end;
    private int start;

    public FixedBuffer(Stream stream, int size = 8192)
    {
        baseStream = stream;
        buffer = new byte[size];
    }

    public override void Flush()
    {
        baseStream.Flush();
    }

    public int Read()
    {
        if (start >= end)
        {
            Fill();
            if (start >= end)
            {
                return -1;
            }
        }

        return Convert.ToInt32(buffer[start++]);
    }

    public override int Read(byte[] buf, int offset, int length)
    {
        int i = GetAvailableBuffer();
        if (i <= 0)
        {
            if (length >= buffer.Length)
            {
                return  baseStream.Read(buf, offset, length);
            }

            Fill();
            i = GetAvailableBuffer();
            if (i <= 0)
            {
                return -1;
            }
        }

        if (length > i)
        {
            length = i;
        }

        Array.Copy(buffer, start, buf, offset, length);
        start += length;
        return length;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return baseStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        baseStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        baseStream.Write(buffer, offset, count);
    }

    public long Skip(long n)
    {
        if (n <= 0L)
        {
            return 0L;
        }
        else
        {
            long l = GetAvailableBuffer();
            if (l <= 0L)
            {
                return baseStream.Seek(n, SeekOrigin.Current);
            }
            else
            {
                if (n > l)
                {
                    n = l;
                }

                start = (int)(start + n);
                return n;
            }
        }
    }

    public new void Dispose()
    {
        baseStream.Dispose();
        base.Dispose();

        GC.SuppressFinalize(this);
    }

    private int GetAvailableBuffer()
    {
        return end - start;
    }

    private void Fill()
    {
        end = 0;
        start = 0;
        int i = baseStream.Read(buffer, 0, buffer.Length);
        if (i > 0) {
            end = i;
        }
    }
}
