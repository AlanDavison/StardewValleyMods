using System;
using Microsoft.Xna.Framework;
using StardewUI.Graphics;
using StardewUI.Layout;

namespace StardewUI.Widgets;

/// <summary>
/// A view that draws a sprite, scaled to the layout size.
/// </summary>
public class Image : View
{
    /// <summary>
    /// How to fit the image in the content area, if sizes differ.
    /// </summary>
    /// <remarks>
    /// The fit setting is always ignored when <i>both</i> the <see cref="LayoutParameters.Width"/> and
    /// <see cref="LayoutParameters.Height"/> use <see cref="LengthType.Content"/>, because that combination of settings
    /// will cause the exact <see cref="Sprite.SourceRect"/> (or texture bounds, if not specified) as the layout size.
    /// At least one dimension must be content-independent (fixed or container size) for this to have any effect.
    /// </remarks>
    public ImageFit Fit
    {
        get => this.fit;
        set
        {
            if (value != this.fit)
            {
                this.fit = value;
                this.OnPropertyChanged(nameof(this.Fit));
            }
        }
    }

    /// <summary>
    /// Specifies where to align the image horizontally if the image width is different from the final layout width.
    /// </summary>
    public Alignment HorizontalAlignment
    {
        get => this.horizontalAlignment;
        set
        {
            if (value != this.horizontalAlignment)
            {
                this.horizontalAlignment = value;
                this.OnPropertyChanged(nameof(this.HorizontalAlignment));
            }
        }
    }

