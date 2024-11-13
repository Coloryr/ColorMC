using System;
using System.IO;

namespace ColorMC.Gui.MusicPlayer.Decoder.Mp3;

public sealed class Bitstream : IDisposable
{
    /// <summary>
    /// Maximum size of the frame buffer.
    /// </summary>
    public const int BUFFER_INT_SIZE = 433;
    /// <summary>
    /// Synchronization control constant for the initial
    /// synchronization to the start of a frame.
    /// </summary>
    public const byte INITIAL_SYNC = 0;

    // max. 1730 bytes per frame: 144 * 384kbit/s / 32000 Hz + 2 Bytes CRC

    /// <summary>
    /// Synchronization control constant for non-initial frame
    /// synchronizations.
    /// </summary>
    public const byte STRICT_SYNC = 1;

    /// <summary>
    /// The frame buffer that holds the data for the current frame.
    /// </summary>
    private readonly int[] _framebuffer = new int[BUFFER_INT_SIZE];

    private readonly int[] _bitmask =
    [
        0,    // dummy
        0x00000001, 0x00000003, 0x00000007, 0x0000000F,
        0x0000001F, 0x0000003F, 0x0000007F, 0x000000FF,
        0x000001FF, 0x000003FF, 0x000007FF, 0x00000FFF,
        0x00001FFF, 0x00003FFF, 0x00007FFF, 0x0000FFFF,
        0x0001FFFF
    ];
    private readonly Header _header = new();
    private readonly byte[] _syncbuf = new byte[4];
    /// <summary>
    /// The bytes read from the stream.
    /// </summary>
    private readonly byte[] _frame_bytes = new byte[BUFFER_INT_SIZE * 4];
    private readonly Crc16[] _crc = new Crc16[1];
    //private int 			current_frame_number;
    //private int				last_frame_number;
    private long _local = 0;
    /// <summary>
    /// Number of valid bytes in the frame buffer.
    /// </summary>
    private int _framesize;
    /// <summary>
    /// Index into <see cref="_framesize"/> where the next bits are
    /// retrieved.
    /// </summary>
    private int _wordpointer;
    /// <summary>
    /// Number (0-31, from MSB to LSB) of next bit for <see cref="GetBits(int)"/>
    /// </summary>
    private int _bitindex;
    /// <summary>
    /// The current specified syncword
    /// </summary>
    private int _syncword;

    public byte[] Rawid3v2 { get; private set; }

    
    private bool _single_ch_mode;

    private bool _firstframe;

    private readonly Stream _stream;
    /// <summary>
    /// Construct a IBitstream that reads data from a
    /// given InputStream.
    /// </summary>
    /// <param name="stream"></param>
    public Bitstream(Stream stream)
    {
        _stream = stream;
        LoadID3v2(stream);
        _firstframe = true;

        CloseFrame();
    }

    /// <summary>
    /// Load ID3v2 frames.
    /// </summary>
    /// <param name="input">MP3 InputStream.</param>
    private void LoadID3v2(Stream input)
    {
        int size;
        // Read ID3v2 header (10 bytes).
        input.Seek(0, SeekOrigin.Begin);
        _local += 10;
        size = ReadID3v2Header(input);
        input.Seek(10, SeekOrigin.Begin);

        if (size > 0)
        {
            Rawid3v2 = new byte[size];
            input.ReadExactly(Rawid3v2);
            _local += Rawid3v2.Length;
        }
        else
        {
            input.Seek(0, SeekOrigin.Begin);
            _local = 0;
        }
    }

    /// <summary>
    /// Parse ID3v2 tag header to find out size of ID3v2 frames.
    /// </summary>
    /// <param name="input">MP3 InputStream</param>
    /// <returns>size of ID3v2 frames + header</returns>
    private static int ReadID3v2Header(Stream input)
    {
        byte[] id3header = new byte[4];
        int size = -10;
        input.ReadExactly(id3header, 0, 3);
        // Look for ID3v2
        if (id3header[0] == 'I' && id3header[1] == 'D' && id3header[2] == '3')
        {
            input.ReadExactly(id3header, 0, 3);
            input.ReadExactly(id3header, 0, 4);
            size = (id3header[0] << 21) + (id3header[1] << 14) + (id3header[2] << 7) + id3header[3];
        }
        return size + 10;
    }

    /// <summary>
    /// Close the Bitstream.
    /// </summary>
    public void Dispose()
    {
        _stream.Dispose();
    }

