using System;

namespace ColorMC.Gui.MusicPlayer.Decoder.Mp3;

public sealed class LayerIIIDecoder : IFrameDecoder
{
    public static readonly int[] Pretab =
    [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 3, 3, 3, 2, 0];

    public static readonly float[] TwoToNegativeHalfPow =
    [
        1.0000000000E+00f, 7.0710678119E-01f, 5.0000000000E-01f, 3.5355339059E-01f,
        2.5000000000E-01f, 1.7677669530E-01f, 1.2500000000E-01f, 8.8388347648E-02f,
        6.2500000000E-02f, 4.4194173824E-02f, 3.1250000000E-02f, 2.2097086912E-02f,
        1.5625000000E-02f, 1.1048543456E-02f, 7.8125000000E-03f, 5.5242717280E-03f,
        3.9062500000E-03f, 2.7621358640E-03f, 1.9531250000E-03f, 1.3810679320E-03f,
        9.7656250000E-04f, 6.9053396600E-04f, 4.8828125000E-04f, 3.4526698300E-04f,
        2.4414062500E-04f, 1.7263349150E-04f, 1.2207031250E-04f, 8.6316745750E-05f,
        6.1035156250E-05f, 4.3158372875E-05f, 3.0517578125E-05f, 2.1579186438E-05f,
        1.5258789062E-05f, 1.0789593219E-05f, 7.6293945312E-06f, 5.3947966094E-06f,
        3.8146972656E-06f, 2.6973983047E-06f, 1.9073486328E-06f, 1.3486991523E-06f,
        9.5367431641E-07f, 6.7434957617E-07f, 4.7683715820E-07f, 3.3717478809E-07f,
        2.3841857910E-07f, 1.6858739404E-07f, 1.1920928955E-07f, 8.4293697022E-08f,
        5.9604644775E-08f, 4.2146848511E-08f, 2.9802322388E-08f, 2.1073424255E-08f,
        1.4901161194E-08f, 1.0536712128E-08f, 7.4505805969E-09f, 5.2683560639E-09f,
        3.7252902985E-09f, 2.6341780319E-09f, 1.8626451492E-09f, 1.3170890160E-09f,
        9.3132257462E-10f, 6.5854450798E-10f, 4.6566128731E-10f, 3.2927225399E-10f
    ];
    public static readonly float[] T43 = CreateTab43();
    public static readonly float[,] Io =
    {
        {1.0000000000E+00f, 8.4089641526E-01f, 7.0710678119E-01f, 5.9460355751E-01f,
            5.0000000001E-01f, 4.2044820763E-01f, 3.5355339060E-01f, 2.9730177876E-01f,
            2.5000000001E-01f, 2.1022410382E-01f, 1.7677669530E-01f, 1.4865088938E-01f,
            1.2500000000E-01f, 1.0511205191E-01f, 8.8388347652E-02f, 7.4325444691E-02f,
            6.2500000003E-02f, 5.2556025956E-02f, 4.4194173826E-02f, 3.7162722346E-02f,
            3.1250000002E-02f, 2.6278012978E-02f, 2.2097086913E-02f, 1.8581361173E-02f,
            1.5625000001E-02f, 1.3139006489E-02f, 1.1048543457E-02f, 9.2906805866E-03f,
            7.8125000006E-03f, 6.5695032447E-03f, 5.5242717285E-03f, 4.6453402934E-03f},
        {1.0000000000E+00f, 7.0710678119E-01f, 5.0000000000E-01f, 3.5355339060E-01f,
            2.5000000000E-01f, 1.7677669530E-01f, 1.2500000000E-01f, 8.8388347650E-02f,
            6.2500000001E-02f, 4.4194173825E-02f, 3.1250000001E-02f, 2.2097086913E-02f,
            1.5625000000E-02f, 1.1048543456E-02f, 7.8125000002E-03f, 5.5242717282E-03f,
            3.9062500001E-03f, 2.7621358641E-03f, 1.9531250001E-03f, 1.3810679321E-03f,
            9.7656250004E-04f, 6.9053396603E-04f, 4.8828125002E-04f, 3.4526698302E-04f,
            2.4414062501E-04f, 1.7263349151E-04f, 1.2207031251E-04f, 8.6316745755E-05f,
            6.1035156254E-05f, 4.3158372878E-05f, 3.0517578127E-05f, 2.1579186439E-05f}
    };
    public static readonly float[] TAN12 =
    [
        0.0f, 0.26794919f, 0.57735027f, 1.0f,
        1.73205081f, 3.73205081f, 9.9999999e10f, -3.73205081f,
        -1.73205081f, -1.0f, -0.57735027f, -0.26794919f,
        0.0f, 0.26794919f, 0.57735027f, 1.0f
    ];

    public static readonly float[][] Win =
    [
        [-1.6141214951E-02f, -5.3603178919E-02f, -1.0070713296E-01f, -1.6280817573E-01f,
            -4.9999999679E-01f, -3.8388735032E-01f, -6.2061144372E-01f, -1.1659756083E+00f,
            -3.8720752656E+00f, -4.2256286556E+00f, -1.5195289984E+00f, -9.7416483388E-01f,
            -7.3744074053E-01f, -1.2071067773E+00f, -5.1636156596E-01f, -4.5426052317E-01f,
            -4.0715656898E-01f, -3.6969460527E-01f, -3.3876269197E-01f, -3.1242222492E-01f,
            -2.8939587111E-01f, -2.6880081906E-01f, -5.0000000266E-01f, -2.3251417468E-01f,
            -2.1596714708E-01f, -2.0004979098E-01f, -1.8449493497E-01f, -1.6905846094E-01f,
            -1.5350360518E-01f, -1.3758624925E-01f, -1.2103922149E-01f, -2.0710679058E-01f,
            -8.4752577594E-02f, -6.4157525656E-02f, -4.1131172614E-02f, -1.4790705759E-02f],

        [-1.6141214951E-02f, -5.3603178919E-02f, -1.0070713296E-01f, -1.6280817573E-01f,
            -4.9999999679E-01f, -3.8388735032E-01f, -6.2061144372E-01f, -1.1659756083E+00f,
            -3.8720752656E+00f, -4.2256286556E+00f, -1.5195289984E+00f, -9.7416483388E-01f,
            -7.3744074053E-01f, -1.2071067773E+00f, -5.1636156596E-01f, -4.5426052317E-01f,
            -4.0715656898E-01f, -3.6969460527E-01f, -3.3908542600E-01f, -3.1511810350E-01f,
            -2.9642226150E-01f, -2.8184548650E-01f, -5.4119610000E-01f, -2.6213228100E-01f,
            -2.5387916537E-01f, -2.3296291359E-01f, -1.9852728987E-01f, -1.5233534808E-01f,
            -9.6496400054E-02f, -3.3423828516E-02f, 0.0000000000E+00f, 0.0000000000E+00f,
            0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f],

        [-4.8300800645E-02f, -1.5715656932E-01f, -2.8325045177E-01f, -4.2953747763E-01f,
            -1.2071067795E+00f, -8.2426483178E-01f, -1.1451749106E+00f, -1.7695290101E+00f,
            -4.5470225061E+00f, -3.4890531002E+00f, -7.3296292804E-01f, -1.5076514758E-01f,
            0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f,
            0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f,
            0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f,
            0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f,
            0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f,
            0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f],

        [0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f, 0.0000000000E+00f,
            0.0000000000E+00f, 0.0000000000E+00f, -1.5076513660E-01f, -7.3296291107E-01f,
            -3.4890530566E+00f, -4.5470224727E+00f, -1.7695290031E+00f, -1.1451749092E+00f,
            -8.3137738100E-01f, -1.3065629650E+00f, -5.4142014250E-01f, -4.6528974900E-01f,
            -4.1066990750E-01f, -3.7004680800E-01f, -3.3876269197E-01f, -3.1242222492E-01f,
            -2.8939587111E-01f, -2.6880081906E-01f, -5.0000000266E-01f, -2.3251417468E-01f,
            -2.1596714708E-01f, -2.0004979098E-01f, -1.8449493497E-01f, -1.6905846094E-01f,
            -1.5350360518E-01f, -1.3758624925E-01f, -1.2103922149E-01f, -2.0710679058E-01f,
            -8.4752577594E-02f, -6.4157525656E-02f, -4.1131172614E-02f, -1.4790705759E-02f]
    ];
    public static readonly int[,,] NrOfSfbBlock =
    {{{6, 5, 5, 5}, {9, 9, 9, 9}, {6, 9, 9, 9}},
        {{6, 5, 7, 3}, {9, 9, 12, 6}, {6, 9, 12, 6}},
        {{11, 10, 0, 0}, {18, 18, 0, 0}, {15, 18, 0, 0}},
        {{7, 7, 7, 0}, {12, 12, 12, 0}, {6, 15, 12, 0}},
        {{6, 6, 6, 3}, {12, 9, 9, 6}, {6, 12, 9, 6}},
        {{8, 8, 5, 0}, {15, 12, 9, 0}, {6, 18, 9, 0}}};
    private static readonly int s_sslimit = 18;
    private static readonly int s_sblimit = 32;
    private static readonly int[,] s_slen =
    {
        {0, 0, 0, 0, 3, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4},
        {0, 1, 2, 3, 0, 1, 2, 3, 1, 2, 3, 1, 2, 3, 2, 3}
    };
    private static readonly float[] s_cs =
    [
        0.857492925712f, 0.881741997318f, 0.949628649103f, 0.983314592492f,
        0.995517816065f, 0.999160558175f, 0.999899195243f, 0.999993155067f
    ];
    private static readonly float[] s_ca =
    [
        -0.5144957554270f, -0.4717319685650f, -0.3133774542040f, -0.1819131996110f,
        -0.0945741925262f, -0.0409655828852f, -0.0141985685725f, -0.00369997467375f
    ];

    private static int[][] s_reorderTable;    // SZD: will be generated on demand

    private readonly double _d43 = 4.0 / 3.0;

