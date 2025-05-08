using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.MinecraftAPI;

public record MinecraftNewObj
{
    public record ArticleGridObj
    {
        public record DefaultTileObj
        {
            public record ImageObj
            {
                [JsonPropertyName("content_type")]
                public string ContentType { get; set; }
                [JsonPropertyName("imageURL")]
                public string ImageURL { get; set; }
                [JsonPropertyName("alt")]
                public string Alt { get; set; }
            }

            [JsonPropertyName("title")]
            public string Title { get; set; }
            [JsonPropertyName("sub_header")]
            public string SubHeader { get; set; }
            [JsonPropertyName("tile_size")]
            public string TileSize { get; set; }
            [JsonPropertyName("image")]
            public ImageObj Image { get; set; }
            
        }
        [JsonPropertyName("default_tile")]
        public DefaultTileObj DefaultTile { get; set; }
        [JsonPropertyName("primary_category")]
        public string PrimaryCategory { get; set; }
        [JsonPropertyName("article_url")]
        public string ArticleUrl { get; set; }
    }

    [JsonPropertyName("article_grid")]
    public List<ArticleGridObj> ArticleGrid { get; set; }
}
