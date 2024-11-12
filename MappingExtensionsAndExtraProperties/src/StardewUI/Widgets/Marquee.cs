using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewUI.Animation;
using StardewUI.Graphics;
using StardewUI.Input;
using StardewUI.Layout;

namespace StardewUI.Widgets;

/// <summary>
/// A scrolling marquee supporting any inner content.
/// </summary>
/// <remarks>
/// Works by doubling and shifting the drawing the portion and applying a clipping rectangle, and therefore should be
/// used only for non-interactive content; clicks and focus searches will not be correct inside the content area.
/// </remarks>
public class Marquee : View
{
    /// <summary>
    /// Content to scroll inside the marquee.
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
    /// Distance in pixels between the copy of the content being scrolled "out" of the marquee, and the second copy of
    /// the content being scrolled "in".
    /// </summary>
    /// <remarks>
    /// For example, a marquee scrolling the text "Hello World" might at any given moment look like:
    /// <example>
    /// <code>
    /// +----------------+
    /// | ld   Hello wor |
    /// +----------------+
    /// </code>
    /// and in the above scenario the value refers to the distance between the ending "d" and the starting "H".
    /// </example>
    /// </remarks>
    public float ExtraDistance
    {
        get => this.extraDistance.Value;
        set
        {
            if (this.extraDistance.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.ExtraDistance));
            }
        }
    }

    /// <summary>
    /// Scrolling speed, in pixels per second.
    /// </summary>
    public float Speed
    {
        get => this.speed.Value;
        set
        {
            if (!this.speed.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.Speed));
            }
        }
    }

    private readonly Animator<Marquee, float> animator;
    private readonly DirtyTracker<IView?> content = new(null);
    private readonly DirtyTracker<float> extraDistance = new(100);
    private readonly DirtyTracker<float> speed = new(1);

    private float progress;

    /// <summary>
    /// Initializes a new instance of <see cref="Marquee"/>.
    /// </summary>
    public Marquee()
    {
        this.animator = Animator.On(this, x => x.progress, (x, v) => x.progress = v);
        this.animator.Loop = true;
    }

    /// <inheritdoc />
    protected override FocusSearchResult? FindFocusableDescendant(Vector2 contentPosition, Direction direction)
    {
        return this.Content?.FocusSearch(contentPosition, direction);
    }

    /// <inheritdoc />
    protected override IEnumerable<ViewChild> GetLocalChildren()
    {
        return this.Content is not null ? [new(this.Content, Vector2.Zero)] : [];
    }

    /// <inheritdoc />
    protected override bool IsContentDirty()
    {
        return this.extraDistance.IsDirty || this.speed.IsDirty || this.content.IsDirty || (this.Content?.IsDirty() ?? false);
    }

    /// <inheritdoc />
    protected override void OnDrawContent(ISpriteBatch b)
    {
        if (this.Content is null)
        {
            return;
        }
        float contentLength = this.Content.OuterSize.X;
        using var _ = b.Clip(new(0, 0, (int)this.OuterSize.X, (int)this.OuterSize.Y));
        b.Translate(-this.progress, 0);
        this.Content.Draw(b);
        b.Translate(contentLength + this.ExtraDistance, 0);
        this.Content.Draw(b);
    }

    /// <inheritdoc />
    protected override void OnMeasure(Vector2 availableSize)
    {
        var containerLimits = this.Layout.GetLimits(availableSize);
        var contentLimits = containerLimits;
        contentLimits.X = float.PositiveInfinity;
        this.Content?.Measure(contentLimits);
        this.ContentSize = this.Layout.Resolve(availableSize, () => this.Content?.OuterSize ?? Vector2.Zero);
        this.UpdateAnimation();
    }

    /// <inheritdoc />
    protected override void ResetDirty()
    {
        this.content.ResetDirty();
        this.extraDistance.ResetDirty();
        this.speed.ResetDirty();
    }

    private void UpdateAnimation()
    {
        if (this.animator is null)
        {
            return;
        }
        if (this.Content is null || this.Speed == 0)
        {
            this.animator.Stop();
            return;
        }
        float scrollWidth = this.Content.OuterSize.X + this.ExtraDistance;
        var duration = TimeSpan.FromSeconds(scrollWidth / this.Speed);
        var animation = new Animation<float>(0, scrollWidth, duration);
        // Try to save current progress and restore it after restarting animation.
        var restoredTimeProgress = TimeSpan.FromSeconds(this.progress / this.Speed);
        // This will reset the progress to 0.
        this.animator.Start(animation);
        // Advance back to the adjusted saved position.
        this.animator.Tick(restoredTimeProgress);
    }
}