    /// <summary>
    /// Reads and parses the next frame from the input source.
    /// </summary>
    /// <returns>the Header describing details of the frame read, 
    /// or null if the end of the stream has been reached.</returns>
    public Header? ReadFrame()
    {
        Header? result = null;
        try
        {
            result = ReadNextFrame();
            // E.B, Parse VBR (if any) first frame.
            if (_firstframe)
            {
                result.ParseVBR(_frame_bytes);
                _firstframe = false;
            }
        }
        catch (BitstreamException ex)
        {
            if (ex.GetErrorCode() == BitstreamErrors.INVALIDFRAME)
            {
                // Try to skip this frame.
                try
                {
                    CloseFrame();
                    result = ReadNextFrame();
                }
                catch
                {

                }
            }
        }
        return result;
    }

    /// <summary>
    /// Read next MP3 frame.
    /// </summary>
    /// <returns>MP3 frame header.</returns>
    private Header ReadNextFrame()
    {
        if (_framesize == -1)
        {
            NextFrame();
        }
        return _header;
    }

    /// <summary>
    /// Read next MP3 frame.
    /// </summary>
    private void NextFrame()
    {
        // entire frame is read by the header class.
        _header.ReadHeader(this, _crc);
    }

    /// <summary>
    /// Unreads the bytes read from the frame.
    /// </summary>
    public void UnreadFrame()
    {
        if (_wordpointer == -1 && _bitindex == -1 && _framesize > 0)
        {
            _stream.Seek(-_framesize, SeekOrigin.Current);
            _local -= _framesize;
        }
    }

    /// <summary>
    /// Close MP3 frame.
    /// </summary>
    public void CloseFrame()
    {
        _framesize = -1;
        _wordpointer = -1;
        _bitindex = -1;
    }

    /// <summary>
    /// Determines if the next 4 bytes of the stream represent a
    /// frame header.
    /// </summary>
    /// <param name="syncmode"></param>
    /// <returns></returns>
    public bool IsSyncCurrentPosition(int syncmode)
    {
        int read = ReadBytes(_syncbuf, 0, 4);
        int headerstring = (int)(_syncbuf[0] << 24 & 0xFF000000) | _syncbuf[1] << 16 & 0x00FF0000 | _syncbuf[2] << 8 & 0x0000FF00 | _syncbuf[3] & 0x000000FF;

        _stream.Seek(-read, SeekOrigin.Current);
        _local -= read;

        bool sync = false;
        switch (read)
        {
            case 0:
                sync = true;
                break;
            case 4:
                sync = IsSyncMark(headerstring, syncmode, _syncword);
                break;
        }

        return sync;
    }

    /// <summary>
    /// Get next 32 bits from bitstream.
    /// They are stored in the headerstring.
    /// syncmod allows Synchro flag ID
    /// The returned value is False at the end of stream.
    /// </summary>
    /// <param name="syncmode"></param>
    /// <returns></returns>
    /// <exception cref="BitstreamException"></exception>
    public int SyncHeader(byte syncmode)
    {
        bool sync;
        int headerstring;
        // read additional 2 bytes
        int bytesRead = ReadBytes(_syncbuf, 0, 3);

        if (bytesRead != 3)
            throw new BitstreamException(BitstreamErrors.STREAM_EOF, null);

        headerstring = _syncbuf[0] << 16 & 0x00FF0000 | _syncbuf[1] << 8 & 0x0000FF00 | _syncbuf[2] & 0x000000FF;

        do
        {
            headerstring <<= 8;

            if (ReadBytes(_syncbuf, 3, 1) != 1)
                throw new BitstreamException(BitstreamErrors.STREAM_EOF, null);

            headerstring |= _syncbuf[3] & 0x000000FF;

            sync = IsSyncMark(headerstring, syncmode, _syncword);
        }
        while (!sync);

        return headerstring;
    }

    public bool IsSyncMark(int headerstring, int syncmode, int word)
    {
        bool sync;

        if (syncmode == INITIAL_SYNC)
        {
            sync = (headerstring & 0xFFE00000) == 0xFFE00000;    // SZD: MPEG 2.5
        }
        else
        {
            var temp = (int)(headerstring & 0xFFF80C00);
            var temp1 = headerstring & 0x000000C0;
            sync = temp == word && temp1 == 0x000000C0 == _single_ch_mode;
        }

        // filter out invalid sample rate
        if (sync)
            sync = ((headerstring >>> 10) & 3) != 3;
        // filter out invalid layer
        if (sync)
            sync = ((headerstring >>> 17) & 3) != 0;
        // filter out invalid version
        if (sync)
            sync = ((headerstring >>> 19) & 3) != 1;

        return sync;
    }

    /// <summary>
    /// Reads the data for the next frame. The frame is not parsed
    /// until parse frame is called.
    /// </summary>
    /// <param name="bytesize"></param>
    /// <returns></returns>
    public int ReadFrameData(int bytesize)
    {
        int numread = ReadFully(_frame_bytes, 0, bytesize);
        _framesize = bytesize;
        _wordpointer = -1;
        _bitindex = -1;
        return numread;
    }

