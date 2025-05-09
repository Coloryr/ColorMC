using System.Text.Json.Serialization;
using ColorMC.Core.Net.Motd;

namespace ColorMC.Core.Objs.MinecraftAPI;

public enum StateType
{
    Ok,
    NoData,
    ConnectFail,
    Error
}

public class ChatObj
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("bold")]
    public bool Bold { get; set; }

    [JsonPropertyName("italic")]
    public bool Italic { get; set; }

    [JsonPropertyName("underlined")]
    public bool Underlined { get; set; }

    [JsonPropertyName("strikethrough")]
    public bool Strikethrough { get; set; }

    [JsonPropertyName("obfuscated")]
    public bool Obfuscated { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; }

    [JsonPropertyName("extra")]
    public List<ChatObj> Extra { get; set; }

    public override string ToString()
    {
        return ServerMotd.CleanFormat(this.ToPlainTextString());
    }
}

public record ServerMotdObj
{
    public record ServerVersionInfoObj
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("protocol")]
        public int Protocol { get; set; }
    }

    public record ServerPlayerInfoObj
    {
        public record Player
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("id")]
            public string Id { get; set; }
        }

        [JsonPropertyName("max")]
        public int Max { get; set; }

        [JsonPropertyName("online")]
        public int Online { get; set; }

        [JsonPropertyName("sample")]
        public List<Player> Sample { get; set; }
    }

    public record ServerMotdModInfoObj
    {
        public record Mod
        {
            [JsonPropertyName("modid")]
            public string ModId { get; set; }

            [JsonPropertyName("version")]
            public string Version { get; set; }
        }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("modList")]
        public List<Mod> ModList { get; set; }
    }

    /// <summary>
    /// Server's address, support srv.
    /// </summary>
    public string ServerAddress { get; set; }

    /// <summary>
    /// Server's runing port
    /// </summary>
    public ushort ServerPort { get; set; }

    /// <summary>
    /// The server version info such name or protocol
    /// </summary>
    public ServerVersionInfoObj? Version { get; set; }

    /// <summary>
    /// The server player info such max or current player count and sample.
    /// </summary>
    public ServerPlayerInfoObj? Players { get; set; }

    /// <summary>
    /// Server's description (aka motd)
    /// </summary>
    public ChatObj Description { get; set; }

    /// <summary>
    /// server's favicon. is a png image that is base64 encoded
    /// </summary>
    public string? Favicon { get; set; }

    /// <summary>
    /// Server's mod info including mod type and mod list (if is avaliable)
    /// </summary>
    public ServerMotdModInfoObj? ModInfo { get; set; }

    public byte[] FaviconByteArray
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Favicon))
            {
                return Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
            }
            else
            {
                return Convert.FromBase64String(Favicon.Replace("data:image/png;base64,", ""));
            }
        }
    }

    /// <summary>
    /// The ping delay time.(ms)
    /// </summary>
    public long Ping { get; set; }
    /// <summary>
    /// The handshake state
    /// </summary>
    public StateType State { get; set; }
    /// <summary>
    /// The handshake message
    /// </summary>
    public string Message { get; set; }

    public ServerMotdObj(string ip, ushort port)
    {
        ServerAddress = ip;
        ServerPort = port;
    }
}
