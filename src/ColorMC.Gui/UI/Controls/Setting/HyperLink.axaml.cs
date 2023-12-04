using Avalonia;
using Avalonia.Controls;

namespace ColorMC.Gui.UI.Controls.Setting;

public class HyperLink : Button
{
    public static readonly DirectProperty<HyperLink, string> AliasProperty
        = AvaloniaProperty.RegisterDirect<HyperLink, string>(nameof(Alias), o => o.Alias, (o, v) => o.Alias = v);

    private string _alias;

    public string Alias
    {
        get => string.IsNullOrEmpty(_alias) ? "Url" : _alias;
        set
        {
            SetAndRaise(AliasProperty, ref _alias, value);
            var textBlock = new TextBlock
            {
                Text = string.IsNullOrEmpty(_alias) ? "Url" : _alias,
                Classes =
                {
                    "link"
                }
            };

            Content = textBlock;
        }
    }
}
