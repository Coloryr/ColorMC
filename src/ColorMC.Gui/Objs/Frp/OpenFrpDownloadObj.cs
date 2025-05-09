using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ColorMC.Gui.Objs.Frp;

public record OpenFrpDownloadObj
{
    public record OpenFrpDownloadItemObj
    {
        //public record Soft
        //{
        //    public record OpenFrpDownloadArchObj
        //    {
        //        public string label { get; set; }
        //        public string file { get; set; }
        //    }
        //    public string os { get; set; }
        //    public string label { get; set; }
        //    public List<OpenFrpDownloadArchObj> arch { get; set; }
        //}
        //public record Launcher
        //{
        //    public string latest { get; set; }
        //}
        public record SourceObj
        {
            //public string label { get; set; }
            [JsonPropertyName("value")]
            public string Value { get; set; }
        }
        [JsonPropertyName("latest")]
        public string Latest { get; set; }
        [JsonPropertyName("latest_full")]
        public string LatestFull { get; set; }
        //public string latest_ver { get; set; }
        //public string latest_msg { get; set; }
        //public string common_details { get; set; }
        //public Launcher launcher { get; set; }
        [JsonPropertyName("source")]
        public List<SourceObj> Source { get; set; }
        //public List<Soft> soft { get; set; }
    }
    [JsonPropertyName("data")]
    public OpenFrpDownloadItemObj Data { get; set; }
}
