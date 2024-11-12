using Microsoft.Xna.Framework;
using StardewUI.Layout;

namespace StardewUI.Graphics;

/// <summary>
/// Draws sprites according to a <see href="https://en.wikipedia.org/wiki/9-slice_scaling">nine-slice scale</see>.
/// </summary>
/// <param name="sprite">The source sprite.</param>
public class NineSlice(Sprite sprite)
{
    /// <summary>
    /// The source sprite.
    /// </summary>
    public Sprite Sprite { get; init; } = sprite;

    private readonly Rectangle[,] sourceGrid = GetGrid(
        sprite.SourceRect ?? sprite.Texture.Bounds,
        sprite.FixedEdges ?? Edges.NONE,
        sprite.SliceSettings
    );

    private Rectangle[,]? destinationGrid;
    private SimpleRotation? rotation;

    /// <summary>
    /// Draws the sprite to an <see cref="ISpriteBatch"/>, applying 9-slice scaling if specified.
    /// </summary>
    /// <param name="b">Output sprite batch.</param>
    /// <param name="tint">Optional tint multiplier color.</param>
    public void Draw(ISpriteBatch b, Color? tint = null)
    {
        if (this.destinationGrid is null)
        {
            // Layout has not been performed.
            return;
        }
        float rotationAngle = this.rotation?.Angle() ?? 0;
        for (int sourceY = 0; sourceY < this.sourceGrid.GetLength(0); sourceY++)
        {
            for (int sourceX = 0; sourceX < this.sourceGrid.GetLength(1); sourceX++)
            {
                if ((this.Sprite.SliceSettings?.EdgesOnly ?? false) && sourceX == 1 && sourceY == 1)
                {
                    continue;
                }
                (int destX, int destY) = RotateGridIndices(sourceX, sourceY, this.rotation);
                var sourceRect = this.sourceGrid[sourceY, sourceX];
                if (sourceRect.Width == 0 || sourceRect.Height == 0)
                {
                    // If some or all of the fixed edges are zero, then there is nothing to draw for that part and we
                    // can skip some wasted cycles trying to "draw" it.
                    continue;
                }
                var destinationRect = this.destinationGrid[destY, destX];
                if (this.rotation.HasValue)
                {
                    var rotationOrigin = sourceRect.Size.ToVector2() / 2;
                    // DestinationRect behaves in very confusing ways when rotation is involved, so
                    // for these cases, it's easier to place using the point overload after
                    // computing the scale from src:dest.
                    var destinationSizeInSourceOrientation = this.rotation.Value.IsQuarter()
                        ? new Point(destinationRect.Height, destinationRect.Width)
                        : destinationRect.Size;
                    var scale = destinationSizeInSourceOrientation.ToVector2() / sourceRect.Size.ToVector2();
                    // Don't use .Center.ToVector2() because it truncates and we get 1-pixel offsets.
                    var center = new Vector2(
                        destinationRect.X + destinationRect.Width / 2f,
                        destinationRect.Y + destinationRect.Height / 2f
                    );
                    b.Draw(this.Sprite.Texture, center, sourceRect, tint, rotationAngle, rotationOrigin, scale);
                }
                else
                {
                    b.Draw(this.Sprite.Texture, destinationRect, sourceRect, tint);
                }
            }
        }
    }

    /// <summary>
    /// Prepares the layout for next <see cref="Draw"/>.
    /// </summary>
    /// <param name="destinationRect">The rectangular area that the drawn sprite should fill.</param>
    /// <param name="rotation">Rotation to apply to the source sprite, if any.</param>
    public void Layout(Rectangle destinationRect, SimpleRotation? rotation = null)
    {
        var destinationEdges = this.Sprite.FixedEdges ?? Edges.NONE;
        float sliceScale = this.Sprite.SliceSettings?.Scale ?? 1;
        if (rotation is not null)
        {
            destinationEdges = destinationEdges.Rotate(rotation.Value);
        }
        this.rotation = rotation;
        this.destinationGrid = GetGrid(destinationRect, destinationEdges, sliceScale: sliceScale);
    }

    private static Rectangle[,] GetGrid(
        Rectangle bounds,
        Edges fixedEdges,
        SliceSettings? settings = null,
        // We pass sliceScale separately (even though it is in SliceSettings) because it actually applies only to the
        // destination rect, not the source, whereas the SliceSettings are used for source but not destination.
        float sliceScale = 1
    )
    {
        int left = bounds.X;
        int top = bounds.Y;
        if (sliceScale != 1)
        {
            fixedEdges *= sliceScale;
        }
        int centerStartX = left + fixedEdges.Left;
        int centerStartY = top + fixedEdges.Top;
        int centerEndX = bounds.Right - fixedEdges.Right;
        int centerEndY = bounds.Bottom - fixedEdges.Bottom;
        if (settings?.CenterX is int cx)
        {
            if (settings.CenterXPosition == SliceCenterPosition.Start)
            {
                centerStartX = cx;
            }
            else
            {
                centerEndX = cx;
            }
        }
        if (settings?.CenterY is int cy)
        {
            if (settings.CenterYPosition == SliceCenterPosition.Start)
            {
                centerStartY = cy;
            }
            else
            {
                centerEndY = cy;
            }
        }
        int innerWidth = centerEndX - centerStartX;
        int innerHeight = centerEndY - centerStartY;
        int startRight = bounds.Right - fixedEdges.Right;
        int startBottom = bounds.Bottom - fixedEdges.Bottom;
        return new Rectangle[3, 3]
        {
            {
                new(left, top, fixedEdges.Left, fixedEdges.Top),
                new(centerStartX, top, innerWidth, fixedEdges.Top),
                new(startRight, top, fixedEdges.Right, fixedEdges.Top),
            },
            {
                new(left, centerStartY, fixedEdges.Left, innerHeight),
                new(centerStartX, centerStartY, innerWidth, innerHeight),
                new(startRight, centerStartY, fixedEdges.Right, innerHeight),
            },
            {
                new(left, startBottom, fixedEdges.Left, fixedEdges.Bottom),
                new(centerStartX, startBottom, innerWidth, fixedEdges.Bottom),
                new(startRight, startBottom, fixedEdges.Right, fixedEdges.Bottom),
            },
        };
    }

    private static (int x, int y) RotateGridIndices(int x, int y, SimpleRotation? rotation)
    {
        return rotation switch
        {
            SimpleRotation.QuarterClockwise => (2 - y, x),
            SimpleRotation.QuarterCounterclockwise => (y, 2 - x),
            SimpleRotation.Half => (2 - x, 2 - y),
            _ => (x, y),
        };
    }
}
