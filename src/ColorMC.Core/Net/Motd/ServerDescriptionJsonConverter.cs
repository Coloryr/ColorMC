using ColorMC.Core.Objs.Minecraft;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ColorMC.Core.Net.Motd;

public class ServerDescriptionJsonConverter : JsonConverter<Chat>
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(Chat) == objectType;
    }

    public override Chat? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var str1 = reader.GetString();
            if (string.IsNullOrWhiteSpace(str1))
                return new Chat() { Text = "" };

            var lines = str1.Split("\n");
            var chat = new Chat()
            {
                Extra = new()
            };

            foreach (var item in lines)
            {
                var chat1 = new Chat();
                bool mode = false;
                for (var a = 0; a < item.Length; a++)
                {
                    var char1 = item[a];
                    if (char1 == '§' && mode == false)
                    {
                        if (!string.IsNullOrWhiteSpace(chat1.Text))
                        {
                            chat.Extra.Add(chat1);
                        }
                        chat1 = new()
                        {
                            Bold = chat1.Bold,
                            Underlined = chat1.Underlined,
                            Obfuscated = chat1.Obfuscated,
                            Strikethrough = chat1.Strikethrough,
                            Italic = chat1.Italic,
                            Color = chat1.Color
                        };
                        mode = true;
                    }
                    else if (mode == true)
                    {
                        mode = false;
                        if (ServerMotd.MinecraftColors.TryGetValue(char1, out var color))
                        {
                            chat1.Color = color;
                        }
                        else if (char1 == 'r' || char1 == 'R')
                        {
                            chat1.Underlined = false;
                            chat1.Obfuscated = false;
                            chat1.Strikethrough = false;
                            chat1.Italic = false;
                            chat1.Bold = false;
                            chat1.Color = "#FFFFFF";
                        }
                        else if (char1 == 'k' || char1 == 'K')
                        {
                            chat1.Obfuscated = true;
                        }
                        else if (char1 == 'l' || char1 == 'L')
                        {
                            chat1.Bold = true;
                        }
                        else if (char1 == 'm' || char1 == 'M')
                        {
                            chat1.Strikethrough = true;
                        }
                        else if (char1 == 'n' || char1 == 'N')
                        {
                            chat1.Underlined = true;
                        }
                        else if (char1 == 'o' || char1 == 'O')
                        {
                            chat1.Italic = true;
                        }
                    }
                    else
                    {
                        chat1.Text += char1;
                    }
                }

                chat.Extra.Add(chat1);

                if (lines.Length != 1)
                {
                    chat.Extra.Add(new Chat()
                    {
                        Text = "\n"
                    });
                }
            }

            return chat;
        }
        else
        {
            return s_defaultConverter.Read(ref reader, typeToConvert, options);
        }
    }

    private readonly static JsonConverter<Chat> s_defaultConverter =
        (JsonConverter<Chat>)JsonSerializerOptions.Default.GetConverter(typeof(Chat));

    public override void Write(Utf8JsonWriter writer, Chat value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
