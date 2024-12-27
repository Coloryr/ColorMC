using System.Text;

namespace ColorMC.Gui.MusicPlayer.Decoder.Mp3;

public static class HeaderHelper
{
    public static readonly string[,,] BitrateStrTable = {
            {{"free format", "32 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s",
                    "80 kbit/s", "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s",
                    "160 kbit/s", "176 kbit/s", "192 kbit/s", "224 kbit/s", "256 kbit/s",
                    "forbidden"},
            {"free format", "8 kbit/s", "16 kbit/s", "24 kbit/s", "32 kbit/s",
                    "40 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s", "80 kbit/s",
                    "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s", "160 kbit/s",
                    "forbidden"},
            {"free format", "8 kbit/s", "16 kbit/s", "24 kbit/s", "32 kbit/s",
                    "40 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s", "80 kbit/s",
                    "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s", "160 kbit/s",
                    "forbidden"}},

            {{"free format", "32 kbit/s", "64 kbit/s", "96 kbit/s", "128 kbit/s",
                    "160 kbit/s", "192 kbit/s", "224 kbit/s", "256 kbit/s", "288 kbit/s",
                    "320 kbit/s", "352 kbit/s", "384 kbit/s", "416 kbit/s", "448 kbit/s",
                    "forbidden"},
            {"free format", "32 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s",
                    "80 kbit/s", "96 kbit/s", "112 kbit/s", "128 kbit/s", "160 kbit/s",
                    "192 kbit/s", "224 kbit/s", "256 kbit/s", "320 kbit/s", "384 kbit/s",
                    "forbidden"},
            {"free format", "32 kbit/s", "40 kbit/s", "48 kbit/s", "56 kbit/s",
                    "64 kbit/s", "80 kbit/s", "96 kbit/s", "112 kbit/s", "128 kbit/s",
                    "160 kbit/s", "192 kbit/s", "224 kbit/s", "256 kbit/s", "320 kbit/s",
                    "forbidden"}},
            // SZD: MPEG2.5
            {{"free format", "32 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s",
                    "80 kbit/s", "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s",
                    "160 kbit/s", "176 kbit/s", "192 kbit/s", "224 kbit/s", "256 kbit/s",
                    "forbidden"},
            {"free format", "8 kbit/s", "16 kbit/s", "24 kbit/s", "32 kbit/s",
                    "40 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s", "80 kbit/s",
                    "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s", "160 kbit/s",
                    "forbidden"},
            {"free format", "8 kbit/s", "16 kbit/s", "24 kbit/s", "32 kbit/s",
                    "40 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s", "80 kbit/s",
                    "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s", "160 kbit/s",
                    "forbidden"}},
    };

    public static string GetBitrateString(this Mp3Header header)
    {
        if (header.Vbr)
        {
            return header.GetBitrate() / 1000 + " kb/s";
        }
        else return BitrateStrTable[(int)header.Version, (int)header.Layer - 1, header.BitrateIndex];
    }

    public static string? GetSampleFrequencyString(this Mp3Header header)
    {
        switch (header.SampleFrequency)
        {
            case FrequencyType.ThirtyTow:
                if (header.Version == VersionType.Mpeg1)
                    return "32 kHz";
                else if (header.Version == VersionType.Mpeg2LSF)
                    return "16 kHz";
                else    // SZD
                    return "8 kHz";
            case FrequencyType.FourtyFourPointOne:
                if (header.Version == VersionType.Mpeg1)
                    return "44.1 kHz";
                else if (header.Version == VersionType.Mpeg2LSF)
                    return "22.05 kHz";
                else    // SZD
                    return "11.025 kHz";
            case FrequencyType.FourtyEight:
                if (header.Version == VersionType.Mpeg1)
                    return "48 kHz";
                else if (header.Version == VersionType.Mpeg2LSF)
                    return "24 kHz";
                else    // SZD
                    return "12 kHz";
        }
        return null;
    }

    public static string GetString(this Mp3Header header)
    {
        StringBuilder buffer = new();
        buffer.Append("Layer ")
            .Append(header.Layer.ToString())
            .Append(" frame ")
            .Append(header.Mode.ToString())
            .Append(' ')
            .Append(header.Version switch
            {
                VersionType.Mpeg1 => "MPEG-1",
                VersionType.Mpeg2LSF => "MPEG-2 LSF",
                VersionType.Mpeg25LSF => "MPEG-2.5 LSF", // SZD
                _ => null
            });
        if (!header.Checksums)
        {
            buffer.Append(" no");
        }
        buffer.Append(" checksums")
            .Append('\n')
            .Append(GetSampleFrequencyString(header))
            .Append('\n')
            .Append(GetBitrateString(header));

        return buffer.ToString();
    }
}
