using System;
using System.IO;
using System.Reflection;

namespace ColorMC.Gui.MusicPlayer.Decoder.Mp3;

public class SynthesisFilter
{
    private const double MY_PI = 3.14159265358979323846;
    private static readonly float cos1_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI / 64.0)));
    private static readonly float cos3_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 3.0 / 64.0)));
    private static readonly float cos5_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 5.0 / 64.0)));
    private static readonly float cos7_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 7.0 / 64.0)));
    private static readonly float cos9_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 9.0 / 64.0)));
    private static readonly float cos11_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 11.0 / 64.0)));
    private static readonly float cos13_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 13.0 / 64.0)));
    private static readonly float cos15_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 15.0 / 64.0)));
    private static readonly float cos17_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 17.0 / 64.0)));
    private static readonly float cos19_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 19.0 / 64.0)));
    private static readonly float cos21_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 21.0 / 64.0)));
    private static readonly float cos23_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 23.0 / 64.0)));
    private static readonly float cos25_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 25.0 / 64.0)));
    private static readonly float cos27_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 27.0 / 64.0)));
    private static readonly float cos29_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 29.0 / 64.0)));
    private static readonly float cos31_64 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 31.0 / 64.0)));
    private static readonly float cos1_32 = (float)(1.0 / (2.0 * Math.Cos(MY_PI / 32.0)));
    private static readonly float cos3_32 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 3.0 / 32.0)));
    private static readonly float cos5_32 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 5.0 / 32.0)));
    private static readonly float cos7_32 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 7.0 / 32.0)));
    private static readonly float cos9_32 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 9.0 / 32.0)));
    private static readonly float cos11_32 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 11.0 / 32.0)));
    private static readonly float cos13_32 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 13.0 / 32.0)));
    private static readonly float cos15_32 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 15.0 / 32.0)));
    private static readonly float cos1_16 = (float)(1.0 / (2.0 * Math.Cos(MY_PI / 16.0)));
    private static readonly float cos3_16 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 3.0 / 16.0)));
    private static readonly float cos5_16 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 5.0 / 16.0)));
    private static readonly float cos7_16 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 7.0 / 16.0)));
    private static readonly float cos1_8 = (float)(1.0 / (2.0 * Math.Cos(MY_PI / 8.0)));
    private static readonly float cos3_8 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 3.0 / 8.0)));
    private static readonly float cos1_4 = (float)(1.0 / (2.0 * Math.Cos(MY_PI / 4.0)));
    /// <summary>
    /// d[] split into subarrays of length 16. This provides for
    /// more faster access by allowing a block of 16 to be addressed
    /// with static readonlyant offset.
    /// </summary>
    private static float[] d;
    private static float[][] d16;
    private readonly float[] v1;
    private readonly float[] v2;
    private readonly float[] samples;            // 32 new subband samples
    private readonly int channel;
    private readonly float scalefactor;
    /// <summary>
    /// Compute PCM Samples.
    /// </summary>
    private readonly float[] _tmpOut = new float[32];
    private float[] _actualV;            // v1 or v2
    private int _actualWritePos;    // 0-15

    /// <summary>
    /// The scalefactor scales the calculated float pcm samples to short values
    /// (raw pcm samples are in [-1.0, 1.0], if no violations occur).
    /// </summary>
    /// <param name="channelnumber"></param>
    /// <param name="factor"></param>
    public SynthesisFilter(int channelnumber, float factor)
    {
        if (d == null)
        {
            d = Load();
            d16 = SplitArray(d);
        }

        v1 = new float[512];
        v2 = new float[512];
        samples = new float[32];
        channel = channelnumber;
        scalefactor = factor;

        Reset();
    }

    /// <summary>
    /// Loads the data for the d[] from the resource SFd.ser.
    /// </summary>
    /// <returns>the loaded values for d[].</returns>
    private static float[] Load()
    {
        string local = $"ColorMC.Gui.MusicPlayer.Decoder.Mp3.sfd.temp";

        var assm = Assembly.GetExecutingAssembly();
        using var istr = assm.GetManifestResourceStream(local)!;
        var stream = new MemoryStream();
        istr.CopyTo(stream);
        var values = stream.ToArray();

        var result = new float[values.Length / 4];
        Buffer.BlockCopy(values, 0, result, 0, values.Length);
        return result;
    }

    /// <summary>
    /// Converts a 1D array into a number of smaller arrays. This is used
    /// to achieve offset + static readonlyant indexing into an array. Each sub-array
    /// represents a block of values of the original array.
    /// </summary>
    /// <param name="array">The array to split up into blocks.</param>
    /// <returns>An array of arrays in which each element in the returned array will be of length blockSize.</returns>
    private static float[][] SplitArray(float[] array)
    {
        int size = array.Length / 16;
        float[][] split = new float[size][];
        for (int i = 0; i < size; i++)
        {
            split[i] = SubArray(array, i * 16, 16);
        }
        return split;
    }

    /// <summary>
    /// Returns a subarray of an existing array.
    /// </summary>
    /// <param name="array">The array to retrieve a subarra from.</param>
    /// <param name="offs">The offset in the array that corresponds to the first index of the subarray.</param>
    /// <param name="len">The number of indeces in the subarray.</param>
    /// <returns>The subarray, which may be of length 0.</returns>
    static private float[] SubArray(float[] array, int offs, int len)
    {
        if (offs + len > array.Length)
        {
            len = array.Length - offs;
        }

        if (len < 0)
            len = 0;

        float[] subarray = new float[len];
        Array.Copy(array, offs, subarray, 0, len);

        return subarray;
    }

    /// <summary>
    /// Reset the synthesis filter.
    /// </summary>
    public void Reset()
    {
        for (int p = 0; p < 512; p++)
            v1[p] = v2[p] = 0.0f;

        for (int p2 = 0; p2 < 32; p2++)
            samples[p2] = 0.0f;

        _actualV = v1;
        _actualWritePos = 15;
    }

    public void InputSample(float sample, int subbandnumber)
    {
        samples[subbandnumber] = sample;
    }

    public void InputSamples(float[] s)
    {
        for (int i = 31; i >= 0; i--)
        {
            samples[i] = s[i];
        }
    }

    /// <summary>
    /// Compute new values via a fast cosine transform.
    /// </summary>
    private void ComputeNewV()
    {
        var newValue = new float[32];

        float[] s = samples;

        float s0 = s[0];
        float s1 = s[1];
        float s2 = s[2];
        float s3 = s[3];
        float s4 = s[4];
        float s5 = s[5];
        float s6 = s[6];
        float s7 = s[7];
        float s8 = s[8];
        float s9 = s[9];
        float s10 = s[10];
        float s11 = s[11];
        float s12 = s[12];
        float s13 = s[13];
        float s14 = s[14];
        float s15 = s[15];
        float s16 = s[16];
        float s17 = s[17];
        float s18 = s[18];
        float s19 = s[19];
        float s20 = s[20];
        float s21 = s[21];
        float s22 = s[22];
        float s23 = s[23];
        float s24 = s[24];
        float s25 = s[25];
        float s26 = s[26];
        float s27 = s[27];
        float s28 = s[28];
        float s29 = s[29];
        float s30 = s[30];
        float s31 = s[31];

        float p0 = s0 + s31;
        float p1 = s1 + s30;
        float p2 = s2 + s29;
        float p3 = s3 + s28;
        float p4 = s4 + s27;
        float p5 = s5 + s26;
        float p6 = s6 + s25;
        float p7 = s7 + s24;
        float p8 = s8 + s23;
        float p9 = s9 + s22;
        float p10 = s10 + s21;
        float p11 = s11 + s20;
        float p12 = s12 + s19;
        float p13 = s13 + s18;
        float p14 = s14 + s17;
        float p15 = s15 + s16;

        float pp0 = p0 + p15;
        float pp1 = p1 + p14;
        float pp2 = p2 + p13;
        float pp3 = p3 + p12;
        float pp4 = p4 + p11;
        float pp5 = p5 + p10;
        float pp6 = p6 + p9;
        float pp7 = p7 + p8;
        float pp8 = (p0 - p15) * cos1_32;
        float pp9 = (p1 - p14) * cos3_32;
        float pp10 = (p2 - p13) * cos5_32;
        float pp11 = (p3 - p12) * cos7_32;
        float pp12 = (p4 - p11) * cos9_32;
        float pp13 = (p5 - p10) * cos11_32;
        float pp14 = (p6 - p9) * cos13_32;
        float pp15 = (p7 - p8) * cos15_32;

        p0 = pp0 + pp7;
        p1 = pp1 + pp6;
        p2 = pp2 + pp5;
        p3 = pp3 + pp4;
        p4 = (pp0 - pp7) * cos1_16;
        p5 = (pp1 - pp6) * cos3_16;
        p6 = (pp2 - pp5) * cos5_16;
        p7 = (pp3 - pp4) * cos7_16;
        p8 = pp8 + pp15;
        p9 = pp9 + pp14;
        p10 = pp10 + pp13;
        p11 = pp11 + pp12;
        p12 = (pp8 - pp15) * cos1_16;
        p13 = (pp9 - pp14) * cos3_16;
        p14 = (pp10 - pp13) * cos5_16;
        p15 = (pp11 - pp12) * cos7_16;


        pp0 = p0 + p3;
        pp1 = p1 + p2;
        pp2 = (p0 - p3) * cos1_8;
        pp3 = (p1 - p2) * cos3_8;
        pp4 = p4 + p7;
        pp5 = p5 + p6;
        pp6 = (p4 - p7) * cos1_8;
        pp7 = (p5 - p6) * cos3_8;
        pp8 = p8 + p11;
        pp9 = p9 + p10;
        pp10 = (p8 - p11) * cos1_8;
        pp11 = (p9 - p10) * cos3_8;
        pp12 = p12 + p15;
        pp13 = p13 + p14;
        pp14 = (p12 - p15) * cos1_8;
        pp15 = (p13 - p14) * cos3_8;

        p0 = pp0 + pp1;
        p1 = (pp0 - pp1) * cos1_4;
        p2 = pp2 + pp3;
        p3 = (pp2 - pp3) * cos1_4;
        p4 = pp4 + pp5;
        p5 = (pp4 - pp5) * cos1_4;
        p6 = pp6 + pp7;
        p7 = (pp6 - pp7) * cos1_4;
        p8 = pp8 + pp9;
        p9 = (pp8 - pp9) * cos1_4;
        p10 = pp10 + pp11;
        p11 = (pp10 - pp11) * cos1_4;
        p12 = pp12 + pp13;
        p13 = (pp12 - pp13) * cos1_4;
        p14 = pp14 + pp15;
        p15 = (pp14 - pp15) * cos1_4;

        // this is pretty insane coding
        float tmp1;
        newValue[19] = -(newValue[4] = (newValue[12] = p7) + p5) - p6;
        newValue[27] = -p6 - p7 - p4;
        newValue[6] = (newValue[10] = (newValue[14] = p15) + p11) + p13;
        newValue[17] = -(newValue[2] = p15 + p13 + p9) - p14;
        newValue[21] = (tmp1 = -p14 - p15 - p10 - p11) - p13;
        newValue[29] = -p14 - p15 - p12 - p8;
        newValue[25] = tmp1 - p12;
        newValue[31] = -p0;
        newValue[0] = p1;
        newValue[23] = -(newValue[8] = p3) - p2;

        p0 = (s0 - s31) * cos1_64;
        p1 = (s1 - s30) * cos3_64;
        p2 = (s2 - s29) * cos5_64;
        p3 = (s3 - s28) * cos7_64;
        p4 = (s4 - s27) * cos9_64;
        p5 = (s5 - s26) * cos11_64;
        p6 = (s6 - s25) * cos13_64;
        p7 = (s7 - s24) * cos15_64;
        p8 = (s8 - s23) * cos17_64;
        p9 = (s9 - s22) * cos19_64;
        p10 = (s10 - s21) * cos21_64;
        p11 = (s11 - s20) * cos23_64;
        p12 = (s12 - s19) * cos25_64;
        p13 = (s13 - s18) * cos27_64;
        p14 = (s14 - s17) * cos29_64;
        p15 = (s15 - s16) * cos31_64;


        pp0 = p0 + p15;
        pp1 = p1 + p14;
        pp2 = p2 + p13;
        pp3 = p3 + p12;
        pp4 = p4 + p11;
        pp5 = p5 + p10;
        pp6 = p6 + p9;
        pp7 = p7 + p8;
        pp8 = (p0 - p15) * cos1_32;
        pp9 = (p1 - p14) * cos3_32;
        pp10 = (p2 - p13) * cos5_32;
        pp11 = (p3 - p12) * cos7_32;
        pp12 = (p4 - p11) * cos9_32;
        pp13 = (p5 - p10) * cos11_32;
        pp14 = (p6 - p9) * cos13_32;
        pp15 = (p7 - p8) * cos15_32;


        p0 = pp0 + pp7;
        p1 = pp1 + pp6;
        p2 = pp2 + pp5;
        p3 = pp3 + pp4;
        p4 = (pp0 - pp7) * cos1_16;
        p5 = (pp1 - pp6) * cos3_16;
        p6 = (pp2 - pp5) * cos5_16;
        p7 = (pp3 - pp4) * cos7_16;
        p8 = pp8 + pp15;
        p9 = pp9 + pp14;
        p10 = pp10 + pp13;
        p11 = pp11 + pp12;
        p12 = (pp8 - pp15) * cos1_16;
        p13 = (pp9 - pp14) * cos3_16;
        p14 = (pp10 - pp13) * cos5_16;
        p15 = (pp11 - pp12) * cos7_16;


        pp0 = p0 + p3;
        pp1 = p1 + p2;
        pp2 = (p0 - p3) * cos1_8;
        pp3 = (p1 - p2) * cos3_8;
        pp4 = p4 + p7;
        pp5 = p5 + p6;
        pp6 = (p4 - p7) * cos1_8;
        pp7 = (p5 - p6) * cos3_8;
        pp8 = p8 + p11;
        pp9 = p9 + p10;
        pp10 = (p8 - p11) * cos1_8;
        pp11 = (p9 - p10) * cos3_8;
        pp12 = p12 + p15;
        pp13 = p13 + p14;
        pp14 = (p12 - p15) * cos1_8;
        pp15 = (p13 - p14) * cos3_8;


        p0 = pp0 + pp1;
        p1 = (pp0 - pp1) * cos1_4;
        p2 = pp2 + pp3;
        p3 = (pp2 - pp3) * cos1_4;
        p4 = pp4 + pp5;
        p5 = (pp4 - pp5) * cos1_4;
        p6 = pp6 + pp7;
        p7 = (pp6 - pp7) * cos1_4;
        p8 = pp8 + pp9;
        p9 = (pp8 - pp9) * cos1_4;
        p10 = pp10 + pp11;
        p11 = (pp10 - pp11) * cos1_4;
        p12 = pp12 + pp13;
        p13 = (pp12 - pp13) * cos1_4;
        p14 = pp14 + pp15;
        p15 = (pp14 - pp15) * cos1_4;


        // manually doing something that a compiler should handle sucks
        // coding like this is hard to read
        float tmp2;
        newValue[5] = (newValue[11] = (newValue[13] = (newValue[15] = p15) + p7) + p11)
                + p5 + p13;
        newValue[7] = (newValue[9] = p15 + p11 + p3) + p13;
        newValue[16] = -(newValue[1] = (tmp1 = p13 + p15 + p9) + p1) - p14;
        newValue[18] = -(newValue[3] = tmp1 + p5 + p7) - p6 - p14;

        newValue[22] = (tmp1 = -p10 - p11 - p14 - p15) - p13 - p2 - p3;
        newValue[20] = tmp1 - p13 - p5 - p6 - p7;
        newValue[24] = tmp1 - p12 - p2 - p3;
        newValue[26] = tmp1 - p12 - (tmp2 = p4 + p6 + p7);
        newValue[30] = (tmp1 = -p8 - p12 - p14 - p15) - p0;
        newValue[28] = tmp1 - tmp2;

        // insert V[0-15] (== new_v[0-15]) into actual v:
        // float[] x2 = actual_v + actual_write_pos;
        float[] dest = _actualV;

        int pos = _actualWritePos;

        dest[pos] = newValue[0];
        dest[16 + pos] = newValue[1];
        dest[32 + pos] = newValue[2];
        dest[48 + pos] = newValue[3];
        dest[64 + pos] = newValue[4];
        dest[80 + pos] = newValue[5];
        dest[96 + pos] = newValue[6];
        dest[112 + pos] = newValue[7];
        dest[128 + pos] = newValue[8];
        dest[144 + pos] = newValue[9];
        dest[160 + pos] = newValue[10];
        dest[176 + pos] = newValue[11];
        dest[192 + pos] = newValue[12];
        dest[208 + pos] = newValue[13];
        dest[224 + pos] = newValue[14];
        dest[240 + pos] = newValue[15];

        // V[16] is always 0.0:
        dest[256 + pos] = 0.0f;

        // insert V[17-31] (== -new_v[15-1]) into actual v:
        dest[272 + pos] = -newValue[15];
        dest[288 + pos] = -newValue[14];
        dest[304 + pos] = -newValue[13];
        dest[320 + pos] = -newValue[12];
        dest[336 + pos] = -newValue[11];
        dest[352 + pos] = -newValue[10];
        dest[368 + pos] = -newValue[9];
        dest[384 + pos] = -newValue[8];
        dest[400 + pos] = -newValue[7];
        dest[416 + pos] = -newValue[6];
        dest[432 + pos] = -newValue[5];
        dest[448 + pos] = -newValue[4];
        dest[464 + pos] = -newValue[3];
        dest[480 + pos] = -newValue[2];
        dest[496 + pos] = -newValue[1];

        // insert V[32] (== -new_v[0]) into other v:
        dest = _actualV == v1 ? v2 : v1;

        dest[pos] = -newValue[0];
        // insert V[33-48] (== new_v[16-31]) into other v:
        dest[16 + pos] = newValue[16];
        dest[32 + pos] = newValue[17];
        dest[48 + pos] = newValue[18];
        dest[64 + pos] = newValue[19];
        dest[80 + pos] = newValue[20];
        dest[96 + pos] = newValue[21];
        dest[112 + pos] = newValue[22];
        dest[128 + pos] = newValue[23];
        dest[144 + pos] = newValue[24];
        dest[160 + pos] = newValue[25];
        dest[176 + pos] = newValue[26];
        dest[192 + pos] = newValue[27];
        dest[208 + pos] = newValue[28];
        dest[224 + pos] = newValue[29];
        dest[240 + pos] = newValue[30];
        dest[256 + pos] = newValue[31];

        // insert V[49-63] (== new_v[30-16]) into other v:
        dest[272 + pos] = newValue[30];
        dest[288 + pos] = newValue[29];
        dest[304 + pos] = newValue[28];
        dest[320 + pos] = newValue[27];
        dest[336 + pos] = newValue[26];
        dest[352 + pos] = newValue[25];
        dest[368 + pos] = newValue[24];
        dest[384 + pos] = newValue[23];
        dest[400 + pos] = newValue[22];
        dest[416 + pos] = newValue[21];
        dest[432 + pos] = newValue[20];
        dest[448 + pos] = newValue[19];
        dest[464 + pos] = newValue[18];
        dest[480 + pos] = newValue[17];
        dest[496 + pos] = newValue[16];
    }

    private void ComputePcmSamples0()
    {
        float[] vp = _actualV;
        //int inc = v_inc;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float pcm_sample;
            float[] dp = d16[i];
            pcm_sample = (vp[dvp] * dp[0] +
                    vp[15 + dvp] * dp[1] +
                    vp[14 + dvp] * dp[2] +
                    vp[13 + dvp] * dp[3] +
                    vp[12 + dvp] * dp[4] +
                    vp[11 + dvp] * dp[5] +
                    vp[10 + dvp] * dp[6] +
                    vp[9 + dvp] * dp[7] +
                    vp[8 + dvp] * dp[8] +
                    vp[7 + dvp] * dp[9] +
                    vp[6 + dvp] * dp[10] +
                    vp[5 + dvp] * dp[11] +
                    vp[4 + dvp] * dp[12] +
                    vp[3 + dvp] * dp[13] +
                    vp[2 + dvp] * dp[14] +
                    vp[1 + dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;

            dvp += 16;
        } // for
    }

    private void ComputePcmSamples1()
    {
        float[] vp = _actualV;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float[] dp = d16[i];
            float pcm_sample;

            pcm_sample = (vp[1 + dvp] * dp[0] +
                    vp[dvp] * dp[1] +
                    vp[15 + dvp] * dp[2] +
                    vp[14 + dvp] * dp[3] +
                    vp[13 + dvp] * dp[4] +
                    vp[12 + dvp] * dp[5] +
                    vp[11 + dvp] * dp[6] +
                    vp[10 + dvp] * dp[7] +
                    vp[9 + dvp] * dp[8] +
                    vp[8 + dvp] * dp[9] +
                    vp[7 + dvp] * dp[10] +
                    vp[6 + dvp] * dp[11] +
                    vp[5 + dvp] * dp[12] +
                    vp[4 + dvp] * dp[13] +
                    vp[3 + dvp] * dp[14] +
                    vp[2 + dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;

            dvp += 16;
        } // for
    }

    private void ComputePcmSamples2()
    {
        float[] vp = _actualV;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float[] dp = d16[i];
            float pcm_sample;

            pcm_sample = (vp[2 + dvp] * dp[0] +
                    vp[1 + dvp] * dp[1] +
                    vp[dvp] * dp[2] +
                    vp[15 + dvp] * dp[3] +
                    vp[14 + dvp] * dp[4] +
                    vp[13 + dvp] * dp[5] +
                    vp[12 + dvp] * dp[6] +
                    vp[11 + dvp] * dp[7] +
                    vp[10 + dvp] * dp[8] +
                    vp[9 + dvp] * dp[9] +
                    vp[8 + dvp] * dp[10] +
                    vp[7 + dvp] * dp[11] +
                    vp[6 + dvp] * dp[12] +
                    vp[5 + dvp] * dp[13] +
                    vp[4 + dvp] * dp[14] +
                    vp[3 + dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;

            dvp += 16;
        } // for
    }

    private void ComputePcmSamples3()
    {
        float[] vp = _actualV;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float[] dp = d16[i];
            float pcm_sample;

            pcm_sample = (vp[3 + dvp] * dp[0] +
                    vp[2 + dvp] * dp[1] +
                    vp[1 + dvp] * dp[2] +
                    vp[dvp] * dp[3] +
                    vp[15 + dvp] * dp[4] +
                    vp[14 + dvp] * dp[5] +
                    vp[13 + dvp] * dp[6] +
                    vp[12 + dvp] * dp[7] +
                    vp[11 + dvp] * dp[8] +
                    vp[10 + dvp] * dp[9] +
                    vp[9 + dvp] * dp[10] +
                    vp[8 + dvp] * dp[11] +
                    vp[7 + dvp] * dp[12] +
                    vp[6 + dvp] * dp[13] +
                    vp[5 + dvp] * dp[14] +
                    vp[4 + dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;

            dvp += 16;
        } // for
    }

    private void ComputePcmSamples4()
    {
        float[] vp = _actualV;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float[] dp = d16[i];
            float pcm_sample;

            pcm_sample = (vp[4 + dvp] * dp[0] +
                    vp[3 + dvp] * dp[1] +
                    vp[2 + dvp] * dp[2] +
                    vp[1 + dvp] * dp[3] +
                    vp[dvp] * dp[4] +
                    vp[15 + dvp] * dp[5] +
                    vp[14 + dvp] * dp[6] +
                    vp[13 + dvp] * dp[7] +
                    vp[12 + dvp] * dp[8] +
                    vp[11 + dvp] * dp[9] +
                    vp[10 + dvp] * dp[10] +
                    vp[9 + dvp] * dp[11] +
                    vp[8 + dvp] * dp[12] +
                    vp[7 + dvp] * dp[13] +
                    vp[6 + dvp] * dp[14] +
                    vp[5 + dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;

            dvp += 16;
        } // for
    }

    private void ComputePcmSamples5()
    {
        float[] vp = _actualV;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float[] dp = d16[i];
            float pcm_sample;

            pcm_sample = (vp[5 + dvp] * dp[0] +
                    vp[4 + dvp] * dp[1] +
                    vp[3 + dvp] * dp[2] +
                    vp[2 + dvp] * dp[3] +
                    vp[1 + dvp] * dp[4] +
                    vp[dvp] * dp[5] +
                    vp[15 + dvp] * dp[6] +
                    vp[14 + dvp] * dp[7] +
                    vp[13 + dvp] * dp[8] +
                    vp[12 + dvp] * dp[9] +
                    vp[11 + dvp] * dp[10] +
                    vp[10 + dvp] * dp[11] +
                    vp[9 + dvp] * dp[12] +
                    vp[8 + dvp] * dp[13] +
                    vp[7 + dvp] * dp[14] +
                    vp[6 + dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;

            dvp += 16;
        } // for
    }

    private void ComputePcmSamples6()
    {
        float[] vp = _actualV;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float[] dp = d16[i];
            float pcm_sample;

            pcm_sample = (vp[6 + dvp] * dp[0] +
                    vp[5 + dvp] * dp[1] +
                    vp[4 + dvp] * dp[2] +
                    vp[3 + dvp] * dp[3] +
                    vp[2 + dvp] * dp[4] +
                    vp[1 + dvp] * dp[5] +
                    vp[dvp] * dp[6] +
                    vp[15 + dvp] * dp[7] +
                    vp[14 + dvp] * dp[8] +
                    vp[13 + dvp] * dp[9] +
                    vp[12 + dvp] * dp[10] +
                    vp[11 + dvp] * dp[11] +
                    vp[10 + dvp] * dp[12] +
                    vp[9 + dvp] * dp[13] +
                    vp[8 + dvp] * dp[14] +
                    vp[7 + dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;

            dvp += 16;
        } // for
    }

    private void ComputePcmSamples7()
    {
        float[] vp = _actualV;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float[] dp = d16[i];
            float pcm_sample;

            pcm_sample = (vp[7 + dvp] * dp[0] +
                    vp[6 + dvp] * dp[1] +
                    vp[5 + dvp] * dp[2] +
                    vp[4 + dvp] * dp[3] +
                    vp[3 + dvp] * dp[4] +
                    vp[2 + dvp] * dp[5] +
                    vp[1 + dvp] * dp[6] +
                    vp[dvp] * dp[7] +
                    vp[15 + dvp] * dp[8] +
                    vp[14 + dvp] * dp[9] +
                    vp[13 + dvp] * dp[10] +
                    vp[12 + dvp] * dp[11] +
                    vp[11 + dvp] * dp[12] +
                    vp[10 + dvp] * dp[13] +
                    vp[9 + dvp] * dp[14] +
                    vp[8 + dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;

            dvp += 16;
        } // for
    }

    private void ComputePcmSamples8()
    {
        float[] vp = _actualV;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float[] dp = d16[i];
            float pcm_sample;

            pcm_sample = (vp[8 + dvp] * dp[0] +
                    vp[7 + dvp] * dp[1] +
                    vp[6 + dvp] * dp[2] +
                    vp[5 + dvp] * dp[3] +
                    vp[4 + dvp] * dp[4] +
                    vp[3 + dvp] * dp[5] +
                    vp[2 + dvp] * dp[6] +
                    vp[1 + dvp] * dp[7] +
                    vp[dvp] * dp[8] +
                    vp[15 + dvp] * dp[9] +
                    vp[14 + dvp] * dp[10] +
                    vp[13 + dvp] * dp[11] +
                    vp[12 + dvp] * dp[12] +
                    vp[11 + dvp] * dp[13] +
                    vp[10 + dvp] * dp[14] +
                    vp[9 + dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;

            dvp += 16;
        } // for
    }

    private void ComputePcmSamples9()
    {
        float[] vp = _actualV;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float[] dp = d16[i];
            float pcm_sample;

            pcm_sample = (vp[9 + dvp] * dp[0] +
                    vp[8 + dvp] * dp[1] +
                    vp[7 + dvp] * dp[2] +
                    vp[6 + dvp] * dp[3] +
                    vp[5 + dvp] * dp[4] +
                    vp[4 + dvp] * dp[5] +
                    vp[3 + dvp] * dp[6] +
                    vp[2 + dvp] * dp[7] +
                    vp[1 + dvp] * dp[8] +
                    vp[dvp] * dp[9] +
                    vp[15 + dvp] * dp[10] +
                    vp[14 + dvp] * dp[11] +
                    vp[13 + dvp] * dp[12] +
                    vp[12 + dvp] * dp[13] +
                    vp[11 + dvp] * dp[14] +
                    vp[10 + dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;

            dvp += 16;
        } // for
    }

    private void ComputePcmSamples10()
    {
        float[] vp = _actualV;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float[] dp = d16[i];
            float pcm_sample;

            pcm_sample = (vp[10 + dvp] * dp[0] +
                    vp[9 + dvp] * dp[1] +
                    vp[8 + dvp] * dp[2] +
                    vp[7 + dvp] * dp[3] +
                    vp[6 + dvp] * dp[4] +
                    vp[5 + dvp] * dp[5] +
                    vp[4 + dvp] * dp[6] +
                    vp[3 + dvp] * dp[7] +
                    vp[2 + dvp] * dp[8] +
                    vp[1 + dvp] * dp[9] +
                    vp[dvp] * dp[10] +
                    vp[15 + dvp] * dp[11] +
                    vp[14 + dvp] * dp[12] +
                    vp[13 + dvp] * dp[13] +
                    vp[12 + dvp] * dp[14] +
                    vp[11 + dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;

            dvp += 16;
        } // for
    }

    private void ComputePcmSamples11()
    {
        float[] vp = _actualV;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float[] dp = d16[i];
            float pcm_sample;

            pcm_sample = (vp[11 + dvp] * dp[0] +
                    vp[10 + dvp] * dp[1] +
                    vp[9 + dvp] * dp[2] +
                    vp[8 + dvp] * dp[3] +
                    vp[7 + dvp] * dp[4] +
                    vp[6 + dvp] * dp[5] +
                    vp[5 + dvp] * dp[6] +
                    vp[4 + dvp] * dp[7] +
                    vp[3 + dvp] * dp[8] +
                    vp[2 + dvp] * dp[9] +
                    vp[1 + dvp] * dp[10] +
                    vp[dvp] * dp[11] +
                    vp[15 + dvp] * dp[12] +
                    vp[14 + dvp] * dp[13] +
                    vp[13 + dvp] * dp[14] +
                    vp[12 + dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;

            dvp += 16;
        } // for
    }

    private void ComputePcmSamples12()
    {
        float[] vp = _actualV;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float[] dp = d16[i];
            float pcm_sample;

            pcm_sample = (vp[12 + dvp] * dp[0] +
                    vp[11 + dvp] * dp[1] +
                    vp[10 + dvp] * dp[2] +
                    vp[9 + dvp] * dp[3] +
                    vp[8 + dvp] * dp[4] +
                    vp[7 + dvp] * dp[5] +
                    vp[6 + dvp] * dp[6] +
                    vp[5 + dvp] * dp[7] +
                    vp[4 + dvp] * dp[8] +
                    vp[3 + dvp] * dp[9] +
                    vp[2 + dvp] * dp[10] +
                    vp[1 + dvp] * dp[11] +
                    vp[dvp] * dp[12] +
                    vp[15 + dvp] * dp[13] +
                    vp[14 + dvp] * dp[14] +
                    vp[13 + dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;

            dvp += 16;
        } // for
    }

    // Note: These values are not in the same order
    // as in Annex 3-B.3 of the ISO/IEC DIS 11172-3
    // private float d[] = {0.000000000, -4.000442505};

    private void ComputePcmSamples13()
    {
        float[] vp = _actualV;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float[] dp = d16[i];
            float pcm_sample;

            pcm_sample = (vp[13 + dvp] * dp[0] +
                    vp[12 + dvp] * dp[1] +
                    vp[11 + dvp] * dp[2] +
                    vp[10 + dvp] * dp[3] +
                    vp[9 + dvp] * dp[4] +
                    vp[8 + dvp] * dp[5] +
                    vp[7 + dvp] * dp[6] +
                    vp[6 + dvp] * dp[7] +
                    vp[5 + dvp] * dp[8] +
                    vp[4 + dvp] * dp[9] +
                    vp[3 + dvp] * dp[10] +
                    vp[2 + dvp] * dp[11] +
                    vp[1 + dvp] * dp[12] +
                    vp[dvp] * dp[13] +
                    vp[15 + dvp] * dp[14] +
                    vp[14 + dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;

            dvp += 16;
        } // for
    }

    private void ComputePcmSamples14()
    {
        float[] vp = _actualV;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float[] dp = d16[i];
            float pcm_sample;

            pcm_sample = (vp[14 + dvp] * dp[0] +
                    vp[13 + dvp] * dp[1] +
                    vp[12 + dvp] * dp[2] +
                    vp[11 + dvp] * dp[3] +
                    vp[10 + dvp] * dp[4] +
                    vp[9 + dvp] * dp[5] +
                    vp[8 + dvp] * dp[6] +
                    vp[7 + dvp] * dp[7] +
                    vp[6 + dvp] * dp[8] +
                    vp[5 + dvp] * dp[9] +
                    vp[4 + dvp] * dp[10] +
                    vp[3 + dvp] * dp[11] +
                    vp[2 + dvp] * dp[12] +
                    vp[1 + dvp] * dp[13] +
                    vp[dvp] * dp[14] +
                    vp[15 + dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;

            dvp += 16;
        } // for
    }

    private void ComputePcmSamples15()
    {
        float[] vp = _actualV;
        int dvp = 0;

        // fat chance of having this loop unroll
        for (int i = 0; i < 32; i++)
        {
            float pcm_sample;
            float[] dp = d16[i];
            pcm_sample = (vp[15 + dvp] * dp[0] +
                    vp[14 + dvp] * dp[1] +
                    vp[13 + dvp] * dp[2] +
                    vp[12 + dvp] * dp[3] +
                    vp[11 + dvp] * dp[4] +
                    vp[10 + dvp] * dp[5] +
                    vp[9 + dvp] * dp[6] +
                    vp[8 + dvp] * dp[7] +
                    vp[7 + dvp] * dp[8] +
                    vp[6 + dvp] * dp[9] +
                    vp[5 + dvp] * dp[10] +
                    vp[4 + dvp] * dp[11] +
                    vp[3 + dvp] * dp[12] +
                    vp[2 + dvp] * dp[13] +
                    vp[1 + dvp] * dp[14] +
                    vp[dvp] * dp[15]
            ) * scalefactor;

            _tmpOut[i] = pcm_sample;
            dvp += 16;
        } // for
    }

    private void ComputePcmSamples(SampleBuffer buffer)
    {

        switch (_actualWritePos)
        {
            case 0:
                ComputePcmSamples0();
                break;
            case 1:
                ComputePcmSamples1();
                break;
            case 2:
                ComputePcmSamples2();
                break;
            case 3:
                ComputePcmSamples3();
                break;
            case 4:
                ComputePcmSamples4();
                break;
            case 5:
                ComputePcmSamples5();
                break;
            case 6:
                ComputePcmSamples6();
                break;
            case 7:
                ComputePcmSamples7();
                break;
            case 8:
                ComputePcmSamples8();
                break;
            case 9:
                ComputePcmSamples9();
                break;
            case 10:
                ComputePcmSamples10();
                break;
            case 11:
                ComputePcmSamples11();
                break;
            case 12:
                ComputePcmSamples12();
                break;
            case 13:
                ComputePcmSamples13();
                break;
            case 14:
                ComputePcmSamples14();
                break;
            case 15:
                ComputePcmSamples15();
                break;
        }

        buffer.AppendSamples(channel, _tmpOut);
    }

    /// <summary>
    /// Calculate 32 PCM samples and put the into the Obuffer-object.
    /// </summary>
    /// <param name="buffer"></param>
    public void CalculatePcmSamples(SampleBuffer buffer)
    {
        ComputeNewV();
        ComputePcmSamples(buffer);

        _actualWritePos = _actualWritePos + 1 & 0xf;
        _actualV = _actualV == v1 ? v2 : v1;

        // MDM: this may not be necessary. The Layer III decoder always
        // outputs 32 subband samples, but I haven't checked layer I & II.
        for (int p = 0; p < 32; p++)
        {
            samples[p] = 0.0f;
        }
    }
}

