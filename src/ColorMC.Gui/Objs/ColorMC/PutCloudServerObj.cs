﻿using System.Text.Json.Serialization;

namespace ColorMC.Gui.Objs.ColorMC;

public record PutCloudServerObj
{
    public record CustomObj
    {
        public string Version { get; set; }
        public int Loader { get; set; }
        public bool IsLoader { get; set; }
        public string Text { get; set; }
    }

    [JsonPropertyName("token")]
    public string Token { get; set; }
    [JsonPropertyName("ip")]
    public string IP { get; set; }
    [JsonPropertyName("custom")]
    public CustomObj Custom { get; set; }
}
