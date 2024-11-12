using System;
using System.Collections.Generic;
using StardewUI.Graphics;
using StardewUI.Widgets;

namespace StardewUI.Animation;

/// <summary>
/// Animates the sprite of an <see cref="Image"/>, using equal duration for all frames in a list.
/// </summary>
public class SpriteAnimator : IAnimator
{
    /// <summary>
    /// Duration of each frame.
    /// </summary>
    public TimeSpan FrameDuration { get; set; }

    /// <summary>
    /// Frames to animate through.
    /// </summary>
    public IReadOnlyList<Sprite> Frames { get; set; } = [];

    /// <summary>
    /// Whether or not to pause animation. If <c>true</c>, the animator will hold at the current position and not
    /// progress until set to <c>false</c> again.
    /// </summary>
    public bool Paused { get; set; }

    /// <summary>
    /// Delay before advancing from the first frame to the next frames.
    /// </summary>
    /// <remarks>
    /// Repeats on every loop, but only applies to the first frame of each loop.
    /// </remarks>
    public TimeSpan StartDelay { get; set; }

    private readonly WeakReference<Image> imageRef;

    private TimeSpan delayElapsed;
    private TimeSpan elapsed;

    /// <summary>
    /// Initializes a new instance of <see cref="SpriteAnimator"/> that animates the sprite on a specified image.
    /// </summary>
    /// <param name="image">The image to animate.</param>
    public SpriteAnimator(Image image)
    {
        this.imageRef = new(image);
        AnimationRunner.Register(this);
    }

    /// <summary>
    /// Resets the animation to the first frame, and waits any <see cref="StartDelay"/> required again.
    /// </summary>
    public void Reset()
    {
        if (!this.imageRef.TryGetTarget(out var image))
        {
            return;
        }
        image.Sprite = this.Frames[0];
        this.delayElapsed = TimeSpan.Zero;
        this.elapsed = TimeSpan.Zero;
    }

    /// <inheritdoc />
    public void Tick(TimeSpan elapsed)
    {
        if (this.Paused || this.FrameDuration == TimeSpan.Zero || this.Frames.Count == 0 || !this.imageRef.TryGetTarget(out var image))
        {
            return;
        }
        if (this.elapsed == TimeSpan.Zero && this.delayElapsed < this.StartDelay)
        {
            this.delayElapsed += elapsed;
            if (this.Frames.Count > 0)
            {
                image.Sprite = this.Frames[0];
            }
            if (this.delayElapsed < this.StartDelay)
            {
                return;
            }
        }

        this.delayElapsed = TimeSpan.Zero;
        var totalDuration = this.FrameDuration * this.Frames.Count;
        this.elapsed += elapsed;
        if (this.elapsed > totalDuration)
        {
            this.elapsed = this.StartDelay > TimeSpan.Zero
                    ? TimeSpan.Zero
                    : TimeSpan.FromTicks(this.elapsed.Ticks % totalDuration.Ticks);
        }
        int frameIndex = (int)(this.elapsed.TotalMilliseconds / this.FrameDuration.TotalMilliseconds);
        image.Sprite = this.Frames[frameIndex];
    }

    bool IAnimator.IsValid()
    {
        return this.imageRef.TryGetTarget(out _);
    }
}
