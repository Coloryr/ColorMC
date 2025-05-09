using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ColorMC.Core.Net.Motd;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls;

/// <summary>
/// 服务器Motd显示
/// </summary>
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
            _port = data.Item2;
            if (_port <= 0)
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
            else
            {
                _port = data.Item2;
            }
            Update();
            Button2.IsVisible = true;
        }
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        Update();
    }

    /// <summary>
    /// 刷新Motd显示内容
    /// </summary>
    private async void Update()
    {
        Button2.IsVisible = false;

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
            Button2.IsVisible = true;
            return;
        }

        var motd = await Task.Run(() =>
        {
            return ServerMotd.GetServerInfo(ip, port);
        });
        if (motd.State == StateType.Ok)
        {
            Grid2.IsVisible = false;
            using var stream = new MemoryStream(motd.FaviconByteArray);
            Image1.Source = new Bitmap(stream);

            Label2.Text = motd.Players?.Online.ToString() ?? "";
            Label3.Text = motd.Players?.Max.ToString() ?? "";
            Label4.Text = motd.Version?.Name ?? "";
            Label5.Text = motd.Ping.ToString();

            MakeText(motd.Description);

            StackPanel1.Inlines?.Add(new Run(""));
            StackPanel2.Inlines?.Add(new Run(""));
        }
        else
        {
            Grid2.IsVisible = true;
        }
        Grid1.IsVisible = false;

        Button2.IsVisible = true;
    }

    /// <summary>
    /// 生成文字显示
    /// </summary>
    /// <param name="chat">文本</param>
    private void MakeText(ChatObj chat)
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
                Foreground = UIUtils.GetColor(chat.Color)
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

    /// <summary>
    /// 添加文本到显示
    /// </summary>
    /// <param name="text"></param>
    private void AddText(Inline text)
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
