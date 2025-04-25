using Newtonsoft.Json;

namespace ColorMC.Core.Objs.MinecraftAPI;

public record MinecraftNewObj
{
    public record ResultObj
    {
        public record ResultsObj
        {
            [JsonProperty("title")]
            public string Title { get; set; }
            [JsonProperty("neutralTitle")]
            public string NeutralTitle { get; set; }
            [JsonProperty("url")]
            public string Url { get; set; }
            [JsonProperty("superhead")]
            public string Superhead { get; set; }
            [JsonProperty("description")]
            public string Description { get; set; }
            [JsonProperty("author")]
            public string Author { get; set; }
            [JsonProperty("image")]
            public string Image { get; set; }
            [JsonProperty("imageAltText")]
            public string ImageAltText { get; set; }
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("productId")]
            public string ProductId { get; set; }
        }
        [JsonProperty("results")]
        public List<ResultsObj> Results { get; set; }
    }

    [JsonProperty("result")]
    public ResultObj Result { get; set; }
}
