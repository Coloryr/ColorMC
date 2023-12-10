using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Net.Motd;
using ColorMC.Core.Objs.Minecraft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

public partial class ServerMotdControl : UserControl
{
    public static readonly StyledProperty<(string, ushort)> IPPortProperty =
        AvaloniaProperty.Register<ServerMotdControl, (string, ushort)>(nameof(IPPort));

    private string? _ip;
    private ushort _port;

    public (string, ushort) IPPort
    {
        get => GetValue(IPPortProperty);
        private set => SetValue(IPPortProperty, value);
    }

    private bool _firstLine = true;
    private readonly Random _random = new();

    public ServerMotdControl()
    {
        InitializeComponent();

        Button2.Click += Button2_Click;

        PropertyChanged += ServerMotdControl_PropertyChanged;
    }

    private void ServerMotdControl_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IPPortProperty)
        {
            var data = IPPort;
            _ip = data.Item1;
            if (_port == 0)
            {
                var ip = _ip;
                if (ip == null)
                {
                    Button2.IsVisible = false;
                    return;
                }
                int index = ip.LastIndexOf(':');
                if (index == -1)
                {
                    _port = 25565;
                }
                else
                {
                    _ip = ip[..index];
                    _ = ushort.TryParse(ip[(index + 1)..], out var port);
                    _port = port;
                }
            }
            _port = data.Item2;
            Update();
            Button2.IsVisible = true;
        }
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        Update();
    }

    private async void Update()
    {
        Grid1.IsVisible = true;

        _firstLine = true;

        StackPanel1.Inlines?.Clear();
        StackPanel2.Inlines?.Clear();
        StackPanel1.Text = "";
        StackPanel2.Text = "";

        var ip = _ip;
        var port = _port;
        if (ip == null)
        {
            return;
        }

        var motd = await Task.Run(() =>
        {
            return ServerMotd.GetServerInfo(ip, port);
        });
        if (motd.State == StateType.GOOD)
        {
            Grid2.IsVisible = false;
            using var stream = new MemoryStream(motd.FaviconByteArray);
            Image1.Source = new Bitmap(stream);

            Label2.Content = motd.Players.Online;
            Label3.Content = motd.Players.Max;
            Label4.Content = motd.Version.Name;
            Label5.Content = motd.Ping;

            MakeText(motd.Description);

            StackPanel1.Inlines?.Add(" ");
            StackPanel2.Inlines?.Add(" ");
        }
        else
        {
            Grid2.IsVisible = true;
        }
        Grid1.IsVisible = false;
    }

    private static Dictionary<string, string> ColorMap = new()
    {
        { "black", "#000000" },
        { "dark_blue", "#0000aa" },
        { "dark_green", "#00aa00" },
        { "dark_aqua", "#000000" },
        { "dark_red", "#aa0000" },
        { "dark_purple", "#aa00aa" },
        { "gold", "#ffaa00" },
        { "gray", "#aaaaaa" },
        { "dark_gray", "#555555" },
        { "blue", "#5555ff" },
        { "green", "#55ff55" },
        { "aqua", "#55ffff" },
        { "red", "#ff5555" },
        { "light_purple", "#ff55ff" },
        { "yellow", "#ffff55" },
        { "white", "#ffffff" }
    };

    private static string FixColor(string color)
    {
        if (color.StartsWith('#'))
            return color;
        if (ColorMap.TryGetValue(color, out var color1))
        {
            return color1;
        }

        return color;
    }

    public void MakeText(Chat chat)
    {
        if (chat.Text == "\n")
        {
            _firstLine = false;
            return;
        }

        if (!string.IsNullOrWhiteSpace(chat.Text))
        {
            var text = new Run()
            {
                Text = chat.Obfuscated ? " " : chat.Text,
                Foreground = chat.Color == null ? Brushes.White
                    : Brush.Parse(FixColor(chat.Color))
            };

            if (chat.Bold)
            {
                text.FontWeight = FontWeight.Bold;
            }
            if (chat.Italic)
            {
                text.FontStyle = FontStyle.Oblique;
            }
            if (chat.Underlined)
            {
                if (text.TextDecorations == null)
                {
                    text.TextDecorations = TextDecorations.Underline;
                }
                else
                {
                    text.TextDecorations.AddRange(TextDecorations.Underline);
                }
            }
            if (chat.Strikethrough)
            {
                if (text.TextDecorations == null)
                {
                    text.TextDecorations = TextDecorations.Strikethrough;
                }
                else
                {
                    text.TextDecorations.AddRange(TextDecorations.Strikethrough);
                }
            }

            if (chat.Obfuscated)
            {
                text.Text = new string((char)_random.Next(33, 126), 1);
            }

            AddText(text);
        }

        if (chat.Extra != null)
        {
            foreach (var item in chat.Extra)
            {
                MakeText(item);
            }
        }
    }

    public void AddText(Inline text)
    {
        if (_firstLine)
        {
            StackPanel1.Inlines?.Add(text);
        }
        else
        {
            StackPanel2.Inlines?.Add(text);
        }
    }
}
