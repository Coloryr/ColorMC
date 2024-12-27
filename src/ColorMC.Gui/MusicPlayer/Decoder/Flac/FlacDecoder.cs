using System;

namespace ColorMC.Gui.MusicPlayer.Decoder.Flac;

public class FlacDecoder(FlacStream stream)
{

    private static readonly int[][] Predications = [[], [1], [2, -1], [3, -3, 1], [4, -6, 4, -1]];

    public int BlockSize { get; set; }

    private static int CheckBitDepth(long val, int depth)
    {
        if (val >> (depth - 1) == val >> depth)
        {
            return (int)val;
        }
        else
        {
            throw new ArgumentException(val + " is not a signed " + depth + "-bit value");
        }
    }

    public void DecodeSubframes(int sampleDepth, int chanAsgn, int[][] buffer)
    {
        if (sampleDepth < 1 || sampleDepth > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleDepth));
        }
        if ((chanAsgn >>> 4) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chanAsgn));
        }

        long[] temp0;
        long[] temp1;

        if (0 <= chanAsgn && chanAsgn <= 7)
        {
            int numChannels = chanAsgn + 1;
            for (int ch = 0; ch < numChannels; ch++)
            {
                temp0 = DecodeSubframe(sampleDepth);
                int[] outChan = buffer[ch];
                for (int i = 0; i < BlockSize; i++)
                {
                    outChan[i] = CheckBitDepth(temp0[i], sampleDepth);
                }
            }

        }
        else if (8 <= chanAsgn && chanAsgn <= 10)
        {
            temp0 = DecodeSubframe(sampleDepth + (chanAsgn == 9 ? 1 : 0));
            temp1 = DecodeSubframe(sampleDepth + (chanAsgn == 9 ? 0 : 1));

            if (chanAsgn == 8)
            {
                for (int i = 0; i < BlockSize; i++) temp1[i] = temp0[i] - temp1[i];
            }
            else if (chanAsgn == 9)
            {
                for (int i = 0; i < BlockSize; i++) temp0[i] += temp1[i];
            }
            else if (chanAsgn == 10)
            {
                for (int i = 0; i < BlockSize; i++)
                {
                    long side = temp1[i];
                    long right = temp0[i] - (side >> 1);
                    temp1[i] = right;
                    temp0[i] = right + side;
                }
            }

            int[] outLeft = buffer[0];
            int[] outRight = buffer[1];
            for (int i = 0; i < BlockSize; i++)
            {
                outLeft[+i] = CheckBitDepth(temp0[i], sampleDepth);
                outRight[+i] = CheckBitDepth(temp1[i], sampleDepth);
            }
        }
        else
        {
            throw new Exception("chanAsgn is out or range");
        }
    }

    private long[] DecodeSubframe(int sampleDepth)
    {
        if (sampleDepth < 1 || sampleDepth > 33)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleDepth));
        }

        if (stream.ReadUintWithCrc(1) != 0)
        {
            throw new Exception("Invalid padding bit");
        }

        int type = stream.ReadUintWithCrc(6);
        int shift = stream.ReadUintWithCrc(1);
        if (shift == 1)
        {
            while (stream.ReadUintWithCrc(1) == 0)
            {
                if (shift >= sampleDepth)
                {
                    throw new Exception("Waste-bits-per-sample exceeds sample depth");
                }
                shift++;
            }
        }
        if (0 > shift || shift > sampleDepth)
        {
            throw new Exception("Shift size is out of range");
        }
        sampleDepth -= shift;

        var result = new long[BlockSize];

        if (type == 0)
        {
            Array.Fill(result, 0, BlockSize, stream.ReadShiftInt(sampleDepth));
        }
        else if (type == 1)
        {
            for (int i = 0; i < BlockSize; i++)
            {
                result[i] = stream.ReadShiftInt(sampleDepth);
            }
        }
        else if (8 <= type && type <= 12)
        {
            DecodeFixedPredictionSubframe(type - 8, sampleDepth, result);
        }
        else if (32 <= type && type <= 63)
        {
            DecodeLinearPredictiveCodingSubframe(type - 31, sampleDepth, result);
        }
        else
        {
            throw new Exception("Reserved subframe type");
        }

        if (shift > 0)
        {
            for (int i = 0; i < BlockSize; i++)
            {
                result[i] <<= shift;
            }
        }

        return result;
    }

    private void DecodeLinearPredictiveCodingSubframe(int lpcOrder, int sampleDepth, long[] result)
    {
        if (sampleDepth < 1 || sampleDepth > 33)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleDepth));
        }
        if (lpcOrder < 1 || lpcOrder > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(lpcOrder));
        }
        if (lpcOrder > BlockSize)
        {
            throw new Exception("LPC order exceeds block size");
        }

        for (int i = 0; i < lpcOrder; i++) result[i] = stream.ReadShiftInt(sampleDepth);

        int precision = stream.ReadUintWithCrc(4) + 1;
        if (precision == 16)
        {
            throw new Exception("Invalid LPC precision");
        }
        int shift = stream.ReadShiftInt(5);
        if (shift < 0)
        {
            throw new Exception("Invalid LPC shift");
        }

        int[] coefs = new int[lpcOrder];
        for (int i = 0; i < coefs.Length; i++)
        {
            coefs[i] = stream.ReadShiftInt(precision);
        }

        ReadResiduals(lpcOrder, result);
        RestoreLpc(result, coefs, sampleDepth, shift);
    }

    private void DecodeFixedPredictionSubframe(int predOrder, int sampleDepth, long[] result)
    {
        if (sampleDepth < 1 || sampleDepth > 33)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleDepth));
        }
        if (predOrder < 0 || predOrder >= Predications.Length || predOrder > BlockSize)
        {
            throw new ArgumentOutOfRangeException(nameof(predOrder));
        }

        for (int i = 0; i < predOrder; i++)
        {
            result[i] = stream.ReadShiftInt(sampleDepth);
        }
        ReadResiduals(predOrder, result);
        RestoreLpc(result, Predications[predOrder], sampleDepth, 0);
    }

    private void RestoreLpc(long[] result, int[] coefs, int sampleDepth, int shift)
    {
        if (sampleDepth < 1 || sampleDepth > 33)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleDepth));
        }
        if (shift < 0 || shift > 63)
        {
            throw new ArgumentOutOfRangeException(nameof(shift));
        }
        long lowerBound = (-1) << (sampleDepth - 1);
        long upperBound = -(lowerBound + 1);

        for (int i = coefs.Length; i < BlockSize; i++)
        {
            long sum = 0;
            for (int j = 0; j < coefs.Length; j++)
            {
                sum += result[i - 1 - j] * coefs[j];
            }
            if ((sum >> 53) != 0 && (sum >> 53) != -1)
            {
                throw new Exception("Count coefs is bad");
            }
            sum = result[i] + (sum >> shift);
            if (sum < lowerBound || sum > upperBound)
            {
                throw new Exception("Post-LPC result exceeds bit depth");
            }
            result[i] = sum;
        }
    }

    private void ReadResiduals(int warmup, long[] result)
    {
        if (warmup < 0 || warmup > BlockSize)
        {
            throw new ArgumentOutOfRangeException(nameof(warmup));
        }

        int method = stream.ReadUintWithCrc(2);
        if (method >= 2 || method < 0)
        {
            throw new Exception("Reserved residual coding method");
        }
        int paramBits = method == 0 ? 4 : 5;
        int escapeParam = method == 0 ? 0xF : 0x1F;

        int partitionOrder = stream.ReadUintWithCrc(4);
        int numPartitions = 1 << partitionOrder;
        if (BlockSize % numPartitions != 0)
        {
            throw new Exception("Block size not divisible by number of Rice partitions");
        }

        for (int inc = BlockSize >>> partitionOrder, partEnd = inc, resultIndex = warmup;
            partEnd <= BlockSize; partEnd += inc)
        {
            int param = stream.ReadUintWithCrc(paramBits);
            if (param == escapeParam)
            {
                int numBits = stream.ReadUintWithCrc(5);
                for (; resultIndex < partEnd; resultIndex++) result[resultIndex] = stream.ReadShiftInt(numBits);
            }
            else
            {
                stream.ReadRiceSignedInts(param, result, resultIndex, partEnd);
                resultIndex = partEnd;
            }
        }
    }
}
