using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Svg.Skia;
using ColorMC.Core.Utils;
using ShimSkiaSharp;
using Svg.Skia;

namespace ColorMC.Gui.UI.Controls.Svg;

/// <summary>
/// Svg control.
/// </summary>
public class SvgControl : Control
{
    private readonly Uri _baseUri;
    private SKSvg? _svg;

    /// <summary>
    /// Defines the <see cref="Path"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> PathProperty =
        AvaloniaProperty.Register<SvgControl, string?>(nameof(Path));

    /// <summary>
    /// Defines the <see cref="Stretch"/> property.
    /// </summary>
    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<SvgControl, Stretch>(nameof(Stretch), Stretch.Uniform);

    /// <summary>
    /// Defines the <see cref="StretchDirection"/> property.
    /// </summary>
    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<SvgControl, StretchDirection>(
            nameof(StretchDirection),
            StretchDirection.Both);

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<SvgControl, IBrush?>(nameof(Stroke));

    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<SvgControl, IBrush?>(nameof(Fill));

    /// <summary>
    /// Gets or sets the Svg path.
    /// </summary>
    [Content]
    public string? Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    /// <summary>
    /// Gets or sets a value controlling how the image will be stretched.
    /// </summary>
    public Stretch Stretch
    {
        get { return GetValue(StretchProperty); }
        set { SetValue(StretchProperty, value); }
    }

    /// <summary>
    /// Gets or sets a value controlling in what direction the image will be stretched.
    /// </summary>
    public StretchDirection StretchDirection
    {
        get { return GetValue(StretchDirectionProperty); }
        set { SetValue(StretchDirectionProperty, value); }
    }

    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <summary>
    /// Gets svg drawable.
    /// </summary>
    public SKDrawable? Drawable => _svg?.Drawable;

    /// <summary>
    /// Gets svg model.
    /// </summary>
    public SKPicture? Model => _svg?.Model;

    /// <summary>
    /// Gets svg picture.
    /// </summary>
    public SkiaSharp.SKPicture? Picture => _svg?.Picture;

    static SvgControl()
    {
        AffectsRender<SvgControl>(PathProperty, StretchProperty, StretchDirectionProperty);
        AffectsMeasure<SvgControl>(PathProperty, StretchProperty, StretchDirectionProperty);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Svg"/> class.
    /// </summary>
    /// <param name="baseUri">The base URL for the XAML context.</param>
    public SvgControl(Uri baseUri)
    {
        _baseUri = baseUri;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Svg"/> class.
    /// </summary>
    /// <param name="serviceProvider">The XAML service provider.</param>
    public SvgControl(IServiceProvider serviceProvider)
    {
        _baseUri = serviceProvider.GetContextBaseUri();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (_svg?.Picture == null)
        {
            return new Size();
        }

        var sourceSize = _svg?.Picture is { }
            ? new Size(_svg.Picture.CullRect.Width, _svg.Picture.CullRect.Height)
            : default;

        return Stretch.CalculateSize(availableSize, sourceSize, StretchDirection);

    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_svg?.Picture == null)
        {
            return new Size();
        }

        var sourceSize = _svg?.Picture is { }
            ? new Size(_svg.Picture.CullRect.Width, _svg.Picture.CullRect.Height)
            : default;

        return Stretch.CalculateSize(finalSize, sourceSize);
    }

    public override void Render(DrawingContext context)
    {
        var source = _svg;
        if (source?.Picture is null)
        {
            return;
        }

        var viewPort = new Rect(Bounds.Size);
        var sourceSize = new Size(source.Picture.CullRect.Width, source.Picture.CullRect.Height);
        if (sourceSize.Width <= 0 || sourceSize.Height <= 0)
        {
            return;
        }

        var scale = Stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection);
        var scaledSize = sourceSize * scale;
        var destRect = viewPort
            .CenterRect(new Rect(scaledSize))
            .Intersect(viewPort);
        var sourceRect = new Rect(sourceSize)
            .CenterRect(new Rect(destRect.Size / scale));

        var bounds = source.Picture.CullRect;
        var scaleMatrix = Matrix.CreateScale(
            destRect.Width / sourceRect.Width,
            destRect.Height / sourceRect.Height);
        var translateMatrix = Matrix.CreateTranslation(
            -sourceRect.X + destRect.X - bounds.Top,
            -sourceRect.Y + destRect.Y - bounds.Left);

        using (context.PushClip(destRect))
        using (context.PushTransform(translateMatrix * scaleMatrix))
        {
            context.Custom(
                new SvgCustomDrawOperation(
                    new Rect(0, 0, bounds.Width, bounds.Height),
                    source));
        }
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PathProperty
            || change.Property == FillProperty
            || change.Property == StrokeProperty)
        {
            Load();
            InvalidateVisual();
        }
    }

    private void Load()
    {
        _svg?.Dispose();
        _svg = null;

        try
        {
            var path = Path;
            if (path is not null)
            {
                _svg = SvgSource.LoadPicture(path, _baseUri, Stroke, Fill);
            }
        }
        catch (Exception e)
        {
            Logs.Error("Failed to load svg image: ", e);
            _svg = null;
        }
    }
}