    // MDM: new_slen is fully initialized before use, no need
    // to reallocate array.
    private readonly int[] _newSlen = new int[4];
    private readonly int[] _is1d;
    private readonly float[][,] _ro;
    private readonly float[][,] _lr;
    private readonly float[] _out1d;
    private readonly float[][] _prevblck;
    private readonly float[,] _k;
    private readonly int[] _nonzero;
    private readonly BitStream _stream;
    private readonly Mp3Header _header;
    private readonly SynthesisFilter _filter1, _filter2;
    private readonly SampleBuffer _buffer;
    private readonly BitReserve _br;
    private readonly IIISideInfo _si;
    private readonly Temporaire2[] _scalefac;
    private readonly int _maxGr;
    private readonly int _channels;
    private readonly int _firstChannel;
    private readonly int _lastChannel;
    private readonly int _sfreq;

    // Decode one frame, filling the buffer with the output samples.
    // subband samples are buffered and passed to the
    // SynthesisFilter in one go.
    private readonly float[] _samples1 = new float[32];
    private readonly float[] _samples2 = new float[32];
    private readonly SBI[] _sfBandIndex; // Init in the constructor.

    private readonly int[] _scalefacBuffer;


    private readonly int[] _x = [0];
    private readonly int[] _y = [0];
    private readonly int[] _v = [0];
    private readonly int[] _w = [0];

    private readonly int[] _isPos = new int[576];
    private readonly float[] _isRatio = new float[576];


    private readonly float[] _tsOutCopy = new float[18];
    private readonly float[] _rawout = new float[36];

    private int _checkSumHuff = 0;
    private int _frameStart;
    private int _part2Start;

    public LayerIIIDecoder(BitStream stream0, Mp3Header header0,
                           SynthesisFilter filtera, SynthesisFilter filterb,
                           SampleBuffer buffer0)
    {
        _is1d = new int[s_sblimit * s_sslimit + 4];
        _ro = new float[2][,];
        _ro[0] = new float[s_sblimit, s_sslimit];
        _ro[1] = new float[s_sblimit, s_sslimit];
        _lr = new float[2][,];
        _lr[0] = new float[s_sblimit, s_sslimit];
        _lr[1] = new float[s_sblimit, s_sslimit];
        _out1d = new float[s_sblimit * s_sslimit];
        _prevblck = new float[2][];
        _prevblck[0] = new float[s_sblimit * s_sslimit];
        _prevblck[1] = new float[s_sblimit * s_sslimit];
        _k = new float[2, s_sblimit * s_sslimit];
        _nonzero = new int[2];

        //III_scalefact_t
        Temporaire2[] III_scalefac_t = [new Temporaire2(), new Temporaire2()];
        _scalefac = III_scalefac_t;
        // L3TABLE INIT

        _sfBandIndex = new SBI[9];    // SZD: MPEG2.5 +3 indices
        int[] l0 = [0, 6, 12, 18, 24, 30, 36, 44, 54, 66, 80, 96, 116, 140, 168, 200, 238, 284, 336, 396, 464, 522, 576];
        int[] s0 = [0, 4, 8, 12, 18, 24, 32, 42, 56, 74, 100, 132, 174, 192];
        int[] l1 = [0, 6, 12, 18, 24, 30, 36, 44, 54, 66, 80, 96, 114, 136, 162, 194, 232, 278, 330, 394, 464, 540, 576];
        int[] s1 = [0, 4, 8, 12, 18, 26, 36, 48, 62, 80, 104, 136, 180, 192];
        int[] l2 = [0, 6, 12, 18, 24, 30, 36, 44, 54, 66, 80, 96, 116, 140, 168, 200, 238, 284, 336, 396, 464, 522, 576];
        int[] s2 = [0, 4, 8, 12, 18, 26, 36, 48, 62, 80, 104, 134, 174, 192];

        int[] l3 = [0, 4, 8, 12, 16, 20, 24, 30, 36, 44, 52, 62, 74, 90, 110, 134, 162, 196, 238, 288, 342, 418, 576];
        int[] s3 = [0, 4, 8, 12, 16, 22, 30, 40, 52, 66, 84, 106, 136, 192];
        int[] l4 = [0, 4, 8, 12, 16, 20, 24, 30, 36, 42, 50, 60, 72, 88, 106, 128, 156, 190, 230, 276, 330, 384, 576];
        int[] s4 = [0, 4, 8, 12, 16, 22, 28, 38, 50, 64, 80, 100, 126, 192];
        int[] l5 = [0, 4, 8, 12, 16, 20, 24, 30, 36, 44, 54, 66, 82, 102, 126, 156, 194, 240, 296, 364, 448, 550, 576];
        int[] s5 = [0, 4, 8, 12, 16, 22, 30, 42, 58, 78, 104, 138, 180, 192];
        // SZD: MPEG2.5
        int[] l6 = [0, 6, 12, 18, 24, 30, 36, 44, 54, 66, 80, 96, 116, 140, 168, 200, 238, 284, 336, 396, 464, 522, 576];
        int[] s6 = [0, 4, 8, 12, 18, 26, 36, 48, 62, 80, 104, 134, 174, 192];
        int[] l7 = [0, 6, 12, 18, 24, 30, 36, 44, 54, 66, 80, 96, 116, 140, 168, 200, 238, 284, 336, 396, 464, 522, 576];
        int[] s7 = [0, 4, 8, 12, 18, 26, 36, 48, 62, 80, 104, 134, 174, 192];
        int[] l8 = [0, 12, 24, 36, 48, 60, 72, 88, 108, 132, 160, 192, 232, 280, 336, 400, 476, 566, 568, 570, 572, 574, 576];
        int[] s8 = [0, 8, 16, 24, 36, 52, 72, 96, 124, 160, 162, 164, 166, 192];

        _sfBandIndex[0] = new SBI(l0, s0);
        _sfBandIndex[1] = new SBI(l1, s1);
        _sfBandIndex[2] = new SBI(l2, s2);

        _sfBandIndex[3] = new SBI(l3, s3);
        _sfBandIndex[4] = new SBI(l4, s4);
        _sfBandIndex[5] = new SBI(l5, s5);
        //SZD: MPEG2.5
        _sfBandIndex[6] = new SBI(l6, s6);
        _sfBandIndex[7] = new SBI(l7, s7);
        _sfBandIndex[8] = new SBI(l8, s8);
        // END OF L3TABLE INIT

        if (s_reorderTable == null)
        {    // SZD: generate LUT
            s_reorderTable = new int[9][];
            for (int i = 0; i < 9; i++)
                s_reorderTable[i] = Reorder(_sfBandIndex[i].s);
        }

        // Sftable
        //int[] ll0 = [0, 6, 11, 16, 21];
        //int[] ss0 = [0, 6, 12];
        //_sftable = new Sftable(ll0, ss0);
        // END OF Sftable

        // scalefac_buffer
        _scalefacBuffer = new int[54];
        // END OF scalefac_buffer

        _stream = stream0;
        _header = header0;
        _filter1 = filtera;
        _filter2 = filterb;
        _buffer = buffer0;

        _frameStart = 0;
        _channels = _header.Mode == ChannelType.SingelChannel ? 1 : 2;
        _maxGr = _header.Version == VersionType.Mpeg1 ? 2 : 1;

        _sfreq = (int)_header.SampleFrequency +
                (_header.Version == VersionType.Mpeg1 ? 3 :
                        _header.Version == VersionType.Mpeg25LSF ? 6 : 0);    // SZD

        if (_channels == 2)
        {
            _firstChannel = 0;
            _lastChannel = 1;
        }
        else
        {
            _firstChannel = _lastChannel = 0;
        }

        for (int ch = 0; ch < 2; ch++)
            for (int j = 0; j < 576; j++)
                _prevblck[ch][j] = 0.0f;

        _nonzero[0] = _nonzero[1] = 576;

        _br = new BitReserve();
        _si = new IIISideInfo();
    }

    private static float[] CreateTab43()
    {
        float[] t43 = new float[8192];
        double d43 = 4.0 / 3.0;

        for (int i = 0; i < 8192; i++)
        {
            t43[i] = (float)Math.Pow(i, d43);
        }
        return t43;
    }

    private static int[] Reorder(int[] scalefac_band)
    {    // SZD: converted from LAME
        int j = 0;
        int[] ix = new int[576];
        for (int sfb = 0; sfb < 13; sfb++)
        {
            int start = scalefac_band[sfb];
            int end = scalefac_band[sfb + 1];
            for (int window = 0; window < 3; window++)
                for (int i = start; i < end; i++)
                    ix[3 * i + window] = j++;
        }
        return ix;
    }

    // Size of the table of whole numbers raised to 4/3 power.
    // This may be adjusted for performance without any problems.

    public void DecodeFrame()
    {
        int nSlots = _header.Slots;
        int flush_main;
        int gr, ch, ss, sb, sb18;
        int main_data_end;
        int bytes_to_discard;
        int i;

        GetSideInfo();

        for (i = 0; i < nSlots; i++)
            _br.Putbuf(_stream.GetBits(8));

        main_data_end = _br.Hsstell >>> 3; // of previous frame

        if ((flush_main = _br.Hsstell & 7) != 0)
        {
            _br.Getbits(8 - flush_main);
            main_data_end++;
        }

        bytes_to_discard = _frameStart - main_data_end
                - _si.main_data_begin;

        _frameStart += nSlots;

        if (bytes_to_discard < 0)
            return;

        if (main_data_end > 4096)
        {
            _frameStart -= 4096;
            _br.RewindNbytes(4096);
        }

        for (; bytes_to_discard > 0; bytes_to_discard--)
            _br.Getbits(8);

        for (gr = 0; gr < _maxGr; gr++)
        {
            for (ch = 0; ch < _channels; ch++)
            {
                _part2Start = _br.Hsstell;

                if (_header.Version == VersionType.Mpeg1)
                    GetScaleFactors(ch, gr);
                else  // MPEG-2 LSF, SZD: MPEG-2.5 LSF
                    GetLSFScaleFactors(ch, gr);

                HuffmanDecode(ch, gr);
                DequantizeSample(_ro[ch], ch, gr);
            }

            Stereo(gr);

            for (ch = _firstChannel; ch <= _lastChannel; ch++)
            {
                Reorder(_lr[ch], ch, gr);
                Antialias(ch, gr);

                Hybrid(ch, gr);

                for (sb18 = 18; sb18 < 576; sb18 += 36) // Frequency inversion
                    for (ss = 1; ss < s_sslimit; ss += 2)
                        _out1d[sb18 + ss] = -_out1d[sb18 + ss];

                if (ch == 0)
                {
                    for (ss = 0; ss < s_sslimit; ss++)
                    {
                        // Polyphase synthesis
                        sb = 0;
                        for (sb18 = 0; sb18 < 576; sb18 += 18)
                        {
                            _samples1[sb] = _out1d[sb18 + ss];
                            sb++;
                        }
                        _filter1.InputSamples(_samples1);
                        _filter1.CalculatePcmSamples(_buffer);
                    }
                }
                else
                {
                    for (ss = 0; ss < s_sslimit; ss++)
                    {
                        // Polyphase synthesis
                        sb = 0;
                        for (sb18 = 0; sb18 < 576; sb18 += 18)
                        {
                            _samples2[sb] = _out1d[sb18 + ss];
                            sb++;
                        }
                        _filter2.InputSamples(_samples2);
                        _filter2.CalculatePcmSamples(_buffer);
                    }
                }
            }
        }
    }

