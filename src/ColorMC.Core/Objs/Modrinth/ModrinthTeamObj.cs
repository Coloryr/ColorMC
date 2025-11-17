using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Modrinth;

public record ModrinthTeamObj
{
    public record TeamserObj
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }
    }

    [JsonPropertyName("user")]
    public TeamserObj User { get; set; }
}