    /// <summary>
    /// Parses the data previously read with <see cref="ReadFrameData(int)">.
    /// </summary>
    public void ParseFrame()
    {
        // Convert Bytes read to int
        int b = 0;
        byte[] byteread = _frame_bytes;
        int bytesize = _framesize;

        for (int k = 0; k < bytesize; k += 4)
        {
            byte b0;
            byte b1 = 0;
            byte b2 = 0;
            byte b3 = 0;
            b0 = byteread[k];
            if (k + 1 < bytesize) b1 = byteread[k + 1];
            if (k + 2 < bytesize) b2 = byteread[k + 2];
            if (k + 3 < bytesize) b3 = byteread[k + 3];
            _framebuffer[b++] = (int)(b0 << 24 & 0xFF000000) | b1 << 16 & 0x00FF0000 | b2 << 8 & 0x0000FF00 | b3 & 0x000000FF;
        }
        _wordpointer = 0;
        _bitindex = 0;
    }

    /// <summary>
    /// Read bits from buffer into the lower bits of an unsigned int.
    /// The LSB contains the latest read bit of the stream.
    /// (1 &lt;= number_of_bits &lt;= 16)
    /// </summary>
    /// <param name="number_of_bits"></param>
    /// <returns></returns>
    public int GetBits(int number_of_bits)
    {
        int returnvalue;
        int sum = _bitindex + number_of_bits;

        // E.B
        // There is a problem here, wordpointer could be -1 ?!
        if (_wordpointer < 0) _wordpointer = 0;
        // E.B : End.

        if (sum <= 32)
        {
            // all bits contained in *wordpointer
            returnvalue = (_framebuffer[_wordpointer] >>> (32 - sum)) & _bitmask[number_of_bits];
            // returnvalue = (wordpointer[0] >> (32 - sum)) & bitmask[number_of_bits];
            if ((_bitindex += number_of_bits) == 32)
            {
                _bitindex = 0;
                _wordpointer++; // added by me!
            }
            return returnvalue;
        }

        int Right = _framebuffer[_wordpointer] & 0x0000FFFF;
        _wordpointer++;
        int Left = (int)(_framebuffer[_wordpointer] & 0xFFFF0000);
        long v = Right << 16 & 0xFFFF0000 | (long)((Left >>> 16) & 0x0000FFFF);
        returnvalue = (int)v;

        returnvalue >>>= 48 - sum;    // returnvalue >>= 16 - (number_of_bits - (32 - bitindex))
        returnvalue &= _bitmask[number_of_bits];
        _bitindex = sum - 32;
        return returnvalue;
    }

    /// <summary>
    /// Set the word we want to sync the header to.
    /// In Big-Endian byte order
    /// </summary>
    /// <param name="syncword0"></param>
    public void SetSyncword(int syncword0)
    {
        _syncword = (int)(syncword0 & 0xFFFFFF3F);
        _single_ch_mode = (syncword0 & 0x000000C0) == 0x000000C0;
    }

    /// <summary>
    /// Reads the exact number of bytes from the source
    /// input stream into a byte array.
    /// </summary>
    /// <param name="b">The byte array to read the specified number of bytes into.</param>
    /// <param name="offs">The index in the array where the first byte read should be stored.</param>
    /// <param name="len">the number of bytes to read.</param>
    /// <returns></returns>
    private int ReadFully(byte[] b, int offs, int len)
    {
        int nRead = 0;
        while (len > 0)
        {
            if (_local >= _stream.Length)
            {
                return 0;
            }
            int bytesread = _stream.Read(b, offs, len);
            _local += bytesread;
            if (bytesread == 0)
            {
                while (len-- > 0)
                {
                    b[offs++] = 0;
                }
                break;
            }
            nRead += bytesread;
            offs += bytesread;
            len -= bytesread;
        }
        return nRead;
    }

    /// <summary>
    /// Simlar to readFully, but doesn't throw exception when
    /// EOF is reached.
    /// </summary>
    /// <param name="b"></param>
    /// <param name="offs"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    private int ReadBytes(byte[] b, int offs, int len)
    {
        int totalBytesRead = 0;
        while (len > 0)
        {
            if (_local >= _stream.Length)
            {
                return 0;
            }
            int bytesread = _stream.Read(b, offs, len);
            _local += bytesread;
            if (bytesread == 0 || _local >= _stream.Length)
            {
                break;
            }
            totalBytesRead += bytesread;
            offs += bytesread;
            len -= bytesread;
        }
        return totalBytesRead;
    }

    public void Reset()
    {
        _stream.Seek(0, SeekOrigin.Begin);
    }
}