    /**
     * Reads the side info from the stream, assuming the entire.
     * frame has been read already.
     * Mono   : 136 bits (= 17 bytes)
     * Stereo : 256 bits (= 32 bytes)
     */
    private void GetSideInfo()
    {
        int ch, gr;
        if (_header.Version == VersionType.Mpeg1)
        {
            _si.main_data_begin = _stream.GetBits(9);
            if (_channels == 1)
                _si.private_bits = _stream.GetBits(5);
            else _si.private_bits = _stream.GetBits(3);

            for (ch = 0; ch < _channels; ch++)
            {
                _si.ch[ch].scfsi[0] = _stream.GetBits(1);
                _si.ch[ch].scfsi[1] = _stream.GetBits(1);
                _si.ch[ch].scfsi[2] = _stream.GetBits(1);
                _si.ch[ch].scfsi[3] = _stream.GetBits(1);
            }

            for (gr = 0; gr < 2; gr++)
            {
                for (ch = 0; ch < _channels; ch++)
                {
                    _si.ch[ch].gr[gr].Part2_3Length = _stream.GetBits(12);
                    _si.ch[ch].gr[gr].BigValues = _stream.GetBits(9);
                    _si.ch[ch].gr[gr].GlobalGain = _stream.GetBits(8);
                    _si.ch[ch].gr[gr].ScalefacCompress = _stream.GetBits(4);
                    _si.ch[ch].gr[gr].WindowSwitchingFlag = _stream.GetBits(1);
                    if (_si.ch[ch].gr[gr].WindowSwitchingFlag != 0)
                    {
                        _si.ch[ch].gr[gr].BlockType = _stream.GetBits(2);
                        _si.ch[ch].gr[gr].MixedBlockFlag = _stream.GetBits(1);

                        _si.ch[ch].gr[gr].TableSelect[0] = _stream.GetBits(5);
                        _si.ch[ch].gr[gr].TableSelect[1] = _stream.GetBits(5);

                        _si.ch[ch].gr[gr].SubblockGain[0] = _stream.GetBits(3);
                        _si.ch[ch].gr[gr].SubblockGain[1] = _stream.GetBits(3);
                        _si.ch[ch].gr[gr].SubblockGain[2] = _stream.GetBits(3);

                        // Set region_count parameters since they are implicit in this case.

                        if (_si.ch[ch].gr[gr].BlockType == 0)
                        {
                            //	 Side info bad: block_type == 0 in split block
                            return;
                        }
                        else if (_si.ch[ch].gr[gr].BlockType == 2
                                && _si.ch[ch].gr[gr].MixedBlockFlag == 0)
                        {
                            _si.ch[ch].gr[gr].Region0Count = 8;
                        }
                        else
                        {
                            _si.ch[ch].gr[gr].Region0Count = 7;
                        }
                        _si.ch[ch].gr[gr].Region1Count = 20 -
                                _si.ch[ch].gr[gr].Region0Count;
                    }
                    else
                    {
                        _si.ch[ch].gr[gr].TableSelect[0] = _stream.GetBits(5);
                        _si.ch[ch].gr[gr].TableSelect[1] = _stream.GetBits(5);
                        _si.ch[ch].gr[gr].TableSelect[2] = _stream.GetBits(5);
                        _si.ch[ch].gr[gr].Region0Count = _stream.GetBits(4);
                        _si.ch[ch].gr[gr].Region1Count = _stream.GetBits(3);
                        _si.ch[ch].gr[gr].BlockType = 0;
                    }
                    _si.ch[ch].gr[gr].Preflag = _stream.GetBits(1);
                    _si.ch[ch].gr[gr].ScalefacScale = _stream.GetBits(1);
                    _si.ch[ch].gr[gr].Count1tableSelect = _stream.GetBits(1);
                }
            }

        }
        else
        {    // MPEG-2 LSF, SZD: MPEG-2.5 LSF

            _si.main_data_begin = _stream.GetBits(8);
            if (_channels == 1)
                _si.private_bits = _stream.GetBits(1);
            else _si.private_bits = _stream.GetBits(2);

            for (ch = 0; ch < _channels; ch++)
            {

                _si.ch[ch].gr[0].Part2_3Length = _stream.GetBits(12);
                _si.ch[ch].gr[0].BigValues = _stream.GetBits(9);
                _si.ch[ch].gr[0].GlobalGain = _stream.GetBits(8);
                _si.ch[ch].gr[0].ScalefacCompress = _stream.GetBits(9);
                _si.ch[ch].gr[0].WindowSwitchingFlag = _stream.GetBits(1);

                if (_si.ch[ch].gr[0].WindowSwitchingFlag != 0)
                {

                    _si.ch[ch].gr[0].BlockType = _stream.GetBits(2);
                    _si.ch[ch].gr[0].MixedBlockFlag = _stream.GetBits(1);
                    _si.ch[ch].gr[0].TableSelect[0] = _stream.GetBits(5);
                    _si.ch[ch].gr[0].TableSelect[1] = _stream.GetBits(5);

                    _si.ch[ch].gr[0].SubblockGain[0] = _stream.GetBits(3);
                    _si.ch[ch].gr[0].SubblockGain[1] = _stream.GetBits(3);
                    _si.ch[ch].gr[0].SubblockGain[2] = _stream.GetBits(3);

                    // Set region_count parameters since they are implicit in this case.

                    if (_si.ch[ch].gr[0].BlockType == 0)
                    {
                        // Side info bad: block_type == 0 in split block
                        return;
                    }
                    else if (_si.ch[ch].gr[0].BlockType == 2
                            && _si.ch[ch].gr[0].MixedBlockFlag == 0)
                    {
                        _si.ch[ch].gr[0].Region0Count = 8;
                    }
                    else
                    {
                        _si.ch[ch].gr[0].Region0Count = 7;
                        _si.ch[ch].gr[0].Region1Count = 20 -
                                _si.ch[ch].gr[0].Region0Count;
                    }

                }
                else
                {
                    _si.ch[ch].gr[0].TableSelect[0] = _stream.GetBits(5);
                    _si.ch[ch].gr[0].TableSelect[1] = _stream.GetBits(5);
                    _si.ch[ch].gr[0].TableSelect[2] = _stream.GetBits(5);
                    _si.ch[ch].gr[0].Region0Count = _stream.GetBits(4);
                    _si.ch[ch].gr[0].Region1Count = _stream.GetBits(3);
                    _si.ch[ch].gr[0].BlockType = 0;
                }

                _si.ch[ch].gr[0].ScalefacScale = _stream.GetBits(1);
                _si.ch[ch].gr[0].Count1tableSelect = _stream.GetBits(1);
            }
        }
    }

