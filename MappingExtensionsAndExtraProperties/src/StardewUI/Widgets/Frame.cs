using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewUI.Graphics;
using StardewUI.Input;
using StardewUI.Layout;

namespace StardewUI.Widgets;

/// <summary>
/// A view that holds another view, typically for the purpose of adding a border or background, or in some cases
/// swapping out the content.
/// </summary>
public class Frame : View
{
    /// <summary>
    /// The background sprite to draw for this frame.
    /// </summary>
    public Sprite? Background
    {
        get => this.background;
        set
        {
            if (value != this.background)
            {
                this.background = value;
                this.OnPropertyChanged(nameof(this.Background));
            }
        }
    }

    /// <summary>
    /// Tint color for the <see cref="Background"/> image.
    /// </summary>
    public Color BackgroundTint
    {
        get => this.backgroundTint;
        set
        {
            if (value != this.backgroundTint)
            {
                this.backgroundTint = value;
                this.OnPropertyChanged(nameof(this.BackgroundTint));
            }
        }
    }

    /// <summary>
    /// The border sprite to draw for this frame.
    /// </summary>
    /// <remarks>
    /// Setting a border here does not affect layout, even if <see cref="Sprite.FixedEdges"/> are set to non-zero
    /// values, since fixed edges only govern scaling and are not necessarily the same as the actual edge thicknesses.
    /// To ensure that inner content does not overlap with the border, <see cref="BorderThickness"/> should also be set
    /// when using a border.
    /// </remarks>
    public Sprite? Border
    {
        get => this.border;
        set
        {
            if (value != this.border)
            {
                this.border = value;
                this.OnPropertyChanged(nameof(this.Border));
            }
        }
    }

