using ColorMC.Core.Objs.Minecraft;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Net.Motd;

public class ServerDescriptionJsonConverter : JsonConverter<Chat>
{
    public override Chat? ReadJson(JsonReader reader, Type objectType, Chat? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            var str1 = reader.Value?.ToString();
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
            JObject obj = JObject.Load(reader);
            Chat chat = new();
            serializer.Populate(obj.CreateReader(), chat);
            return chat;
        }
    }

    public override void WriteJson(JsonWriter writer, Chat? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
