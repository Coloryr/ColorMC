using System;
using System.ComponentModel;
using Svg.Model;
using Svg;
using Avalonia.Platform;
using Avalonia.Media;
using Avalonia.Media.Immutable;

using Color = System.Drawing.Color;
using Avalonia.Svg.Skia;
using Svg.Skia;
using SkiaSharp;

namespace ColorMC.Gui.UI.Controls.Svg;

/// <summary>
/// Represents a Svg based image.
/// </summary>
[TypeConverter(typeof(SvgSourceTypeConverter))]
public class SvgSource
{

    /// <summary>
    /// Loads svg picture from file or resource.
    /// </summary>
    /// <param name="path">The path to file or resource.</param>
    /// <param name="baseUri">The base uri.</param>
    /// <returns>The svg picture.</returns>
    public static SKSvg? LoadPicture(string path, Uri? baseUri, IBrush? stroke, IBrush? fill)
    {
        Color? stroke1 = stroke is ImmutableSolidColorBrush brush
                ? Color.FromArgb(brush.Color.A, brush.Color.R,
                brush.Color.G, brush.Color.B) : null;
        Color? fill1 = fill is ImmutableSolidColorBrush brush1
            ? Color.FromArgb(brush1.Color.A, brush1.Color.R,
            brush1.Color.G, brush1.Color.B) : null;
        var uri = path.StartsWith("/") ? new Uri(path, UriKind.Relative) : new Uri(path, UriKind.RelativeOrAbsolute);
        if (uri.IsAbsoluteUri && uri.IsFile)
        {
            var document = SvgExtensions.Open(uri.LocalPath);
            if (document != null)
            {
                if (stroke1.HasValue) document.Stroke = new SvgColourServer(stroke1.Value);
                if (fill1.HasValue) document.Fill = new SvgColourServer(fill1.Value);
            }
            var skia = new SKSvg();

            if (document != null)
            {
                skia.FromSvgDocument(document);
            }

            return skia;
        }
        else
        {
            var stream = AssetLoader.Open(uri, baseUri);
            if (stream is null)
            {
                return default;
            }
            var document = SvgExtensions.Open(stream);
            if (document != null)
            {
                if (stroke1.HasValue) document.Stroke = new SvgColourServer(stroke1.Value);
                if (fill1.HasValue) document.Fill = new SvgColourServer(fill1.Value);
            }
            var skia = new SKSvg();

            if (document != null)
            {
                skia.FromSvgDocument(document);
            }

            return skia;
        }
    }
}