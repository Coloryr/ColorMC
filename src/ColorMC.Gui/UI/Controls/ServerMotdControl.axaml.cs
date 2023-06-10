using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Net;
using ColorMC.Core.Objs.Minecraft;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

public partial class ServerMotdControl : UserControl
{
    public static readonly StyledProperty<(string, ushort)> IPPortProperty =
        AvaloniaProperty.Register<ServerMotdControl, (string, ushort)>(nameof(IPPort));

    private string? IP;
    private ushort Port;

    public (string, ushort) IPPort
    {
        get => GetValue(IPPortProperty);
        private set => SetValue(IPPortProperty, value);
    }

    private bool FirstLine = true;
    private readonly Random random = new();

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
            IP = data.Item1;
            if (Port == 0)
            {
                var ip = IP;
                if (ip == null)
                {
                    return;
                }
                int index = ip.LastIndexOf(':');
                if (index == -1)
                {
                    Port = 25565;
                }
                else
                {
                    IP = ip[..index];
                    _ = ushort.TryParse(ip[(index + 1)..], out var port);
                    Port = port;
                }
            }
            Port = data.Item2;
            Update();
        }
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        Update();
    }

    public void Load(string ip, ushort port)
    {
        IP = ip;
        Port = port;
    }

    private async void Update()
    {
        Grid1.IsVisible = true;

        FirstLine = true;
        StackPanel1.Children.Clear();
        StackPanel2.Children.Clear();


        var ip = IP;
        var port = Port;
        if (ip == null)
            return;

        var motd = await Task.Run(async () =>
        {
            return await ServerMotd.GetServerInfo(ip, port);
        });
        if (motd.State == StateType.GOOD)
        {
            Grid2.IsVisible = false;
            using var stream = new MemoryStream();
            await stream.WriteAsync(motd.FaviconByteArray);
            stream.Seek(0, SeekOrigin.Begin);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Image1.Source = new Bitmap(stream);

                Label2.Content = motd.Players.Online;
                Label3.Content = motd.Players.Max;
                Label4.Content = motd.Version.Name;
                Label5.Content = motd.Ping;

                MakeText(motd.Description);
            });
        }
        else
        {
            Grid2.IsVisible = true;
        }
        Grid1.IsVisible = false;
    }

    public void MakeText(Chat chat)
    {
        if (chat.Text == "\n")
        {
            FirstLine = false;
            return;
        }

        if (!string.IsNullOrWhiteSpace(chat.Text))
        {
            TextBlock text = new()
            {
                Text = chat.Obfuscated ? " " : chat.Text,
                Foreground = chat.Color == null ? Brushes.White : Brush.Parse(chat.Color)
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
                text.Text = new string((char)random.Next(33, 126), 1);
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

    public void AddText(TextBlock text)
    {
        if (FirstLine)
        {
            StackPanel1.Children.Add(text);
        }
        else
        {
            StackPanel2.Children.Add(text);
        }
    }
}
