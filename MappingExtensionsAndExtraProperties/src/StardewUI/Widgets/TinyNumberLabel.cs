using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewUI.Graphics;
using StardewUI.Layout;

namespace StardewUI.Widgets;

/// <summary>
/// Renders a single-line numeric label using custom digit sprites.
/// </summary>
/// <remarks>
/// <para>
/// Corresponds to <see cref="StardewValley.Utility.drawTinyDigits"/>.
/// </para>
/// <para>
/// For this widget type, <see cref="Scale"/> <b> does</b> affect layout, and the size of the rendered text is entirely
/// based on the <see cref="DigitSprites"/> and cumulative scale which is effectively treated like a font size. If the
/// view's <see cref="View.Layout"/> uses any non-content-based dimensions, it will affect the box size as expected but
/// will not change the rendered text; the text is not scaled to the layout bounds.
/// </para>
/// </remarks>
public class TinyNumberLabel : View
{
    /// <summary>
    /// The sprites for each individual digit, with the index corresponding to the digit itself (element 0 for digit
    /// '0', element 4 for digit '4', etc.). This must have exactly 10 elements.
    /// </summary>
    public IReadOnlyList<Sprite> DigitSprites
    {
        get => this.digitSprites.Value;
        set
        {
            if (value.Count != 10)
            {
                throw new ArgumentException(
                    $"Digit sprite list has the wrong number of sprites (expected 10, got {value.Count}).",
                    nameof(value)
                );
            }
            if (this.digitSprites.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.DigitSprites));
            }
        }
    }

    /// <summary>
    /// The number to display.
    /// </summary>
    public int Number
    {
        get => this.number.Value;
        set
        {
            if (this.number.SetIfChanged(value))
            {
                this.digits = [.. GetDigits(value)];
                this.OnPropertyChanged(nameof(this.Number));
            }
        }
    }

    /// <summary>
    /// Scale to draw the digits, relative to their original pixel size.
    /// </summary>
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

    private readonly DirtyTracker<IReadOnlyList<Sprite>> digitSprites = new(UiSprites.Digits);
    private readonly DirtyTracker<int> number = new(0);
    private readonly DirtyTracker<float> scale = new(2.0f);

    private Rectangle[] digitRects = [];
    private int[] digits = [];

    /// <inheritdoc />
    protected override bool IsContentDirty()
    {
        return this.digitSprites.IsDirty || this.number.IsDirty || this.scale.IsDirty;
    }

    /// <inheritdoc />
    protected override void OnDrawContent(ISpriteBatch b)
    {
        var digitSprites = this.digitSprites.Value;
        for (int i = 0; i < this.digits.Length; i++)
        {
            var digitSprite = digitSprites[this.digits[i]];
            var destinationRect = this.digitRects[i];
            b.Draw(digitSprite.Texture, destinationRect, digitSprite.SourceRect, Color.White);
        }
    }

    /// <inheritdoc />
    protected override void OnMeasure(Vector2 availableSize)
    {
        var digitSprites = this.digitSprites.Value;
        int totalWidth = 0;
        int maxHeight = 0;
        this.digitRects = new Rectangle[this.digits.Length];
        for (int i = 0; i < this.digits.Length; i++)
        {
            var digitSprite = digitSprites[this.digits[i]];
            var size = digitSprite.SourceRect?.Size ?? digitSprite.Texture.Bounds.Size;
            int digitWidth = (int)(size.X * this.Scale);
            int digitHeight = (int)(size.Y * this.Scale);
            this.digitRects[i] = new(totalWidth, 0, digitWidth, digitHeight);
            totalWidth += digitWidth;
            maxHeight = Math.Max(maxHeight, digitHeight);
        }

        this.ContentSize = this.Layout.Resolve(availableSize, () => new(totalWidth, maxHeight));
    }

    /// <inheritdoc />
    protected override void ResetDirty()
    {
        this.digitSprites.ResetDirty();
        this.number.ResetDirty();
        this.scale.ResetDirty();
    }

    private static int GetDigit(int number, int position)
    {
        // Switch-based solution isn't elegant, but is vastly more performant than involving any exponential/logarithmic
        // or floating-point arithmetic, and slightly faster than a lookup table since the compiler can optimize the
        // divisions to multiplications.
        int n = position switch
        {
            0 => number,
            1 => number / 10,
            2 => number / 100,
            3 => number / 1000,
            4 => number / 10_000,
            5 => number / 100_000,
            6 => number / 1_000_000,
            7 => number / 10_000_000,
            8 => number / 100_000_000,
            9 => number / 1_000_000_000,
            _ => throw new ArgumentOutOfRangeException(
                nameof(position),
                $"{position} is not a valid digit position for an integer."
            ),
        };
        return n % 10;
    }

    private static IEnumerable<int> GetDigits(int number)
    {
        bool hasYielded = false;
        for (int i = 9; i >= 0; i--)
        {
            int digit = GetDigit(number, i);
            if (digit == 0 && !hasYielded)
            {
                continue;
            }
            yield return digit;
            hasYielded = true;
        }
    }
}