    /// <summary>
    /// The thickness of the border edges.
    /// </summary>
    /// <remarks>
    /// This property has no effect on the appearance of the <see cref="Border"/>, but affects how content is positioned
    /// inside the border. It is often correct to set it to the same value as the <see cref="Sprite.FixedEdges"/> of the
    /// <see cref="Border"/> sprite, but the values are considered independent.
    /// </remarks>
    public Edges BorderThickness
    {
        get => this.borderThickness.Value;
        set
        {
            if (this.borderThickness.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.BorderThickness));
            }
        }
    }

    /// <summary>
    /// The inner content view, which will render inside the border and padding.
    /// </summary>
    public IView? Content
    {
        get => this.content.Value;
        set
        {
            if (this.content.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.Content));
            }
        }
    }

    /// <summary>
    /// Specifies how to align the <see cref="Content"/> horizontally within the frame's area. Only has an effect if the
    /// frame's content area is larger than the content size, i.e. when <see cref="LayoutParameters.Width"/> does
    /// <i>not</i> use <see cref="LengthType.Content"/>.
    /// </summary>
    public Alignment HorizontalContentAlignment
    {
        get => this.horizontalContentAlignment.Value;
        set
        {
            if (this.horizontalContentAlignment.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.HorizontalContentAlignment));
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
    /// Number of shadows to draw if <see cref="ShadowAlpha"/> is non-zero.
    /// </summary>
    /// <remarks>
    /// While rare, some game sprites are supposed to be drawn with multiple stacked shadows. If this number is higher
    /// than the default of <c>1</c>, shadows will be drawn stacked with the offset repeatedly applied.
    /// </remarks>
    public int ShadowCount
    {
        get => this.shadowCount;
        set
        {
            if (value != this.shadowCount)
            {
                this.shadowCount = value;
                this.OnPropertyChanged(nameof(this.ShadowCount));
            }
        }
    }

    /// <summary>
    /// Offset to draw the sprite shadow, which is a second copy of the <see cref="Background"/> drawn entirely black.
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
    /// Specifies how to align the <see cref="Content"/> vertically within the frame's area. Only has an effect if the
    /// frame's content area is larger than the content size, i.e. when <see cref="LayoutParameters.Height"/> does
    /// <i>not</i> use <see cref="LengthType.Content"/>.
    /// </summary>
    public Alignment VerticalContentAlignment
    {
        get => this.verticalContentAlignment.Value;
        set
        {
            if (this.verticalContentAlignment.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.VerticalContentAlignment));
            }
        }
    }

    private readonly DirtyTracker<Edges> borderThickness = new(Edges.NONE);
    private readonly DirtyTracker<IView?> content = new(null);
    private readonly DirtyTracker<Alignment> horizontalContentAlignment = new(Alignment.Start);
    private readonly DirtyTracker<Alignment> verticalContentAlignment = new(Alignment.Start);

    private Sprite? background;
    private NineSlice? backgroundSlice;
    private Color backgroundTint = Color.White;
    private Sprite? border;
    private NineSlice? borderSlice;
    private Vector2 contentPosition;
    private float shadowAlpha;
    private int shadowCount = 1;
    private Vector2 shadowOffset;

    /// <inheritdoc />
    protected override FocusSearchResult? FindFocusableDescendant(Vector2 contentPosition, Direction direction)
    {
        return this.Content?.FocusSearch(contentPosition, direction);
    }

    /// <inheritdoc />
    protected override Edges GetBorderThickness()
    {
        return this.BorderThickness;
    }

    /// <inheritdoc />
    protected override IEnumerable<ViewChild> GetLocalChildren()
    {
        return this.Content is not null ? [new(this.Content, this.contentPosition)] : [];
    }

    /// <inheritdoc />
    protected override bool IsContentDirty()
    {
        return this.borderThickness.IsDirty
            || this.horizontalContentAlignment.IsDirty
            || this.verticalContentAlignment.IsDirty
            || this.content.IsDirty
            || (this.Content?.IsDirty() ?? false);
    }

    /// <inheritdoc />
    protected override void OnDrawBorder(ISpriteBatch b)
    {
        using (b.SaveTransform())
        {
            b.Translate(this.BorderThickness.Left, this.BorderThickness.Top);
            if (this.ShadowAlpha > 0 && this.ShadowCount >= 1 && this.backgroundSlice is not null)
            {
                using var _ = b.SaveTransform();
                for (int i = 0; i < this.ShadowCount; i++)
                {
                    b.Translate(this.ShadowOffset);
                    this.backgroundSlice.Draw(b, new(Color.Black, this.ShadowAlpha));
                }
            }

            this.backgroundSlice?.Draw(b, this.BackgroundTint);
        }

        this.borderSlice?.Draw(b);
    }

    /// <inheritdoc />
    protected override void OnDrawContent(ISpriteBatch b)
    {
        if (this.Content is null)
        {
            return;
        }
        using var _ = b.SaveTransform();
        b.Translate(this.contentPosition);
        this.Content.Draw(b);
    }

    /// <inheritdoc />
    protected override void OnMeasure(Vector2 availableSize)
    {
        this.Content?.Measure(this.Layout.GetLimits(availableSize));

        this.ContentSize = this.Layout.Resolve(availableSize, () => this.Content?.OuterSize ?? Vector2.Zero);
        this.UpdateContentPosition();

        if (this.borderSlice?.Sprite != this.Border)
        {
            this.borderSlice = this.Border is not null ? new(this.Border) : null;
        }

        this.borderSlice?.Layout(new(Point.Zero, this.BorderSize.ToPoint()));

        if (this.backgroundSlice?.Sprite != this.Background)
        {
            this.backgroundSlice = this.Background is not null ? new(this.Background) : null;
        }

        this.backgroundSlice?.Layout(new(Point.Zero, this.InnerSize.ToPoint()));
    }

    /// <inheritdoc />
    protected override void ResetDirty()
    {
        this.borderThickness.ResetDirty();
        this.horizontalContentAlignment.ResetDirty();
        this.verticalContentAlignment.ResetDirty();
        this.content.ResetDirty();
    }

    private void UpdateContentPosition()
    {
        if (this.Content is null || this.Content.OuterSize == this.ContentSize)
        {
            this.contentPosition = Vector2.Zero;
            return;
        }
        float left = this.HorizontalContentAlignment.Align(this.Content.OuterSize.X, this.ContentSize.X);
        float top = this.VerticalContentAlignment.Align(this.Content.OuterSize.Y, this.ContentSize.Y);
        this.contentPosition = new(left, top);
    }
}
