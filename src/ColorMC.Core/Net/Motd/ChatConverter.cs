using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Motd;

/// <summary>
/// Chat解析
/// </summary>
public static class ChatConverter
{
    /// <summary>
    /// 字符串转Chat
    /// </summary>
    /// <param name="str1">输入字符串</param>
    /// <returns></returns>
    public static ChatObj StringToChar(string? str1)
    {
        if (string.IsNullOrWhiteSpace(str1))
        {
            return new ChatObj() { Text = "" };
        }
            
        var lines = str1.Split('\n');
        var chat = new ChatObj()
        {
            Extra = []
        };

        foreach (var item in lines)
        {
            var currentSegment = new ChatObj();
            bool isProcessingCode = false;
            foreach(var c in item)
            {
                if (c == '§' && !isProcessingCode)
                {
                    if (!string.IsNullOrEmpty(currentSegment.Text))
                    {
                        chat.Extra.Add(currentSegment);
                    }
                    currentSegment = currentSegment.Clone();
                    isProcessingCode = true;
                }
                else if (isProcessingCode)
                {
                    isProcessingCode = false;
                    ApplyFormattingCode(currentSegment, c);
                }
                else
                {
                    currentSegment.Text += c;
                }
            }

            chat.Extra.Add(currentSegment);

            if (lines.Length != 1)
            {
                chat.Extra.Add(new ChatObj()
                {
                    Text = "\n"
                });
            }
        }

        return chat;
    }

    private static ChatObj Clone(this ChatObj source)
    {
        return new ChatObj
        {
            Bold = source.Bold,
            Italic = source.Italic,
            Underlined = source.Underlined,
            Strikethrough = source.Strikethrough,
            Obfuscated = source.Obfuscated,
            Color = source.Color
        };
    }

    private static void ResetFormatting(ChatObj segment)
    {
        segment.Bold = segment.Italic = segment.Underlined =
        segment.Strikethrough = segment.Obfuscated = false;
        segment.Color = "#FFFFFF";
    }

    private static void ApplyFormattingCode(ChatObj segment, char code)
    {
        code = char.ToLower(code);
        if (code == 'r')
        {
            ResetFormatting(segment);
        }
        else if (ServerMotd.MinecraftColors.TryGetValue(code, out var color))
        {
            segment.Color = color;
        }
        else
        {
            switch (code)
            {
                case 'l': segment.Bold = true; break;
                case 'o': segment.Italic = true; break;
                case 'n': segment.Underlined = true; break;
                case 'm': segment.Strikethrough = true; break;
                case 'k': segment.Obfuscated = true; break;
            }
        }
    }
}
