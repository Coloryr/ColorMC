using System;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Items;

/// <summary>
/// 资源包子项目
/// </summary>
public partial class ResourcePackControl : UserControl
{
    /// <summary>
    /// 用于随机颜色
    /// </summary>
    private readonly Random _random = new();

    public ResourcePackControl()
    {
        InitializeComponent();

        PointerPressed += ResourcePackControl_PointerPressed;
        PointerReleased += ResourcePackControl_PointerReleased;

        DataContextChanged += ResourcePackControl_DataContextChanged;
    }

    private void ResourcePackControl_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ResourcePackModel model)
        {
            MakeText(model.Description);
        }
    }

    /// <summary>
    /// 创建文本
    /// </summary>
    /// <param name="chat">内容</param>
    private void MakeText(ChatObj chat)
    {
        if (chat.Text == "\n")
        {
            Description.Inlines?.Add("\n");
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

            Description.Inlines?.Add(text);
        }

        if (chat.Extra != null)
        {
            foreach (var item in chat.Extra)
            {
                MakeText(item);
            }
        }
    }

    private void ResourcePackControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        LongPressed.Released();
    }

    private void ResourcePackControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var model = (DataContext as ResourcePackModel)!;
        model.Select();
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Flyout((sender as Control)!);
        }
        else
        {
            LongPressed.Pressed(() => Flyout((sender as Control)!));
        }
    }

    private void Flyout(Control control)
    {
        if (DataContext is not ResourcePackModel model)
        {
            return;
        }
        _ = new GameEditFlyout3(control, model);
    }
}
