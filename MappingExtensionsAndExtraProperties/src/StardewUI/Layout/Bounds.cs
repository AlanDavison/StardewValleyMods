using System;
using Microsoft.Xna.Framework;

namespace StardewUI.Layout;

/// <summary>
/// A bounding rectangle using floating-point dimensions.
/// </summary>
/// <param name="Position">The top-left position.</param>
/// <param name="Size">The width and height.</param>
[DuckType]
public record Bounds(Vector2 Position, Vector2 Size) : IOffsettable<Bounds>
{
    /// <summary>
    /// Empty bounds, used for invalid results.
    /// </summary>
    public static readonly Bounds Empty = new(Vector2.Zero, Vector2.Zero);

    /// <summary>
    /// The Y value at the bottom edge of the bounding rectangle.
    /// </summary>
    public float Bottom => this.Position.Y + this.Size.Y;

    /// <summary>
    /// The X value at the left edge of the bounding rectangle.
    /// </summary>
    public float Left => this.Position.X;

    /// <summary>
    /// The X value at the right edge of the bounding rectangle.
    /// </summary>
    public float Right => this.Position.X + this.Size.X;

    /// <summary>
    /// The Y value at the top edge of the bounding rectangle.
    /// </summary>
    public float Top => this.Position.Y;

    /// <summary>
    /// Gets the point at the center of the bounding rectangle.
    /// </summary>
    public Vector2 Center()
    {
        return new(this.Position.X + this.Size.X / 2, this.Position.Y + this.Size.Y / 2);
    }

    /// <summary>
    /// Checks if an entire bounding rectangle is fully within these bounds.
    /// </summary>
    /// <param name="bounds">The other bounds.</param>
    /// <returns><c>true</c> if <paramref name="bounds"/> are a subset of the current instance; <c>false</c> if the two
    /// bounds do not overlap or only overlap partially.</returns>
    public bool ContainsBounds(Bounds bounds)
    {
        return this.Intersection(bounds) == bounds;
    }

    /// <summary>
    /// Checks if a given point is within the bounds.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <returns><c>true</c> if <paramref name="point"/> is inside these bounds; otherwise <c>false</c>.</returns>
    public bool ContainsPoint(Vector2 point)
    {
        return point.X >= this.Left && point.X < this.Right && point.Y >= this.Top && point.Y < this.Bottom;
    }

    /// <summary>
    /// Computes the intersection of this <see cref="Bounds"/> with another instance.
    /// </summary>
    /// <param name="other">The other bounds to intersect with.</param>
    /// <returns>A new <see cref="Bounds"/> whose area is the intersection of this instance and
    /// <paramref name="other"/>, or <see cref="Empty"/> if they do not overlap.</returns>
    public Bounds Intersection(Bounds other)
    {
        float left = MathF.Max(this.Left, other.Left);
        float right = MathF.Min(this.Right, other.Right);
        if (right <= left)
        {
            return Empty;
        }
        float top = MathF.Max(this.Top, other.Top);
        float bottom = MathF.Min(this.Bottom, other.Bottom);
        if (bottom <= top)
        {
            return Empty;
        }
        var position = new Vector2(left, top);
        var size = new Vector2(right - left, bottom - top);
        return new(position, size);
    }

    /// <summary>
    /// Checks if this <see cref="Bounds"/> intersects with another instance, without computing the intersection.
    /// </summary>
    /// <param name="other">The other bounds to check for intersection.</param>
    /// <returns>True if this <see cref="Bounds"/> and the <paramref name="other"/> bounds have any intersecting area,
    /// otherwise <c>false</c>.</returns>
    public bool IntersectsWith(Bounds other)
    {
        return other.Right > this.Left && other.Left < this.Right && other.Bottom > this.Top && other.Top < this.Bottom;
    }

    /// <summary>
    /// Offsets a <see cref="Bounds"/> by a given distance.
    /// </summary>
    /// <param name="distance">The offset distance.</param>
    /// <returns>A new <see cref="Bounds"/> with the same size as this instance and a <see cref="Position"/> offset by
    /// the specified <paramref name="distance"/>.</returns>
    public Bounds Offset(Vector2 distance)
    {
        return new(this.Position + distance, this.Size);
    }

    /// <summary>
    /// Computes the union of this <see cref="Bounds"/> with another instance.
    /// </summary>
    /// <param name="other">The other bounds to add to the union.</param>
    /// <returns>A new <see cref="Bounds"/> whose set is the union of this instance and <paramref name="other"/>; i.e.
    /// is exactly large enough to contain both bounds.</returns>
    public Bounds Union(Bounds other)
    {
        float left = MathF.Min(this.Left, other.Left);
        float top = MathF.Min(this.Top, other.Top);
        float right = MathF.Max(this.Right, other.Right);
        float bottom = MathF.Max(this.Bottom, other.Bottom);
        var position = new Vector2(left, top);
        var size = new Vector2(right - left, bottom - top);
        return new(position, size);
    }
}
