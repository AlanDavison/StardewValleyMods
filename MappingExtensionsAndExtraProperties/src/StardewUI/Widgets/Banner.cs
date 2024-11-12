using System;
using Microsoft.Xna.Framework;
using StardewUI.Graphics;
using StardewUI.Layout;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace StardewUI.Widgets;

/// <summary>
/// Draws banner-style text with an optional background.
/// </summary>
/// <remarks>
/// This is very similar to a <see cref="Label"/> inside a <see cref="Frame"/>, but uses the special
/// <see cref="SpriteText"/> font which is more prominent than any of the game's available
/// <see cref="Microsoft.Xna.Framework.Graphics.SpriteFont"/>s and often used for top-level headings/menu titles.
/// </remarks>
public class Banner : View
{
    /// <summary>
    /// Background sprite (including border) to draw underneath the text.
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
    /// The thickness of the border edges within the <see cref="Background"/>. sprite.
    /// </summary>
    /// <remarks>
    /// This property has no effect on the appearance of the <see cref="Background"/>, but affects how content is
    /// positioned inside the border. It is often correct to set it to the same value as the
    /// <see cref="Sprite.FixedEdges"/> of the <see cref="Background"/> sprite, but the values are considered
    /// independent.
    /// </remarks>
    public Edges BackgroundBorderThickness
    {
        get => this.backgroundBorderThickness.Value;
        set
        {
            if (this.backgroundBorderThickness.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.BackgroundBorderThickness));
            }
        }
    }

    /// <summary>
    /// The text to display within the banner.
    /// </summary>
    public string Text
    {
        get => this.text.Value;
        set
        {
            if (this.text.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.Text));
            }
        }
    }

    /// <summary>
    /// Alpha value for the text shadow, per layer in <see cref="ShadowLayers"/>.
    /// </summary>
    /// <remarks>
    /// If set to zero, no text shadow will be drawn.
    /// </remarks>
    public float TextShadowAlpha
    {
        get => this.textShadowAlpha;
        set
        {
            if (value != this.textShadowAlpha)
            {
                this.textShadowAlpha = value;
                this.OnPropertyChanged(nameof(this.TextShadowAlpha));
            }
        }
    }

    /// <summary>
    /// Base color for the text shadow, before applying <see cref="TextShadowAlpha"/>.
    /// </summary>
    public Color TextShadowColor
    {
        get => this.textShadowColor;
        set
        {
            if (value != this.textShadowColor)
            {
                this.textShadowColor = value;
                this.OnPropertyChanged(nameof(this.textShadowColor));
            }
        }
    }

    /// <summary>
    /// Specifies which layers of the text shadow should be drawn.
    /// </summary>
    /// <remarks>
    /// Layers are additive, so the same <see cref="TextShadowAlpha"/> will have a different visual intensity depending
    /// on which layers are allowed. If set to <see cref="ShadowLayers.None"/>, then no shadow will be drawn.
    /// </remarks>
    public ShadowLayers TextShadowLayers
    {
        get => this.textShadowLayers;
        set
        {
            if (value != this.textShadowLayers)
            {
                this.textShadowLayers = value;
                this.OnPropertyChanged(nameof(this.TextShadowLayers));
            }
        }
    }

    /// <summary>
    /// Offset to draw the text shadow, which is a second copy of the <see cref="Text"/> drawn entirely black.
    /// Text shadows will not be visible unless <see cref="TextShadowAlpha"/> is non-zero.
    /// </summary>
    public Vector2 TextShadowOffset
    {
        get => this.textShadowOffset;
        set
        {
            if (value != this.textShadowOffset)
            {
                this.textShadowOffset = value;
                this.OnPropertyChanged(nameof(this.TextShadowOffset));
            }
        }
    }

    private static readonly ShadowLayers[] shadowLayerOrder =
    [
        ShadowLayers.Diagonal,
        ShadowLayers.Vertical,
        ShadowLayers.Horizontal,
    ];

    private readonly DirtyTracker<Edges> backgroundBorderThickness = new(Edges.NONE);
    private readonly DirtyTracker<string> text = new("");

    private Sprite? background;
    private NineSlice? backgroundSlice;
    private float textShadowAlpha;
    private Color textShadowColor = Game1.textShadowDarkerColor;
    private ShadowLayers textShadowLayers = ShadowLayers.All;
    private Vector2 textShadowOffset = new(-3, 3);
    private Vector2 textSize;

    /// <inheritdoc />
    protected override Edges GetBorderThickness()
    {
        return this.BackgroundBorderThickness;
    }

    /// <inheritdoc />
    protected override bool IsContentDirty()
    {
        return this.backgroundBorderThickness.IsDirty || this.text.IsDirty;
    }

    /// <inheritdoc />
    protected override void OnDrawBorder(ISpriteBatch b)
    {
        this.backgroundSlice?.Draw(b);
    }

    /// <inheritdoc />
    protected override void OnDrawContent(ISpriteBatch b)
    {
        float centerX = this.ContentSize.X / 2;
        b.DelegateDraw(
            (wb, origin) =>
            {
                if (this.TextShadowAlpha > 0 && this.TextShadowLayers > 0)
                {
                    var shadowAlphaColor = this.TextShadowColor * this.TextShadowAlpha;
                    foreach (var layer in shadowLayerOrder)
                    {
                        if ((this.TextShadowLayers & layer) == 0)
                        {
                            continue;
                        }
                        var offset = layer switch
                        {
                            ShadowLayers.Diagonal => this.TextShadowOffset,
                            ShadowLayers.Horizontal => new(this.TextShadowOffset.X, 0),
                            ShadowLayers.Vertical => new(0, this.TextShadowOffset.Y),
                            _ => throw new InvalidOperationException($"Invalid shadow layer {layer}"),
                        };
                        SpriteText.drawStringHorizontallyCenteredAt(
                            wb, this.Text,
                            (int)(origin.X + centerX + offset.X),
                            (int)(origin.Y + offset.Y),
                            color: shadowAlphaColor
                        );
                    }
                }
                SpriteText.drawStringHorizontallyCenteredAt(wb, this.Text, (int)(origin.X + centerX), (int)origin.Y);
            }
        );
    }

    /// <inheritdoc />
    protected override void OnMeasure(Vector2 availableSize)
    {
        int width = SpriteText.getWidthOfString(this.Text);
        int height = SpriteText.getHeightOfString(this.Text);
        this.textSize = new(width, height);
        this.ContentSize = this.Layout.Resolve(availableSize, () => this.textSize);

        if (this.backgroundSlice?.Sprite != this.Background)
        {
            this.backgroundSlice = this.Background is not null ? new(this.Background) : null;
        }

        this.backgroundSlice?.Layout(new(Point.Zero, this.BorderSize.ToPoint()));
    }

    /// <inheritdoc />
    protected override void ResetDirty()
    {
        this.backgroundBorderThickness.ResetDirty();
        this.text.ResetDirty();
    }
}
