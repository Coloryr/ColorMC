using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ColorMC.Core.Objs.MinecraftAPI;

public record MinecraftNewObj
{
    public record ArticleObj
    {
        public record DefaultObj
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
            [JsonProperty("tile_size")]
            public string TileSize { get; set; }
            [JsonProperty("sub_header")]
            public string SubHeader { get; set; }
            [JsonProperty("image")]
            public ImageObj Image { get; set; }
        }
        [JsonProperty("default_tile")]
        public DefaultObj DefaultTile { get; set; }
        [JsonProperty("primary_category")]
        public string PrimaryCategory { get; set; }
        [JsonProperty("article_url")]
        public string ArticleUrl { get; set; }
    }
    [JsonProperty("article_grid")]
    public List<ArticleObj> ArticleGrid { get; set; }
    [JsonProperty("article_count")]
    public int ArticleCount { get; set; }
}