    /// <summary>
    /// Rotation to apply to the image.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="Scale"/>, rotation potentially <b>does</b> affect layout under specific conditions;
    /// specifically, if any dimensions are <see cref="LengthType.Content"/> sized, and the rotation is 90° in either
    /// direction, it will take the opposite dimension for layout. However, images whose dimensions are entirely fixed
    /// or stretch-based will not have their layout affected.
    /// </remarks>
    public SimpleRotation? Rotation
    {
        get => this.rotation.Value;
        set
        {
            if (this.rotation.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.Rotation));
            }
        }
    }

    /// <summary>
    /// Scale to apply to the image.
    /// </summary>
    /// <remarks>
    /// This scale acts only as a drawing transformation and does not affect layout; a scaled-up image can potentially
    /// draw (or clip) outside its container, and a scaled-down image will not shrink the size of an image that
    /// specifies <see cref="LengthType.Content"/> for either or both dimensions.
    /// </remarks>
    public float Scale
    {
        get => this.scale.Value;
        set
        {
            if (this.scale.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.Scale));
            }
        }
    }

    /// <summary>
    /// Alpha value for the shadow. If set to the default of zero, no shadow will be drawn.
    /// </summary>
    public float ShadowAlpha
    {
        get => this.shadowAlpha;
        set
        {
            if (value != this.shadowAlpha)
            {
                this.shadowAlpha = value;
                this.OnPropertyChanged(nameof(this.ShadowAlpha));
            }
        }
    }

    /// <summary>
    /// Offset to draw the sprite shadow, which is a second copy of the <see cref="Sprite"/> drawn entirely black.
    /// Shadows will not be visible unless <see cref="ShadowAlpha"/> is non-zero.
    /// </summary>
    public Vector2 ShadowOffset
    {
        get => this.shadowOffset;
        set
        {
            if (value != this.shadowOffset)
            {
                this.shadowOffset = value;
                this.OnPropertyChanged(nameof(this.ShadowOffset));
            }
        }
    }

    /// <summary>
    /// The sprite to draw.
    /// </summary>
    /// <remarks>
    /// If <see cref="LayoutParameters"/> uses <see cref="LengthType.Content"/> for either dimension, then changing the
    /// sprite can affect layout depending on <see cref="Fit"/>.
    /// </remarks>
    public Sprite? Sprite
    {
        get => this.sprite.Value;
        set
        {
            if (this.sprite.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.Sprite));
            }
        }
    }

    /// <summary>
    /// Tint color (multiplier) to apply when drawing.
    /// </summary>
    public Color Tint
    {
        get => this.tint;
        set
        {
            if (value != this.tint)
            {
                this.tint = value;
                this.OnPropertyChanged(nameof(this.Tint));
            }
        }
    }

    /// <summary>
    /// Specifies where to align the image vertically if the image height is different from the final layout height.
    /// </summary>
    public Alignment VerticalAlignment
    {
        get => this.verticalAlignment;
        set
        {
            if (value != this.verticalAlignment)
            {
                this.verticalAlignment = value;
                this.OnPropertyChanged(nameof(this.VerticalAlignment));
            }
        }
    }

    private readonly DirtyTracker<SimpleRotation?> rotation = new(null);
    private readonly DirtyTracker<float> scale = new(1.0f);
    private readonly DirtyTracker<Sprite?> sprite = new(null);

    private Rectangle destinationRect = Rectangle.Empty;
    private ImageFit fit = ImageFit.Contain;
    private Alignment horizontalAlignment = Alignment.Start;
    private float shadowAlpha;
    private Vector2 shadowOffset;
    private NineSlice? slice = null;
    private Color tint = Color.White;
    private Alignment verticalAlignment = Alignment.Start;

    /// <inheritdoc />
    protected override bool IsContentDirty()
    {
        // We intentionally don't check scale here, as scale doesn't affect layout size.
        // Instead, that is checked (and reset) in the draw method.
        return this.sprite.IsDirty || this.rotation.IsDirty;
    }

    /// <inheritdoc />
    protected override void OnDrawContent(ISpriteBatch b)
    {
        if (this.scale.IsDirty)
        {
            this.UpdateSlice();
        }
        Rectangle? clipRect = this.Fit == ImageFit.Cover ? new(0, 0, (int)this.ContentSize.X, (int)this.ContentSize.Y) : null;
        if (this.ShadowAlpha > 0 && this.slice is not null)
        {
            using var _transform = b.SaveTransform();
            b.Translate(this.ShadowOffset);
            using var _shadowClip = clipRect.HasValue ? b.Clip(clipRect.Value) : null;
            this.slice.Draw(b, new(Color.Black, this.ShadowAlpha));
        }
        using var _clip = clipRect.HasValue ? b.Clip(clipRect.Value) : null;
        this.slice?.Draw(b, this.Tint);
    }

    /// <inheritdoc />
    protected override void OnMeasure(Vector2 availableSize)
    {
        var limits = this.Layout.GetLimits(availableSize);
        var imageSize = this.GetImageSize(limits);
        this.ContentSize = this.Layout.Resolve(availableSize, () => imageSize);
        if (this.sprite.IsDirty)
        {
            this.slice = this.sprite.Value is not null ? new(this.sprite.Value) : null;
        }
        if (this.slice is not null)
        {
            float left = this.HorizontalAlignment.Align(imageSize.X, this.ContentSize.X);
            float top = this.VerticalAlignment.Align(imageSize.Y, this.ContentSize.Y);
            this.destinationRect = new Rectangle(new Vector2(left, top).ToPoint(), imageSize.ToPoint());
            this.UpdateSlice();
        }
    }

    /// <inheritdoc />
    protected override void ResetDirty()
    {
        this.rotation.ResetDirty();
        this.sprite.ResetDirty();
    }

    private Vector2 GetImageSize(Vector2 limits)
    {
        if (this.Sprite is null)
        {
            return Vector2.Zero;
        }
        var sourceRect = this.Sprite.SourceRect ?? this.Sprite.Texture.Bounds;
        bool swapDimensions = this.Rotation?.IsQuarter() ?? false;
        (int sourceWidth, int sourceHeight) = !swapDimensions
            ? (sourceRect.Width, sourceRect.Height)
            : (sourceRect.Height, sourceRect.Width);
        float scale = this.Sprite.SliceSettings?.Scale ?? 1;
        float scaledSourceWidth = sourceWidth * scale;
        float scaledSourceHeight = sourceHeight * scale;
        if (this.Layout.Width.Type == LengthType.Content
            && (this.Fit != ImageFit.Contain || this.Layout.Height.Type == LengthType.Content)
            && scaledSourceWidth < limits.X
        )
        {
            limits.X = scaledSourceWidth;
        }
        if (this.Layout.Height.Type == LengthType.Content
            && (this.Fit != ImageFit.Contain || this.Layout.Width.Type == LengthType.Content)
            && scaledSourceHeight < limits.Y
        )
        {
            limits.Y = scaledSourceHeight;
        }
        if (this.Fit == ImageFit.Stretch)
        {
            return limits;
        }
        if (this.Fit == ImageFit.None || this.IsSourceSize())
        {
            return new(scaledSourceWidth, scaledSourceHeight);
        }
        float maxScaleX = limits.X / sourceWidth;
        float maxScaleY = limits.Y / sourceHeight;
        return this.Fit switch
        {
            ImageFit.Contain => sourceRect.Size.ToVector2() * MathF.Min(maxScaleX, maxScaleY),
            ImageFit.Cover => sourceRect.Size.ToVector2() * MathF.Max(maxScaleX, maxScaleY),
            _ => throw new NotImplementedException($"Invalid fit type: {this.Fit}"),
        };
    }

    private bool IsSourceSize()
    {
        return this.Layout.Width.Type == LengthType.Content && this.Layout.Height.Type == LengthType.Content;
    }

    private void UpdateSlice()
    {
        if (this.slice is null)
        {
            // Still reset the dirty flag, because when the slice is eventually created it will have the newest scale.
            this.scale.ResetDirty();
            return;
        }

        if (this.Scale == 1.0f)
        {
            this.slice.Layout(this.destinationRect, this.Rotation);
        }
        else
        {
            var deltaSize = this.destinationRect.Size.ToVector2() * (this.Scale - 1) / 2;
            var scaledRect = this.destinationRect; // Make a copy (Rectangle is struct)
            scaledRect.Inflate(deltaSize.X, deltaSize.Y);
            this.slice.Layout(scaledRect, this.Rotation);
        }

        this.rotation.ResetDirty();
        this.scale.ResetDirty();
    }
}

/// <summary>
/// Specifies how an image should be scaled to fit the content area when the available size is different from the image
/// size, and especially when it has a different aspect ratio.
/// </summary>
public enum ImageFit
{
    /// <summary>
    /// Don't scale the image, i.e. draw it at its original size regardless of the eventual layout size.
    /// </summary>
    None,

    /// <summary>
    /// Force uniform scaling, and make both dimensions small enough to fit in the content area.
    /// </summary>
    /// <remarks>
    /// If one dimension uses <see cref="LengthType.Content"/>, then the other dimension will be scaled to fit, and the
    /// content-dependent dimension will be set according to the image's aspect ratio.
    /// </remarks>
    Contain,

    /// <summary>
    /// Force uniform scaling, and make both dimensions large enough to completely cover the content area (i.e. clip
    /// whatever parts are outside the bounds).
    /// </summary>
    /// <remarks>
    /// </remarks>
    Cover,

    /// <summary>
    /// Allow non-uniform scaling, and scale the image to exactly match the content area.
    /// </summary>
    Stretch,
}
