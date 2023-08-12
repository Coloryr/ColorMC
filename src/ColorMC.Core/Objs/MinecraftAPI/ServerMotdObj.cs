using ColorMC.Core.Net.Motd;
using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Minecraft;

public enum StateType
{
    GOOD,
    NO_RESPONSE,
    BAD_CONNECT,
    EXCEPTION
}

[JsonConverter(typeof(ServerDescriptionJsonConverter))]
public class Chat
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
    public List<Chat> Extra { get; set; }

    public override string ToString()
    {
        return ServerMotd.CleanFormat(this.ToPlainTextString());
    }
}

public record ServerVersionInfo
{
    public string Name { get; set; }

    public int Protocol { get; set; }
}

public record ServerPlayerInfo
{
    public int Max { get; set; }

    public int Online { get; set; }

    public List<Player> Sample { get; set; }
}

public record Player
{
    public string Name { get; set; }

    public string Id { get; set; }
}

public record ModInfo
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("modList")]
    public List<Mod> ModList { get; set; }
}

public record Mod
{
    [JsonPropertyName("modid")]
    public string ModId { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }
}

public class ServerMotdObj
{
    /// <summary>
    /// Server's address, support srv.
    /// </summary>
    [JsonIgnore]
    public string ServerAddress { get; set; }

    /// <summary>
    /// Server's runing port
    /// </summary>
    [JsonIgnore]
    public ushort ServerPort { get; set; }

    /// <summary>
    /// The server version info such name or protocol
    /// </summary>
    [JsonPropertyName("version")]
    public ServerVersionInfo Version { get; set; }

    /// <summary>
    /// The server player info such max or current player count and sample.
    /// </summary>
    [JsonPropertyName("players")]
    public ServerPlayerInfo Players { get; set; }

    /// <summary>
    /// Server's description (aka motd)
    /// </summary>
    [JsonPropertyName("description")]
    public Chat Description { get; private set; }

    /// <summary>
    /// server's favicon. is a png image that is base64 encoded
    /// </summary>
    [JsonPropertyName("favicon")]
    public string Favicon { get; set; }

    /// <summary>
    /// Server's mod info including mod type and mod list (if is avaliable)
    /// </summary>
    [JsonPropertyName("modinfo")]
    public ModInfo ModInfo { get; private set; }

    [JsonIgnore]
    public byte[] FaviconByteArray { get { return Convert.FromBase64String(Favicon.Replace("data:image/png;base64,", "")); } }

    /// <summary>
    /// The ping delay time.(ms)
    /// </summary>
    [JsonIgnore]
    public long Ping { get; set; }

    /// <summary>
    /// The handshake state
    /// </summary>
    [JsonIgnore]
    public StateType State { get; set; }

    /// <summary>
    /// The handshake message
    /// </summary>
    [JsonIgnore]
    public string Message { get; set; }

    [JsonIgnore]
    public bool AcceptTextures { get; set; }

    public ServerMotdObj(string ip, ushort port)
    {
        ServerAddress = ip;
        ServerPort = port;
        Favicon = "data:image/png;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";
    }
}
