using System;
using System.IO;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public sealed class Bitstream : IDisposable
{
    /**
     * Maximum size of the frame buffer.
     */
    public const int BUFFER_INT_SIZE = 433;
    /**
     * Synchronization control constant for the initial
     * synchronization to the start of a frame.
     */
    public const byte INITIAL_SYNC = 0;

    // max. 1730 bytes per frame: 144 * 384kbit/s / 32000 Hz + 2 Bytes CRC
    /**
     * Synchronization control constant for non-initial frame
     * synchronizations.
     */
    public const byte STRICT_SYNC = 1;
    /**
     * The frame buffer that holds the data for the current frame.
     */
    private readonly int[] framebuffer = new int[BUFFER_INT_SIZE];
    private readonly int[] bitmask = {0,    // dummy
            0x00000001, 0x00000003, 0x00000007, 0x0000000F,
            0x0000001F, 0x0000003F, 0x0000007F, 0x000000FF,
            0x000001FF, 0x000003FF, 0x000007FF, 0x00000FFF,
            0x00001FFF, 0x00003FFF, 0x00007FFF, 0x0000FFFF,
            0x0001FFFF};
    private readonly Header header = new();
    private readonly byte[] syncbuf = new byte[4];
    /**
     * The bytes read from the stream.
     */
    private readonly byte[] frame_bytes = new byte[BUFFER_INT_SIZE * 4];
    private readonly Crc16[] crc = new Crc16[1];
    //private int 			current_frame_number;
    //private int				last_frame_number;
    private long local = 0;
    /**
     * Number of valid bytes in the frame buffer.
     */
    private int framesize;
    /**
     * Index into <code>framebuffer</code> where the next bits are
     * retrieved.
     */
    private int wordpointer;
    /**
     * Number (0-31, from MSB to LSB) of next bit for get_bits()
     */
    private int bitindex;
    /**
     * The current specified syncword
     */
    private int syncword;

    public byte[] rawid3v2 { get; private set; }

    /**
     *
     */
    private bool single_ch_mode;

    private bool firstframe;
    private readonly Stream stream;
    /**
     * Construct a IBitstream that reads data from a
     * given InputStream.
     */
    public Bitstream(Stream stream)
    {
        this.stream = stream;
        LoadID3v2(stream);
        firstframe = true;

        CloseFrame();
    }

    /**
     * Load ID3v2 frames.
     *
     * @param in MP3 InputStream.
     * @author JavaZOOM
     */
    private void LoadID3v2(Stream input)
    {
        int size;
        // Read ID3v2 header (10 bytes).
        input.Seek(0, SeekOrigin.Begin);
        local += 10;
        size = ReadID3v2Header(input);
        input.Seek(10, SeekOrigin.Begin);

        if (size > 0)
        {
            rawid3v2 = new byte[size];
            input.Read(rawid3v2, 0, rawid3v2.Length);
            local += rawid3v2.Length;
        }
        else
        {
            input.Seek(0, SeekOrigin.Begin);
            local = 0;
        }
    }

    /**
     * Parse ID3v2 tag header to find out size of ID3v2 frames.
     *
     * @param in MP3 InputStream
     * @return size of ID3v2 frames + header
     * @throws IOException
     * @author JavaZOOM
     */
    private static int ReadID3v2Header(Stream input)
    {
        byte[] id3header = new byte[4];
        int size = -10;
        input.Read(id3header, 0, 3);
        // Look for ID3v2
        if ((id3header[0] == 'I') && (id3header[1] == 'D') && (id3header[2] == '3'))
        {
            input.Read(id3header, 0, 3);
            input.Read(id3header, 0, 4);
            size = (id3header[0] << 21) + (id3header[1] << 14) + (id3header[2] << 7) + (id3header[3]);
        }
        return size + 10;
    }

    /**
     * Close the Bitstream.
     *
     * @throws BitstreamException
     */
    public void Dispose()
    {
        stream.Close();
    }

    /**
     * Reads and parses the next frame from the input source.
     *
     * @return the Header describing details of the frame read,
     * or null if the end of the stream has been reached.
     */
    public Header? ReadFrame()
    {
        Header? result = null;
        try
        {
            result = ReadNextFrame();
            // E.B, Parse VBR (if any) first frame.
            if (firstframe)
            {
                result.ParseVBR(frame_bytes);
                firstframe = false;
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

    /**
     * Read next MP3 frame.
     *
     * @return MP3 frame header.
     * @throws Exception
     */
    private Header ReadNextFrame()
    {
        if (framesize == -1)
        {
            NextFrame();
        }
        return header;
    }


    /**
     * Read next MP3 frame.
     *
     * @throws Exception
     */
    private void NextFrame()
    {
        // entire frame is read by the header class.
        header.ReadHeader(this, crc);
    }

    /**
     * Unreads the bytes read from the frame.
     *
     * @throws Exception
     */
    // REVIEW: add new error codes for this.
    public void UnreadFrame()
    {
        if (wordpointer == -1 && bitindex == -1 && (framesize > 0))
        {
            stream.Seek(-framesize, SeekOrigin.Current);
            local -= framesize;
        }
    }

    /**
     * Close MP3 frame.
     */
    public void CloseFrame()
    {
        framesize = -1;
        wordpointer = -1;
        bitindex = -1;
    }

    /**
     * Determines if the next 4 bytes of the stream represent a
     * frame header.
     */
    public bool IsSyncCurrentPosition(int syncmode)
    {
        int read = ReadBytes(syncbuf, 0, 4);
        int headerstring = (int)((syncbuf[0] << 24) & 0xFF000000) | ((syncbuf[1] << 16) & 0x00FF0000) | ((syncbuf[2] << 8) & 0x0000FF00) | ((syncbuf[3]) & 0x000000FF);

        stream.Seek(-read, SeekOrigin.Current);
        local -= read;

        bool sync = false;
        switch (read)
        {
            case 0:
                sync = true;
                break;
            case 4:
                sync = IsSyncMark(headerstring, syncmode, syncword);
                break;
        }

        return sync;
    }

    /**
     * Get next 32 bits from bitstream.
     * They are stored in the headerstring.
     * syncmod allows Synchro flag ID
     * The returned value is False at the end of stream.
     */

    public int syncHeader(byte syncmode)
    {
        bool sync;
        int headerstring;
        // read additional 2 bytes
        int bytesRead = ReadBytes(syncbuf, 0, 3);

        if (bytesRead != 3)
            throw new BitstreamException(BitstreamErrors.STREAM_EOF, null);

        headerstring = ((syncbuf[0] << 16) & 0x00FF0000) | ((syncbuf[1] << 8) & 0x0000FF00) | ((syncbuf[2]) & 0x000000FF);

        do
        {
            headerstring <<= 8;

            if (ReadBytes(syncbuf, 3, 1) != 1)
                throw new BitstreamException(BitstreamErrors.STREAM_EOF, null);

            headerstring |= (syncbuf[3] & 0x000000FF);

            sync = IsSyncMark(headerstring, syncmode, syncword);
        }
        while (!sync);

        return headerstring;
    }

    public bool IsSyncMark(int headerstring, int syncmode, int word)
    {
        bool sync;

        if (syncmode == INITIAL_SYNC)
        {
            sync = ((headerstring & 0xFFE00000) == 0xFFE00000);    // SZD: MPEG 2.5
        }
        else
        {
            var temp = (int)(headerstring & 0xFFF80C00);
            var temp1 = (int)(headerstring & 0x000000C0);
            sync = temp == word && temp1 == 0x000000C0 == single_ch_mode;
        }

        // filter out invalid sample rate
        if (sync)
            sync = (((headerstring >>> 10) & 3) != 3);
        // filter out invalid layer
        if (sync)
            sync = (((headerstring >>> 17) & 3) != 0);
        // filter out invalid version
        if (sync)
            sync = (((headerstring >>> 19) & 3) != 1);

        return sync;
    }

    /**
     * Reads the data for the next frame. The frame is not parsed
     * until parse frame is called.
     */
    public int ReadFrameData(int bytesize)
    {
        int numread = ReadFully(frame_bytes, 0, bytesize);
        framesize = bytesize;
        wordpointer = -1;
        bitindex = -1;
        return numread;
    }

    /**
     * Parses the data previously read with read_frame_data().
     */
    public void ParseFrame()
    {
        // Convert Bytes read to int
        int b = 0;
        byte[] byteread = frame_bytes;
        int bytesize = framesize;

        for (int k = 0; k < bytesize; k = k + 4)
        {
            byte b0;
            byte b1 = 0;
            byte b2 = 0;
            byte b3 = 0;
            b0 = byteread[k];
            if (k + 1 < bytesize) b1 = byteread[k + 1];
            if (k + 2 < bytesize) b2 = byteread[k + 2];
            if (k + 3 < bytesize) b3 = byteread[k + 3];
            framebuffer[b++] = (int)((b0 << 24) & 0xFF000000) | ((b1 << 16) & 0x00FF0000) | ((b2 << 8) & 0x0000FF00) | (b3 & 0x000000FF);
        }
        wordpointer = 0;
        bitindex = 0;
    }

    /**
     * Read bits from buffer into the lower bits of an unsigned int.
     * The LSB contains the latest read bit of the stream.
     * (1 <= number_of_bits <= 16)
     */
    public int GetBits(int number_of_bits)
    {
        int returnvalue;
        int sum = bitindex + number_of_bits;

        // E.B
        // There is a problem here, wordpointer could be -1 ?!
        if (wordpointer < 0) wordpointer = 0;
        // E.B : End.

        if (sum <= 32)
        {
            // all bits contained in *wordpointer
            returnvalue = (framebuffer[wordpointer] >>> (32 - sum)) & bitmask[number_of_bits];
            // returnvalue = (wordpointer[0] >> (32 - sum)) & bitmask[number_of_bits];
            if ((bitindex += number_of_bits) == 32)
            {
                bitindex = 0;
                wordpointer++; // added by me!
            }
            return returnvalue;
        }

        int Right = (framebuffer[wordpointer] & 0x0000FFFF);
        wordpointer++;
        int Left = (int)(framebuffer[wordpointer] & 0xFFFF0000);
        long v = (Right << 16 & 0xFFFF0000) | (long)((Left >>> 16) & 0x0000FFFF);
        returnvalue = (int)v;

        returnvalue >>>= 48 - sum;    // returnvalue >>= 16 - (number_of_bits - (32 - bitindex))
        returnvalue &= bitmask[number_of_bits];
        bitindex = sum - 32;
        return returnvalue;
    }

    /**
     * Set the word we want to sync the header to.
     * In Big-Endian byte order
     */
    public void SetSyncword(int syncword0)
    {
        syncword = (int)(syncword0 & 0xFFFFFF3F);
        single_ch_mode = (syncword0 & 0x000000C0) == 0x000000C0;
    }

    /**
     * Reads the exact number of bytes from the source
     * input stream into a byte array.
     *
     * @param b    The byte array to read the specified number
     *             of bytes into.
     * @param offs The index in the array where the first byte
     *             read should be stored.
     * @param len  the number of bytes to read.
     * @throws BitstreamException is thrown if the specified
     *                            number of bytes could not be read from the stream.
     */
    private int ReadFully(byte[] b, int offs, int len)
    {
        int nRead = 0;
        while (len > 0)
        {
            if (local >= stream.Length)
            {
                return 0;
            }
            int bytesread = stream.Read(b, offs, len);
            local += bytesread;
            if (bytesread == 0)
            {
                while (len-- > 0)
                {
                    b[offs++] = 0;
                }
                break;
            }
            nRead = nRead + bytesread;
            offs += bytesread;
            len -= bytesread;
        }
        return nRead;
    }

    /**
     * Simlar to readFully, but doesn't throw exception when
     * EOF is reached.
     */
    private int ReadBytes(byte[] b, int offs, int len)
    {
        int totalBytesRead = 0;
        while (len > 0)
        {
            if (local >= stream.Length)
            {
                return 0;
            }
            int bytesread = stream.Read(b, offs, len);
            local += bytesread;
            if (bytesread == 0 || local >= stream.Length)
            {
                break;
            }
            totalBytesRead += bytesread;
            offs += bytesread;
            len -= bytesread;
        }
        return totalBytesRead;
    }
}