    /**
     *
     */
    private void GetScaleFactors(int ch, int gr)
    {
        int sfb, window;
        GrInfo gr_info = _si.ch[ch].gr[gr];
        int scale_comp = gr_info.ScalefacCompress;
        int length0 = s_slen[0, scale_comp];
        int length1 = s_slen[1, scale_comp];

        if (gr_info.WindowSwitchingFlag != 0 && gr_info.BlockType == 2)
        {
            if (gr_info.MixedBlockFlag != 0)
            { // MIXED
                for (sfb = 0; sfb < 8; sfb++)
                    _scalefac[ch].l[sfb] = _br.Getbits(
                            s_slen[0, gr_info.ScalefacCompress]);
                for (sfb = 3; sfb < 6; sfb++)
                    for (window = 0; window < 3; window++)
                        _scalefac[ch].s[window, sfb] = _br.Getbits(
                                s_slen[0, gr_info.ScalefacCompress]);
                for (sfb = 6; sfb < 12; sfb++)
                    for (window = 0; window < 3; window++)
                        _scalefac[ch].s[window, sfb] = _br.Getbits(
                                s_slen[1, gr_info.ScalefacCompress]);
                for (sfb = 12, window = 0; window < 3; window++)
                    _scalefac[ch].s[window, sfb] = 0;

            }
            else
            {  // SHORT

                _scalefac[ch].s[0, 0] = _br.Getbits(length0);
                _scalefac[ch].s[1, 0] = _br.Getbits(length0);
                _scalefac[ch].s[2, 0] = _br.Getbits(length0);
                _scalefac[ch].s[0, 1] = _br.Getbits(length0);
                _scalefac[ch].s[1, 1] = _br.Getbits(length0);
                _scalefac[ch].s[2, 1] = _br.Getbits(length0);
                _scalefac[ch].s[0, 2] = _br.Getbits(length0);
                _scalefac[ch].s[1, 2] = _br.Getbits(length0);
                _scalefac[ch].s[2, 2] = _br.Getbits(length0);
                _scalefac[ch].s[0, 3] = _br.Getbits(length0);
                _scalefac[ch].s[1, 3] = _br.Getbits(length0);
                _scalefac[ch].s[2, 3] = _br.Getbits(length0);
                _scalefac[ch].s[0, 4] = _br.Getbits(length0);
                _scalefac[ch].s[1, 4] = _br.Getbits(length0);
                _scalefac[ch].s[2, 4] = _br.Getbits(length0);
                _scalefac[ch].s[0, 5] = _br.Getbits(length0);
                _scalefac[ch].s[1, 5] = _br.Getbits(length0);
                _scalefac[ch].s[2, 5] = _br.Getbits(length0);
                _scalefac[ch].s[0, 6] = _br.Getbits(length1);
                _scalefac[ch].s[1, 6] = _br.Getbits(length1);
                _scalefac[ch].s[2, 6] = _br.Getbits(length1);
                _scalefac[ch].s[0, 7] = _br.Getbits(length1);
                _scalefac[ch].s[1, 7] = _br.Getbits(length1);
                _scalefac[ch].s[2, 7] = _br.Getbits(length1);
                _scalefac[ch].s[0, 8] = _br.Getbits(length1);
                _scalefac[ch].s[1, 8] = _br.Getbits(length1);
                _scalefac[ch].s[2, 8] = _br.Getbits(length1);
                _scalefac[ch].s[0, 9] = _br.Getbits(length1);
                _scalefac[ch].s[1, 9] = _br.Getbits(length1);
                _scalefac[ch].s[2, 9] = _br.Getbits(length1);
                _scalefac[ch].s[0, 10] = _br.Getbits(length1);
                _scalefac[ch].s[1, 10] = _br.Getbits(length1);
                _scalefac[ch].s[2, 10] = _br.Getbits(length1);
                _scalefac[ch].s[0, 11] = _br.Getbits(length1);
                _scalefac[ch].s[1, 11] = _br.Getbits(length1);
                _scalefac[ch].s[2, 11] = _br.Getbits(length1);
                _scalefac[ch].s[0, 12] = 0;
                _scalefac[ch].s[1, 12] = 0;
                _scalefac[ch].s[2, 12] = 0;
            } // SHORT

        }
        else
        {   // LONG types 0,1,3

            if (_si.ch[ch].scfsi[0] == 0 || gr == 0)
            {
                _scalefac[ch].l[0] = _br.Getbits(length0);
                _scalefac[ch].l[1] = _br.Getbits(length0);
                _scalefac[ch].l[2] = _br.Getbits(length0);
                _scalefac[ch].l[3] = _br.Getbits(length0);
                _scalefac[ch].l[4] = _br.Getbits(length0);
                _scalefac[ch].l[5] = _br.Getbits(length0);
            }
            if (_si.ch[ch].scfsi[1] == 0 || gr == 0)
            {
                _scalefac[ch].l[6] = _br.Getbits(length0);
                _scalefac[ch].l[7] = _br.Getbits(length0);
                _scalefac[ch].l[8] = _br.Getbits(length0);
                _scalefac[ch].l[9] = _br.Getbits(length0);
                _scalefac[ch].l[10] = _br.Getbits(length0);
            }
            if (_si.ch[ch].scfsi[2] == 0 || gr == 0)
            {
                _scalefac[ch].l[11] = _br.Getbits(length1);
                _scalefac[ch].l[12] = _br.Getbits(length1);
                _scalefac[ch].l[13] = _br.Getbits(length1);
                _scalefac[ch].l[14] = _br.Getbits(length1);
                _scalefac[ch].l[15] = _br.Getbits(length1);
            }
            if (_si.ch[ch].scfsi[3] == 0 || gr == 0)
            {
                _scalefac[ch].l[16] = _br.Getbits(length1);
                _scalefac[ch].l[17] = _br.Getbits(length1);
                _scalefac[ch].l[18] = _br.Getbits(length1);
                _scalefac[ch].l[19] = _br.Getbits(length1);
                _scalefac[ch].l[20] = _br.Getbits(length1);
            }

            _scalefac[ch].l[21] = 0;
            _scalefac[ch].l[22] = 0;
        }
    }

