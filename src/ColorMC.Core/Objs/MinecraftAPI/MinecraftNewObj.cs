using Newtonsoft.Json;

namespace ColorMC.Core.Objs.MinecraftAPI;

public record MinecraftNewObj
{
    public record ArticleGridObj
    {
        public record DefaultTileObj
        {
            public record ImageObj
            {
                [JsonProperty("content_type")]
                public string ContentType { get; set; }
                [JsonProperty("imageURL")]
                public string ImageURL { get; set; }
                [JsonProperty("alt")]
                public string Alt { get; set; }
            }

            [JsonProperty("title")]
            public string Title { get; set; }
            [JsonProperty("sub_header")]
            public string SubHeader { get; set; }
            [JsonProperty("tile_size")]
            public string TileSize { get; set; }
            [JsonProperty("image")]
            public ImageObj Image { get; set; }
            
        }
        [JsonProperty("default_tile")]
        public DefaultTileObj DefaultTile { get; set; }
        [JsonProperty("primary_category")]
        public string PrimaryCategory { get; set; }
        [JsonProperty("article_url")]
        public string ArticleUrl { get; set; }
    }

    [JsonProperty("article_grid")]
    public List<ArticleGridObj> ArticleGrid { get; set; }
}