    private void GetLSFScaleData(int ch, int gr)
    {

        int scalefac_comp, int_scalefac_comp;
        int mode_ext = _header.ModeExtension;
        int m;
        int blocktypenumber;
        int blocknumber = 0;

        GrInfo gr_info = _si.ch[ch].gr[gr];

        scalefac_comp = gr_info.ScalefacCompress;

        if (gr_info.BlockType == 2)
        {
            if (gr_info.MixedBlockFlag == 0)
                blocktypenumber = 1;
            else if (gr_info.MixedBlockFlag == 1)
                blocktypenumber = 2;
            else
                blocktypenumber = 0;
        }
        else
        {
            blocktypenumber = 0;
        }

        if (!((mode_ext == 1 || mode_ext == 3) && ch == 1))
        {

            if (scalefac_comp < 400)
            {

                _newSlen[0] = (scalefac_comp >>> 4) / 5;
                _newSlen[1] = (scalefac_comp >>> 4) % 5;
                _newSlen[2] = (scalefac_comp & 0xF) >>> 2;
                _newSlen[3] = scalefac_comp & 3;
                _si.ch[ch].gr[gr].Preflag = 0;
                blocknumber = 0;

            }
            else if (scalefac_comp < 500)
            {

                _newSlen[0] = ((scalefac_comp - 400) >>> 2) / 5;
                _newSlen[1] = ((scalefac_comp - 400) >>> 2) % 5;
                _newSlen[2] = scalefac_comp - 400 & 3;
                _newSlen[3] = 0;
                _si.ch[ch].gr[gr].Preflag = 0;
                blocknumber = 1;

            }
            else if (scalefac_comp < 512)
            {

                _newSlen[0] = (scalefac_comp - 500) / 3;
                _newSlen[1] = (scalefac_comp - 500) % 3;
                _newSlen[2] = 0;
                _newSlen[3] = 0;
                _si.ch[ch].gr[gr].Preflag = 1;
                blocknumber = 2;
            }
        }

        if ((mode_ext == 1 || mode_ext == 3) && ch == 1)
        {
            int_scalefac_comp = scalefac_comp >>> 1;

            if (int_scalefac_comp < 180)
            {
                _newSlen[0] = int_scalefac_comp / 36;
                _newSlen[1] = int_scalefac_comp % 36 / 6;
                _newSlen[2] = int_scalefac_comp % 36 % 6;
                _newSlen[3] = 0;
                _si.ch[ch].gr[gr].Preflag = 0;
                blocknumber = 3;
            }
            else if (int_scalefac_comp < 244)
            {
                _newSlen[0] = (int_scalefac_comp - 180 & 0x3F) >>> 4;
                _newSlen[1] = (int_scalefac_comp - 180 & 0xF) >>> 2;
                _newSlen[2] = int_scalefac_comp - 180 & 3;
                _newSlen[3] = 0;
                _si.ch[ch].gr[gr].Preflag = 0;
                blocknumber = 4;
            }
            else if (int_scalefac_comp < 255)
            {
                _newSlen[0] = (int_scalefac_comp - 244) / 3;
                _newSlen[1] = (int_scalefac_comp - 244) % 3;
                _newSlen[2] = 0;
                _newSlen[3] = 0;
                _si.ch[ch].gr[gr].Preflag = 0;
                blocknumber = 5;
            }
        }

        for (int x = 0; x < 45; x++) // why 45, not 54?
            _scalefacBuffer[x] = 0;

        m = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < NrOfSfbBlock[blocknumber, blocktypenumber, i];
                 j++)
            {
                _scalefacBuffer[m] = _newSlen[i] == 0 ? 0 :
                        _br.Getbits(_newSlen[i]);
                m++;

            } // for (unint32 j ...
        } // for (uint32 i ...
    }

    private void GetLSFScaleFactors(int ch, int gr)
    {
        int m = 0;
        int sfb, window;
        GrInfo gr_info = _si.ch[ch].gr[gr];

        GetLSFScaleData(ch, gr);

        if (gr_info.WindowSwitchingFlag != 0 && gr_info.BlockType == 2)
        {
            if (gr_info.MixedBlockFlag != 0)
            {    // MIXED
                for (sfb = 0; sfb < 8; sfb++)
                {
                    _scalefac[ch].l[sfb] = _scalefacBuffer[m];
                    m++;
                }
                for (sfb = 3; sfb < 12; sfb++)
                {
                    for (window = 0; window < 3; window++)
                    {
                        _scalefac[ch].s[window, sfb] = _scalefacBuffer[m];
                        m++;
                    }
                }
            }
            else
            {
                for (sfb = 0; sfb < 12; sfb++)
                {
                    for (window = 0; window < 3; window++)
                    {
                        _scalefac[ch].s[window, sfb] = _scalefacBuffer[m];
                        m++;
                    }
                }
            }
            for (window = 0; window < 3; window++)
                _scalefac[ch].s[window, 12] = 0;
        }
        else
        {   // LONG types 0,1,3
            for (sfb = 0; sfb < 21; sfb++)
            {
                _scalefac[ch].l[sfb] = _scalefacBuffer[m];
                m++;
            }
            _scalefac[ch].l[21] = 0; // Jeff
            _scalefac[ch].l[22] = 0;
        }
    }

    private void HuffmanDecode(int ch, int gr)
    {
        _x[0] = 0;
        _y[0] = 0;
        _v[0] = 0;
        _w[0] = 0;

        int part2_3_end = _part2Start + _si.ch[ch].gr[gr].Part2_3Length;
        int num_bits;
        int region1Start;
        int region2Start;
        int index;

        int buf, buf1;

        HuffcodeTabel h;

        // Find region boundary for short block case

        if (_si.ch[ch].gr[gr].WindowSwitchingFlag != 0 &&
                _si.ch[ch].gr[gr].BlockType == 2)
        {

            // Region2.
            //MS: Extrahandling for 8KHZ
            region1Start = _sfreq == 8 ? 72 : 36;  // sfb[9/3]*3=36 or in case 8KHZ = 72
            region2Start = 576; // No Region2 for short block case

        }
        else
        {          // Find region boundary for long block case

            buf = _si.ch[ch].gr[gr].Region0Count + 1;
            buf1 = buf + _si.ch[ch].gr[gr].Region1Count + 1;

            if (buf1 > _sfBandIndex[_sfreq].l.Length - 1) buf1 = _sfBandIndex[_sfreq].l.Length - 1;

            region1Start = _sfBandIndex[_sfreq].l[buf];
            region2Start = _sfBandIndex[_sfreq].l[buf1]; /* MI */
        }

        index = 0;
        // Read bigvalues area
        for (int i = 0; i < _si.ch[ch].gr[gr].BigValues << 1; i += 2)
        {
            if (i < region1Start) h = HuffcodeTabel.HT[_si.ch[ch].gr[gr].TableSelect[0]];
            else if (i < region2Start) h = HuffcodeTabel.HT[_si.ch[ch].gr[gr].TableSelect[1]];
            else h = HuffcodeTabel.HT[_si.ch[ch].gr[gr].TableSelect[2]];

            HuffcodeTabel.HuffmanDecoder(h, _x, _y, _v, _w, _br);

            _is1d[index++] = _x[0];
            _is1d[index++] = _y[0];

            _checkSumHuff = _checkSumHuff + _x[0] + _y[0];
        }

        // Read count1 area
        h = HuffcodeTabel.HT[_si.ch[ch].gr[gr].Count1tableSelect + 32];
        num_bits = _br.Hsstell;

        while (num_bits < part2_3_end && index < 576)
        {

            HuffcodeTabel.HuffmanDecoder(h, _x, _y, _v, _w, _br);

            _is1d[index++] = _v[0];
            _is1d[index++] = _w[0];
            _is1d[index++] = _x[0];
            _is1d[index++] = _y[0];
            _checkSumHuff = _checkSumHuff + _v[0] + _w[0] + _x[0] + _y[0];
            num_bits = _br.Hsstell;
        }

        if (num_bits > part2_3_end)
        {
            _br.RewindNbits(num_bits - part2_3_end);
            index -= 4;
        }

        num_bits = _br.Hsstell;

        // Dismiss stuffing bits
        if (num_bits < part2_3_end)
            _br.Getbits(part2_3_end - num_bits);

        // Zero out rest

        _nonzero[ch] = Math.Min(index, 576);

        if (index < 0) index = 0;

        // may not be necessary
        for (; index < 576; index++)
            _is1d[index] = 0;
    }

    /**
     *
     */
    private void IStereoKValues(int is_pos, int io_type, int i)
    {
        if (is_pos == 0)
        {
            _k[0, i] = 1.0f;
            _k[1, i] = 1.0f;
        }
        else if ((is_pos & 1) != 0)
        {
            _k[0, i] = Io[io_type, (is_pos + 1) >>> 1];
            _k[1, i] = 1.0f;
        }
        else
        {
            _k[0, i] = 1.0f;
            _k[1, i] = Io[io_type, is_pos >>> 1];
        }
    }

    private void DequantizeSample(float[,] xr, int ch, int gr)
    {
        GrInfo gr_info = _si.ch[ch].gr[gr];
        int cb = 0;
        int next_cb_boundary;
        int cb_begin = 0;
        int cb_width = 0;
        int index = 0, t_index, j;
        float g_gain;

        // choose correct scalefactor band per block type, initalize boundary

        if (gr_info.WindowSwitchingFlag != 0 && gr_info.BlockType == 2)
        {
            if (gr_info.MixedBlockFlag != 0)
                next_cb_boundary = _sfBandIndex[_sfreq].l[1];  // LONG blocks: 0,1,3
            else
            {
                cb_width = _sfBandIndex[_sfreq].s[1];
                next_cb_boundary = (cb_width << 2) - cb_width;
                cb_begin = 0;
            }
        }
        else
        {
            next_cb_boundary = _sfBandIndex[_sfreq].l[1];  // LONG blocks: 0,1,3
        }

        // Compute overall (global) scaling.

        g_gain = (float)Math.Pow(2.0, 0.25 * (gr_info.GlobalGain - 210.0));

        for (j = 0; j < _nonzero[ch]; j++)
        {
            // Modif E.B 02/22/99
            int reste = j % s_sslimit;
            int quotien = (j - reste) / s_sslimit;
            if (_is1d[j] == 0) xr[quotien, reste] = 0.0f;
            else
            {
                int abv = _is1d[j];
                // Pow Array fix (11/17/04)
                if (abv < T43.Length)
                {
                    if (_is1d[j] > 0) xr[quotien, reste] = g_gain * T43[abv];
                    else
                    {
                        if (-abv < T43.Length) xr[quotien, reste] = -g_gain * T43[-abv];
                        else xr[quotien, reste] = -g_gain * (float)Math.Pow(-abv, _d43);
                    }
                }
                else
                {
                    if (_is1d[j] > 0) xr[quotien, reste] = g_gain * (float)Math.Pow(abv, _d43);
                    else xr[quotien, reste] = -g_gain * (float)Math.Pow(-abv, _d43);
                }
            }
        }

        // apply formula per block type
        for (j = 0; j < _nonzero[ch]; j++)
        {
            // Modif E.B 02/22/99
            int reste = j % s_sslimit;
            int quotien = (j - reste) / s_sslimit;

            if (index == next_cb_boundary)
            { /* Adjust critical band boundary */
                if (gr_info.WindowSwitchingFlag != 0 && gr_info.BlockType == 2)
                {
                    if (gr_info.MixedBlockFlag != 0)
                    {

                        if (index == _sfBandIndex[_sfreq].l[8])
                        {
                            next_cb_boundary = _sfBandIndex[_sfreq].s[4];
                            next_cb_boundary = (next_cb_boundary << 2) -
                                    next_cb_boundary;
                            cb = 3;
                            cb_width = _sfBandIndex[_sfreq].s[4] -
                                    _sfBandIndex[_sfreq].s[3];

                            cb_begin = _sfBandIndex[_sfreq].s[3];
                            cb_begin = (cb_begin << 2) - cb_begin;

                        }
                        else if (index < _sfBandIndex[_sfreq].l[8])
                        {

                            next_cb_boundary = _sfBandIndex[_sfreq].l[++cb + 1];

                        }
                        else
                        {

                            next_cb_boundary = _sfBandIndex[_sfreq].s[++cb + 1];
                            next_cb_boundary = (next_cb_boundary << 2) -
                                    next_cb_boundary;

                            cb_begin = _sfBandIndex[_sfreq].s[cb];
                            cb_width = _sfBandIndex[_sfreq].s[cb + 1] -
                                    cb_begin;
                            cb_begin = (cb_begin << 2) - cb_begin;
                        }

                    }
                    else
                    {

                        next_cb_boundary = _sfBandIndex[_sfreq].s[++cb + 1];
                        next_cb_boundary = (next_cb_boundary << 2) -
                                next_cb_boundary;

                        cb_begin = _sfBandIndex[_sfreq].s[cb];
                        cb_width = _sfBandIndex[_sfreq].s[cb + 1] -
                                cb_begin;
                        cb_begin = (cb_begin << 2) - cb_begin;
                    }

                }
                else
                { // long blocks

                    next_cb_boundary = _sfBandIndex[_sfreq].l[++cb + 1];

                }
            }

            // Do long/short dependent scaling operations

            if (gr_info.WindowSwitchingFlag != 0 && (gr_info.BlockType == 2
                    && gr_info.MixedBlockFlag == 0 || gr_info.BlockType == 2 && j >= 36))
            {

                t_index = (index - cb_begin) / cb_width;
                int idx = _scalefac[ch].s[t_index, cb]
                        << gr_info.ScalefacScale;
                idx += gr_info.SubblockGain[t_index] << 2;

                xr[quotien, reste] *= TwoToNegativeHalfPow[idx];

            }
            else
            {
                int idx = _scalefac[ch].l[cb];

                if (gr_info.Preflag != 0)
                    idx += Pretab[cb];

                idx <<= gr_info.ScalefacScale;
                xr[quotien, reste] *= TwoToNegativeHalfPow[idx];
            }
            index++;
        }

        for (j = _nonzero[ch]; j < 576; j++)
        {
            // Modif E.B 02/22/99
            int reste = j % s_sslimit;
            int quotien = (j - reste) / s_sslimit;
            if (reste < 0) reste = 0;
            if (quotien < 0) quotien = 0;
            xr[quotien, reste] = 0.0f;
        }
    }

    /**
     *
     */
    private void Reorder(float[,] xr, int ch, int gr)
    {
        GrInfo gr_info = _si.ch[ch].gr[gr];
        int freq, freq3;
        int index;
        int sfb, sfb_start, sfb_lines;
        int src_line, des_line;

        if (gr_info.WindowSwitchingFlag != 0 && gr_info.BlockType == 2)
        {

            for (index = 0; index < 576; index++)
                _out1d[index] = 0.0f;

            if (gr_info.MixedBlockFlag != 0)
            {
                // NO REORDER FOR LOW 2 SUBBANDS
                for (index = 0; index < 36; index++)
                {
                    // Modif E.B 02/22/99
                    int reste = index % s_sslimit;
                    int quotien = (index - reste) / s_sslimit;
                    _out1d[index] = xr[quotien, reste];
                }
                for (sfb = 3; sfb < 13; sfb++)
                {
                    sfb_start = _sfBandIndex[_sfreq].s[sfb];
                    sfb_lines = _sfBandIndex[_sfreq].s[sfb + 1] - sfb_start;

                    int sfb_start3 = (sfb_start << 2) - sfb_start;

                    for (freq = 0, freq3 = 0; freq < sfb_lines;
                         freq++, freq3 += 3)
                    {
                        src_line = sfb_start3 + freq;
                        des_line = sfb_start3 + freq3;
                        // Modif E.B 02/22/99
                        int reste = src_line % s_sslimit;
                        int quotien = (src_line - reste) / s_sslimit;

                        _out1d[des_line] = xr[quotien, reste];
                        src_line += sfb_lines;
                        des_line++;

                        reste = src_line % s_sslimit;
                        quotien = (src_line - reste) / s_sslimit;

                        _out1d[des_line] = xr[quotien, reste];
                        src_line += sfb_lines;
                        des_line++;

                        reste = src_line % s_sslimit;
                        quotien = (src_line - reste) / s_sslimit;

                        _out1d[des_line] = xr[quotien, reste];
                    }
                }

            }
            else
            {  // pure short
                for (index = 0; index < 576; index++)
                {
                    int j = s_reorderTable[_sfreq][index];
                    int reste = j % s_sslimit;
                    int quotien = (j - reste) / s_sslimit;
                    _out1d[index] = xr[quotien, reste];
                }
            }
        }
        else
        {   // long blocks
            for (index = 0; index < 576; index++)
            {
                // Modif E.B 02/22/99
                int reste = index % s_sslimit;
                int quotien = (index - reste) / s_sslimit;
                _out1d[index] = xr[quotien, reste];
            }
        }
    }

    private void Stereo(int gr)
    {
        int sb, ss;

        if (_channels == 1)
        {

            for (sb = 0; sb < s_sblimit; sb++)
                for (ss = 0; ss < s_sslimit; ss += 3)
                {
                    _lr[0][sb, ss] = _ro[0][sb, ss];
                    _lr[0][sb, ss + 1] = _ro[0][sb, ss + 1];
                    _lr[0][sb, ss + 2] = _ro[0][sb, ss + 2];
                }

        }
        else
        {
            GrInfo gr_info = _si.ch[0].gr[gr];
            int mode_ext = _header.ModeExtension;
            int sfb;
            int i;
            int lines, temp, temp2;

            bool ms_stereo = _header.Mode == ChannelType.JointStereo && (mode_ext & 0x2) != 0;
            bool i_stereo = _header.Mode == ChannelType.JointStereo && (mode_ext & 0x1) != 0;
            bool lsf = _header.Version == VersionType.Mpeg2LSF || _header.Version == VersionType.Mpeg25LSF;    // SZD

            int io_type = gr_info.ScalefacCompress & 1;

            // initialization

            for (i = 0; i < 576; i++)
            {
                _isPos[i] = 7;

                _isRatio[i] = 0.0f;
            }

            if (i_stereo)
            {
                if (gr_info.WindowSwitchingFlag != 0 && gr_info.BlockType == 2)
                {
                    if (gr_info.MixedBlockFlag != 0)
                    {

                        int max_sfb = 0;

                        for (int j = 0; j < 3; j++)
                        {
                            int sfbcnt;
                            sfbcnt = 2;
                            for (sfb = 12; sfb >= 3; sfb--)
                            {
                                i = _sfBandIndex[_sfreq].s[sfb];
                                lines = _sfBandIndex[_sfreq].s[sfb + 1] - i;
                                i = (i << 2) - i + (j + 1) * lines - 1;

                                while (lines > 0)
                                {
                                    if (_ro[1][i / 18, i % 18] != 0.0f)
                                    {
                                        sfbcnt = sfb;
                                        sfb = -10;
                                        lines = -10;
                                    }
                                    lines--;
                                    i--;
                                }
                            }
                            sfb = sfbcnt + 1;

                            if (sfb > max_sfb)
                                max_sfb = sfb;

                            while (sfb < 12)
                            {
                                temp = _sfBandIndex[_sfreq].s[sfb];
                                sb = _sfBandIndex[_sfreq].s[sfb + 1] - temp;
                                i = (temp << 2) - temp + j * sb;

                                for (; sb > 0; sb--)
                                {
                                    _isPos[i] = _scalefac[1].s[j, sfb];
                                    if (_isPos[i] != 7)
                                        if (lsf)
                                            IStereoKValues(_isPos[i], io_type, i);
                                        else
                                            _isRatio[i] = TAN12[_isPos[i]];
                                    i++;
                                }
                                sfb++;
                            }
                            sfb = _sfBandIndex[_sfreq].s[10];
                            sb = _sfBandIndex[_sfreq].s[11] - sfb;
                            sfb = (sfb << 2) - sfb + j * sb;
                            temp = _sfBandIndex[_sfreq].s[11];
                            sb = _sfBandIndex[_sfreq].s[12] - temp;
                            i = (temp << 2) - temp + j * sb;

                            for (; sb > 0; sb--)
                            {
                                _isPos[i] = _isPos[sfb];

                                if (lsf)
                                {
                                    _k[0, i] = _k[0, sfb];
                                    _k[1, i] = _k[1, sfb];
                                }
                                else
                                {
                                    _isRatio[i] = _isRatio[sfb];
                                }
                                i++;
                            }
                        }
                        if (max_sfb <= 3)
                        {
                            i = 2;
                            ss = 17;
                            sb = -1;
                            while (i >= 0)
                            {
                                if (_ro[1][i, ss] != 0.0f)
                                {
                                    sb = (i << 4) + (i << 1) + ss;
                                    i = -1;
                                }
                                else
                                {
                                    ss--;
                                    if (ss < 0)
                                    {
                                        i--;
                                        ss = 17;
                                    }
                                } // if (ro ...
                            } // while (i>=0)
                            i = 0;
                            while (_sfBandIndex[_sfreq].l[i] <= sb)
                                i++;
                            sfb = i;
                            i = _sfBandIndex[_sfreq].l[i];
                            for (; sfb < 8; sfb++)
                            {
                                sb = _sfBandIndex[_sfreq].l[sfb + 1] - _sfBandIndex[_sfreq].l[sfb];
                                for (; sb > 0; sb--)
                                {
                                    _isPos[i] = _scalefac[1].l[sfb];
                                    if (_isPos[i] != 7)
                                        if (lsf)
                                            IStereoKValues(_isPos[i], io_type, i);
                                        else
                                            _isRatio[i] = TAN12[_isPos[i]];
                                    i++;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            int sfbcnt;
                            sfbcnt = -1;
                            for (sfb = 12; sfb >= 0; sfb--)
                            {
                                temp = _sfBandIndex[_sfreq].s[sfb];
                                lines = _sfBandIndex[_sfreq].s[sfb + 1] - temp;
                                i = (temp << 2) - temp + (j + 1) * lines - 1;

                                while (lines > 0)
                                {
                                    if (_ro[1][i / 18, i % 18] != 0.0f)
                                    {
                                        sfbcnt = sfb;
                                        sfb = -10;
                                        lines = -10;
                                    }
                                    lines--;
                                    i--;
                                }

                            }
                            sfb = sfbcnt + 1;
                            while (sfb < 12)
                            {
                                temp = _sfBandIndex[_sfreq].s[sfb];
                                sb = _sfBandIndex[_sfreq].s[sfb + 1] - temp;
                                i = (temp << 2) - temp + j * sb;
                                for (; sb > 0; sb--)
                                {
                                    _isPos[i] = _scalefac[1].s[j, sfb];
                                    if (_isPos[i] != 7)
                                        if (lsf)
                                            IStereoKValues(_isPos[i], io_type, i);
                                        else
                                            _isRatio[i] = TAN12[_isPos[i]];
                                    i++;
                                } // for (; sb>0 ...
                                sfb++;
                            } // while (sfb<12)

                            temp = _sfBandIndex[_sfreq].s[10];
                            temp2 = _sfBandIndex[_sfreq].s[11];
                            sb = temp2 - temp;
                            sfb = (temp << 2) - temp + j * sb;
                            sb = _sfBandIndex[_sfreq].s[12] - temp2;
                            i = (temp2 << 2) - temp2 + j * sb;

                            for (; sb > 0; sb--)
                            {
                                _isPos[i] = _isPos[sfb];

                                if (lsf)
                                {
                                    _k[0, i] = _k[0, sfb];
                                    _k[1, i] = _k[1, sfb];
                                }
                                else
                                {
                                    _isRatio[i] = _isRatio[sfb];
                                }
                                i++;
                            } // for (; sb>0 ...
                        } // for (sfb=12
                    } // for (j=0 ...
                }
                else
                { // if (gr_info.window_switching_flag ...
                    i = 31;
                    ss = 17;
                    sb = 0;
                    while (i >= 0)
                    {
                        if (_ro[1][i, ss] != 0.0f)
                        {
                            sb = (i << 4) + (i << 1) + ss;
                            i = -1;
                        }
                        else
                        {
                            ss--;
                            if (ss < 0)
                            {
                                i--;
                                ss = 17;
                            }
                        }
                    }
                    i = 0;
                    while (_sfBandIndex[_sfreq].l[i] <= sb)
                        i++;

                    sfb = i;
                    i = _sfBandIndex[_sfreq].l[i];
                    for (; sfb < 21; sfb++)
                    {
                        sb = _sfBandIndex[_sfreq].l[sfb + 1] - _sfBandIndex[_sfreq].l[sfb];
                        for (; sb > 0; sb--)
                        {
                            _isPos[i] = _scalefac[1].l[sfb];
                            if (_isPos[i] != 7)
                                if (lsf)
                                    IStereoKValues(_isPos[i], io_type, i);
                                else
                                    _isRatio[i] = TAN12[_isPos[i]];
                            i++;
                        }
                    }
                    sfb = _sfBandIndex[_sfreq].l[20];
                    for (sb = 576 - _sfBandIndex[_sfreq].l[21]; sb > 0 && i < 576; sb--)
                    {
                        _isPos[i] = _isPos[sfb]; // error here : i >=576

                        if (lsf)
                        {
                            _k[0, i] = _k[0, sfb];
                            _k[1, i] = _k[1, sfb];
                        }
                        else
                        {
                            _isRatio[i] = _isRatio[sfb];
                        }
                        i++;
                    } // if (gr_info.mixed_block_flag)
                } // if (gr_info.window_switching_flag ...
            } // if (i_stereo)

            i = 0;
            for (sb = 0; sb < s_sblimit; sb++)
                for (ss = 0; ss < s_sslimit; ss++)
                {
                    if (_isPos[i] == 7)
                    {
                        if (ms_stereo)
                        {
                            _lr[0][sb, ss] = (_ro[0][sb, ss] + _ro[1][sb, ss]) * 0.707106781f;
                            _lr[1][sb, ss] = (_ro[0][sb, ss] - _ro[1][sb, ss]) * 0.707106781f;
                        }
                        else
                        {
                            _lr[0][sb, ss] = _ro[0][sb, ss];
                            _lr[1][sb, ss] = _ro[1][sb, ss];
                        }
                    }
                    else if (i_stereo)
                    {

                        if (lsf)
                        {
                            _lr[0][sb, ss] = _ro[0][sb, ss] * _k[0, i];
                            _lr[1][sb, ss] = _ro[0][sb, ss] * _k[1, i];
                        }
                        else
                        {
                            _lr[1][sb, ss] = _ro[0][sb, ss] / (1 + _isRatio[i]);
                            _lr[0][sb, ss] = _lr[1][sb, ss] * _isRatio[i];
                        }
                    }
                    /*				else {
                                        System.out.println("Error in stereo processing\n");
                                    } */
                    i++;
                }

        } // channels == 2

    }

    /**
     *
     */
    private void Antialias(int ch, int gr)
    {
        int sb18, ss, sb18lim;
        GrInfo gr_info = _si.ch[ch].gr[gr];
        // 31 alias-reduction operations between each pair of sub-bands
        // with 8 butterflies between each pair

        if (gr_info.WindowSwitchingFlag != 0 && gr_info.BlockType == 2 &&
                gr_info.MixedBlockFlag == 0)
            return;

        if (gr_info.WindowSwitchingFlag != 0 && gr_info.MixedBlockFlag != 0 &&
                gr_info.BlockType == 2)
        {
            sb18lim = 18;
        }
        else
        {
            sb18lim = 558;
        }

        for (sb18 = 0; sb18 < sb18lim; sb18 += 18)
        {
            for (ss = 0; ss < 8; ss++)
            {
                int src_idx1 = sb18 + 17 - ss;
                int src_idx2 = sb18 + 18 + ss;
                float bu = _out1d[src_idx1];
                float bd = _out1d[src_idx2];
                _out1d[src_idx1] = bu * s_cs[ss] - bd * s_ca[ss];
                _out1d[src_idx2] = bd * s_cs[ss] + bu * s_ca[ss];
            }
        }
    }

    private void Hybrid(int ch, int gr)
    {
        int bt;
        int sb18;
        GrInfo gr_info = _si.ch[ch].gr[gr];
        float[] tsOut;

        float[][] prvblk;

        for (sb18 = 0; sb18 < 576; sb18 += 18)
        {
            bt = gr_info.WindowSwitchingFlag != 0 && gr_info.MixedBlockFlag != 0 &&
                    sb18 < 36 ? 0 : gr_info.BlockType;

            tsOut = _out1d;
            // Modif E.B 02/22/99
            Array.Copy(tsOut, sb18, _tsOutCopy, 0, 18);

            InvMdct(_tsOutCopy, _rawout, bt);


            Array.Copy(_tsOutCopy, 0, tsOut, sb18, 18);
            // Fin Modif

            // overlap addition
            prvblk = _prevblck;

            tsOut[sb18] = _rawout[0] + prvblk[ch][sb18];
            prvblk[ch][sb18] = _rawout[18];
            tsOut[1 + sb18] = _rawout[1] + prvblk[ch][sb18 + 1];
            prvblk[ch][sb18 + 1] = _rawout[19];
            tsOut[2 + sb18] = _rawout[2] + prvblk[ch][sb18 + 2];
            prvblk[ch][sb18 + 2] = _rawout[20];
            tsOut[3 + sb18] = _rawout[3] + prvblk[ch][sb18 + 3];
            prvblk[ch][sb18 + 3] = _rawout[21];
            tsOut[4 + sb18] = _rawout[4] + prvblk[ch][sb18 + 4];
            prvblk[ch][sb18 + 4] = _rawout[22];
            tsOut[5 + sb18] = _rawout[5] + prvblk[ch][sb18 + 5];
            prvblk[ch][sb18 + 5] = _rawout[23];
            tsOut[6 + sb18] = _rawout[6] + prvblk[ch][sb18 + 6];
            prvblk[ch][sb18 + 6] = _rawout[24];
            tsOut[7 + sb18] = _rawout[7] + prvblk[ch][sb18 + 7];
            prvblk[ch][sb18 + 7] = _rawout[25];
            tsOut[8 + sb18] = _rawout[8] + prvblk[ch][sb18 + 8];
            prvblk[ch][sb18 + 8] = _rawout[26];
            tsOut[9 + sb18] = _rawout[9] + prvblk[ch][sb18 + 9];
            prvblk[ch][sb18 + 9] = _rawout[27];
            tsOut[10 + sb18] = _rawout[10] + prvblk[ch][sb18 + 10];
            prvblk[ch][sb18 + 10] = _rawout[28];
            tsOut[11 + sb18] = _rawout[11] + prvblk[ch][sb18 + 11];
            prvblk[ch][sb18 + 11] = _rawout[29];
            tsOut[12 + sb18] = _rawout[12] + prvblk[ch][sb18 + 12];
            prvblk[ch][sb18 + 12] = _rawout[30];
            tsOut[13 + sb18] = _rawout[13] + prvblk[ch][sb18 + 13];
            prvblk[ch][sb18 + 13] = _rawout[31];
            tsOut[14 + sb18] = _rawout[14] + prvblk[ch][sb18 + 14];
            prvblk[ch][sb18 + 14] = _rawout[32];
            tsOut[15 + sb18] = _rawout[15] + prvblk[ch][sb18 + 15];
            prvblk[ch][sb18 + 15] = _rawout[33];
            tsOut[16 + sb18] = _rawout[16] + prvblk[ch][sb18 + 16];
            prvblk[ch][sb18 + 16] = _rawout[34];
            tsOut[17 + sb18] = _rawout[17] + prvblk[ch][sb18 + 17];
            prvblk[ch][sb18 + 17] = _rawout[35];
        }
    }

    /// <summary>
    /// Fast INV_MDCT.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <param name="block_type"></param>
    public static void InvMdct(float[] input, float[] output, int block_type)
    {
        float[] win_bt;
        int i;

        float tmpf_0, tmpf_1, tmpf_2, tmpf_3, tmpf_4, tmpf_5, tmpf_6, tmpf_7, tmpf_8, tmpf_9;
        float tmpf_10, tmpf_11, tmpf_12, tmpf_13, tmpf_14, tmpf_15, tmpf_16, tmpf_17;

        if (block_type == 2)
        {
            output[0] = 0.0f;
            output[1] = 0.0f;
            output[2] = 0.0f;
            output[3] = 0.0f;
            output[4] = 0.0f;
            output[5] = 0.0f;
            output[6] = 0.0f;
            output[7] = 0.0f;
            output[8] = 0.0f;
            output[9] = 0.0f;
            output[10] = 0.0f;
            output[11] = 0.0f;
            output[12] = 0.0f;
            output[13] = 0.0f;
            output[14] = 0.0f;
            output[15] = 0.0f;
            output[16] = 0.0f;
            output[17] = 0.0f;
            output[18] = 0.0f;
            output[19] = 0.0f;
            output[20] = 0.0f;
            output[21] = 0.0f;
            output[22] = 0.0f;
            output[23] = 0.0f;
            output[24] = 0.0f;
            output[25] = 0.0f;
            output[26] = 0.0f;
            output[27] = 0.0f;
            output[28] = 0.0f;
            output[29] = 0.0f;
            output[30] = 0.0f;
            output[31] = 0.0f;
            output[32] = 0.0f;
            output[33] = 0.0f;
            output[34] = 0.0f;
            output[35] = 0.0f;

            int six_i = 0;

            for (i = 0; i < 3; i++)
            {
                // 12 point IMDCT
                // Begin 12 point IDCT
                // Input aliasing for 12 pt IDCT
                input[15 + i] += input[12 + i];
                input[12 + i] += input[9 + i];
                input[9 + i] += input[6 + i];
                input[6 + i] += input[3 + i];
                input[3 + i] += input[i];

                // Input aliasing on odd indices (for 6 point IDCT)
                input[15 + i] += input[9 + i];
                input[9 + i] += input[3 + i];

                // 3 point IDCT on even indices
                float pp1, pp2, sum;
                pp2 = input[12 + i] * 0.500000000f;
                pp1 = input[6 + i] * 0.866025403f;
                sum = input[i] + pp2;
                tmpf_1 = input[i] - input[12 + i];
                tmpf_0 = sum + pp1;
                tmpf_2 = sum - pp1;

                // End 3 point IDCT on even indices
                // 3 point IDCT on odd indices (for 6 point IDCT)
                pp2 = input[15 + i] * 0.500000000f;
                pp1 = input[9 + i] * 0.866025403f;
                sum = input[3 + i] + pp2;
                tmpf_4 = input[3 + i] - input[15 + i];
                tmpf_5 = sum + pp1;
                tmpf_3 = sum - pp1;
                // End 3 point IDCT on odd indices
                // Twiddle factors on odd indices (for 6 point IDCT)

                tmpf_3 *= 1.931851653f;
                tmpf_4 *= 0.707106781f;
                tmpf_5 *= 0.517638090f;

                // Output butterflies on 2 3 point IDCT's (for 6 point IDCT)
                float save = tmpf_0;
                tmpf_0 += tmpf_5;
                tmpf_5 = save - tmpf_5;
                save = tmpf_1;
                tmpf_1 += tmpf_4;
                tmpf_4 = save - tmpf_4;
                save = tmpf_2;
                tmpf_2 += tmpf_3;
                tmpf_3 = save - tmpf_3;

                // End 6 point IDCT
                // Twiddle factors on indices (for 12 point IDCT)

                tmpf_0 *= 0.504314480f;
                tmpf_1 *= 0.541196100f;
                tmpf_2 *= 0.630236207f;
                tmpf_3 *= 0.821339815f;
                tmpf_4 *= 1.306562965f;
                tmpf_5 *= 3.830648788f;

                // End 12 point IDCT

                // Shift to 12 point modified IDCT, multiply by window type 2
                tmpf_8 = -tmpf_0 * 0.793353340f;
                tmpf_9 = -tmpf_0 * 0.608761429f;
                tmpf_7 = -tmpf_1 * 0.923879532f;
                tmpf_10 = -tmpf_1 * 0.382683432f;
                tmpf_6 = -tmpf_2 * 0.991444861f;
                tmpf_11 = -tmpf_2 * 0.130526192f;

                tmpf_0 = tmpf_3;
                tmpf_1 = tmpf_4 * 0.382683432f;
                tmpf_2 = tmpf_5 * 0.608761429f;

                tmpf_3 = -tmpf_5 * 0.793353340f;
                tmpf_4 = -tmpf_4 * 0.923879532f;
                tmpf_5 = -tmpf_0 * 0.991444861f;

                tmpf_0 *= 0.130526192f;

                output[six_i + 6] += tmpf_0;
                output[six_i + 7] += tmpf_1;
                output[six_i + 8] += tmpf_2;
                output[six_i + 9] += tmpf_3;
                output[six_i + 10] += tmpf_4;
                output[six_i + 11] += tmpf_5;
                output[six_i + 12] += tmpf_6;
                output[six_i + 13] += tmpf_7;
                output[six_i + 14] += tmpf_8;
                output[six_i + 15] += tmpf_9;
                output[six_i + 16] += tmpf_10;
                output[six_i + 17] += tmpf_11;

                six_i += 6;
            }
        }
        else
        {
            // 36 point IDCT
            // input aliasing for 36 point IDCT
            input[17] += input[16];
            input[16] += input[15];
            input[15] += input[14];
            input[14] += input[13];
            input[13] += input[12];
            input[12] += input[11];
            input[11] += input[10];
            input[10] += input[9];
            input[9] += input[8];
            input[8] += input[7];
            input[7] += input[6];
            input[6] += input[5];
            input[5] += input[4];
            input[4] += input[3];
            input[3] += input[2];
            input[2] += input[1];
            input[1] += input[0];

            // 18 point IDCT for odd indices
            // input aliasing for 18 point IDCT
            input[17] += input[15];
            input[15] += input[13];
            input[13] += input[11];
            input[11] += input[9];
            input[9] += input[7];
            input[7] += input[5];
            input[5] += input[3];
            input[3] += input[1];

            float tmp0, tmp1, tmp2, tmp3, tmp4, tmp0_, tmp1_, tmp2_, tmp3_;
            float tmp0o, tmp1o, tmp2o, tmp3o, tmp4o, tmp0_o, tmp1_o, tmp2_o, tmp3_o;

            // Fast 9 Point Inverse Discrete Cosine Transform
            //
            // By  Francois-Raymond Boyer
            //         mailto:boyerf@iro.umontreal.ca
            //         http://www.iro.umontreal.ca/~boyerf
            //
            // The code has been optimized for Intel processors
            //  (takes a lot of time to convert float to and from iternal FPU representation)
            //
            // It is a simple "factorization" of the IDCT matrix.

            // 9 point IDCT on even indices

            // 5 points on odd indices (not realy an IDCT)
            float i00 = input[0] + input[0];
            float iip12 = i00 + input[12];

            tmp0 = iip12 + input[4] * 1.8793852415718f + input[8] * 1.532088886238f + input[16] * 0.34729635533386f;
            tmp1 = i00 + input[4] - input[8] - input[12] - input[12] - input[16];
            tmp2 = iip12 - input[4] * 0.34729635533386f - input[8] * 1.8793852415718f + input[16] * 1.532088886238f;
            tmp3 = iip12 - input[4] * 1.532088886238f + input[8] * 0.34729635533386f - input[16] * 1.8793852415718f;
            tmp4 = input[0] - input[4] + input[8] - input[12] + input[16];

            // 4 points on even indices
            float i66_ = input[6] * 1.732050808f;        // Sqrt[3]

            tmp0_ = input[2] * 1.9696155060244f + i66_ + input[10] * 1.2855752193731f + input[14] * 0.68404028665134f;
            tmp1_ = (input[2] - input[10] - input[14]) * 1.732050808f;
            tmp2_ = input[2] * 1.2855752193731f - i66_ - input[10] * 0.68404028665134f + input[14] * 1.9696155060244f;
            tmp3_ = input[2] * 0.68404028665134f - i66_ + input[10] * 1.9696155060244f - input[14] * 1.2855752193731f;

            // 9 point IDCT on odd indices
            // 5 points on odd indices (not realy an IDCT)
            float i0 = input[1] + input[1];
            float i0p12 = i0 + input[12 + 1];

            tmp0o = i0p12 + input[4 + 1] * 1.8793852415718f + input[8 + 1] * 1.532088886238f + input[16 + 1] * 0.34729635533386f;
            tmp1o = i0 + input[4 + 1] - input[8 + 1] - input[12 + 1] - input[12 + 1] - input[16 + 1];
            tmp2o = i0p12 - input[4 + 1] * 0.34729635533386f - input[8 + 1] * 1.8793852415718f + input[16 + 1] * 1.532088886238f;
            tmp3o = i0p12 - input[4 + 1] * 1.532088886238f + input[8 + 1] * 0.34729635533386f - input[16 + 1] * 1.8793852415718f;
            tmp4o = (input[1] - input[4 + 1] + input[8 + 1] - input[12 + 1] + input[16 + 1]) * 0.707106781f; // Twiddled

            // 4 points on even indices
            float i6_ = input[6 + 1] * 1.732050808f;        // Sqrt[3]

            tmp0_o = input[2 + 1] * 1.9696155060244f + i6_ + input[10 + 1] * 1.2855752193731f + input[14 + 1] * 0.68404028665134f;
            tmp1_o = (input[2 + 1] - input[10 + 1] - input[14 + 1]) * 1.732050808f;
            tmp2_o = input[2 + 1] * 1.2855752193731f - i6_ - input[10 + 1] * 0.68404028665134f + input[14 + 1] * 1.9696155060244f;
            tmp3_o = input[2 + 1] * 0.68404028665134f - i6_ + input[10 + 1] * 1.9696155060244f - input[14 + 1] * 1.2855752193731f;

            // Twiddle factors on odd indices
            // and
            // Butterflies on 9 point IDCT's
            // and
            // twiddle factors for 36 point IDCT

            float e, o;
            e = tmp0 + tmp0_;
            o = (tmp0o + tmp0_o) * 0.501909918f;
            tmpf_0 = e + o;
            tmpf_17 = e - o;
            e = tmp1 + tmp1_;
            o = (tmp1o + tmp1_o) * 0.517638090f;
            tmpf_1 = e + o;
            tmpf_16 = e - o;
            e = tmp2 + tmp2_;
            o = (tmp2o + tmp2_o) * 0.551688959f;
            tmpf_2 = e + o;
            tmpf_15 = e - o;
            e = tmp3 + tmp3_;
            o = (tmp3o + tmp3_o) * 0.610387294f;
            tmpf_3 = e + o;
            tmpf_14 = e - o;
            tmpf_4 = tmp4 + tmp4o;
            tmpf_13 = tmp4 - tmp4o;
            e = tmp3 - tmp3_;
            o = (tmp3o - tmp3_o) * 0.871723397f;
            tmpf_5 = e + o;
            tmpf_12 = e - o;
            e = tmp2 - tmp2_;
            o = (tmp2o - tmp2_o) * 1.183100792f;
            tmpf_6 = e + o;
            tmpf_11 = e - o;
            e = tmp1 - tmp1_;
            o = (tmp1o - tmp1_o) * 1.931851653f;
            tmpf_7 = e + o;
            tmpf_10 = e - o;
            e = tmp0 - tmp0_;
            o = (tmp0o - tmp0_o) * 5.736856623f;
            tmpf_8 = e + o;
            tmpf_9 = e - o;

            // end 36 point IDCT */
            // shift to modified IDCT
            win_bt = Win[block_type];

            output[0] = -tmpf_9 * win_bt[0];
            output[1] = -tmpf_10 * win_bt[1];
            output[2] = -tmpf_11 * win_bt[2];
            output[3] = -tmpf_12 * win_bt[3];
            output[4] = -tmpf_13 * win_bt[4];
            output[5] = -tmpf_14 * win_bt[5];
            output[6] = -tmpf_15 * win_bt[6];
            output[7] = -tmpf_16 * win_bt[7];
            output[8] = -tmpf_17 * win_bt[8];
            output[9] = tmpf_17 * win_bt[9];
            output[10] = tmpf_16 * win_bt[10];
            output[11] = tmpf_15 * win_bt[11];
            output[12] = tmpf_14 * win_bt[12];
            output[13] = tmpf_13 * win_bt[13];
            output[14] = tmpf_12 * win_bt[14];
            output[15] = tmpf_11 * win_bt[15];
            output[16] = tmpf_10 * win_bt[16];
            output[17] = tmpf_9 * win_bt[17];
            output[18] = tmpf_8 * win_bt[18];
            output[19] = tmpf_7 * win_bt[19];
            output[20] = tmpf_6 * win_bt[20];
            output[21] = tmpf_5 * win_bt[21];
            output[22] = tmpf_4 * win_bt[22];
            output[23] = tmpf_3 * win_bt[23];
            output[24] = tmpf_2 * win_bt[24];
            output[25] = tmpf_1 * win_bt[25];
            output[26] = tmpf_0 * win_bt[26];
            output[27] = tmpf_0 * win_bt[27];
            output[28] = tmpf_1 * win_bt[28];
            output[29] = tmpf_2 * win_bt[29];
            output[30] = tmpf_3 * win_bt[30];
            output[31] = tmpf_4 * win_bt[31];
            output[32] = tmpf_5 * win_bt[32];
            output[33] = tmpf_6 * win_bt[33];
            output[34] = tmpf_7 * win_bt[34];
            output[35] = tmpf_8 * win_bt[35];
        }
    }

    public class SBI(int[] thel, int[] thes)
    {
        public int[] l = thel;
        public int[] s = thes;
    }

    public class GrInfo
    {
        public int Part2_3Length = 0;
        public int BigValues = 0;
        public int GlobalGain = 0;
        public int ScalefacCompress = 0;
        public int WindowSwitchingFlag = 0;
        public int BlockType = 0;
        public int MixedBlockFlag = 0;
        public int[] TableSelect;
        public int[] SubblockGain;
        public int Region0Count = 0;
        public int Region1Count = 0;
        public int Preflag = 0;
        public int ScalefacScale = 0;
        public int Count1tableSelect = 0;

        public GrInfo()
        {
            TableSelect = new int[3];
            SubblockGain = new int[3];
        }
    }

    public class Temporaire
    {
        public int[] scfsi;
        public GrInfo[] gr;

        public Temporaire()
        {
            scfsi = new int[4];
            gr = new GrInfo[2];
            gr[0] = new GrInfo();
            gr[1] = new GrInfo();
        }
    }

    public class IIISideInfo
    {
        public int main_data_begin = 0;
        public int private_bits = 0;
        public Temporaire[] ch;

        public IIISideInfo()
        {
            ch = new Temporaire[2];
            ch[0] = new Temporaire();
            ch[1] = new Temporaire();
        }
    }

    public class Temporaire2
    {
        public int[] l;         /* [cb] */
        public int[,] s;         /* [window][cb] */

        public Temporaire2()
        {
            l = new int[23];
            s = new int[3, 13];
        }
    }
}

