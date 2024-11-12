using System;
using Microsoft.Xna.Framework;
using StardewUI.Events;
using StardewUI.Graphics;
using StardewUI.Input;
using StardewUI.Layout;
using StardewValley;

namespace StardewUI.Widgets;

/// <summary>
/// A horizontal track with draggable thumb (button) for choosing a numeric value in a range.
/// </summary>
public class Slider : ComponentView
{
    private const int DEFAULT_TRACK_WIDTH = 120;
    private const int TRACK_HEIGHT = 24;

    /// <summary>
    /// Event raised when the <see cref="Value"/> changes.
    /// </summary>
    public event EventHandler<EventArgs>? ValueChange;

    /// <summary>
    /// Background or track sprite, if not using the default.
    /// </summary>
    public Sprite? BackgroundSprite
    {
        get => this.backgroundImage.Sprite;
        set => this.backgroundImage.Sprite = value;
    }

    /// <summary>
    /// Sprite for the thumb/button, if not using the default.
    /// </summary>
    public Sprite? ThumbSprite
    {
        get => this.thumbImage.Sprite;
        set => this.thumbImage.Sprite = value;
    }

    /// <summary>
    /// The interval of which <see cref="Value"/> should be a multiple. Affects which values will be hit while dragging.
    /// </summary>
    public float Interval
    {
        get => this.interval;
        set
        {
            if (value != this.interval)
            {
                this.interval = value;
                this.OnPropertyChanged(nameof(this.Interval));
            }
        }
    }

    /// <summary>
    /// The maximum value allowed for <see cref="Value"/>.
    /// </summary>
    public float Max
    {
        get => this.max;
        set
        {
            if (value == this.max)
            {
                return;
            }
            if (this.Min > value)
            {
                this.Min = value;
            }

            this.max = value;
            this.OnPropertyChanged(nameof(this.Max));
            if (this.Value > this.max)
            {
                this.Value = this.max;
            }
        }
    }

    /// <summary>
    /// The minimum value allowed for <see cref="Value"/>.
    /// </summary>
    public float Min
    {
        get => this.min;
        set
        {
            if (value == this.min)
            {
                return;
            }
            if (this.Max < value)
            {
                this.Max = value;
            }

            this.min = value;
            this.OnPropertyChanged(nameof(this.Min));
            if (this.Value < this.min)
            {
                this.Value = this.min;
            }
        }
    }

    /// <summary>
    /// Override for the thumb/button size, recommended when using a custom <see cref="ThumbSprite"/>.
    /// </summary>
    public Vector2? ThumbSize
    {
        get => this.thumbSize;
        set
        {
            if (value != this.thumbSize)
            {
                return;
            }

            this.thumbSize = value;
            this.thumbImage.Layout = GetThumbImageLayout(value);
            this.OnPropertyChanged(nameof(this.ThumbSize));
        }
    }

    /// <summary>
    /// Width of the track bar.
    /// </summary>
    public float TrackWidth
    {
        get => this.sliderPanel.Layout.Width.Value;
        set
        {
            if (this.sliderPanel.Layout.Width.Type != LengthType.Px || this.sliderPanel.Layout.Width.Value != value)
            {
                this.sliderPanel.Layout = LayoutParameters.FixedSize(value, TRACK_HEIGHT);
                this.OnPropertyChanged(nameof(this.TrackWidth));
            }
        }
    }

    /// <summary>
    /// The current value.
    /// </summary>
    public float Value
    {
        get => this.value;
        set
        {
            float clamped = Math.Clamp(value, this.min, this.max);
            if (clamped == this.value)
            {
                return;
            }
            this.value = clamped;
            this.UpdatePosition();
            this.UpdateValueLabel();
            this.ValueChange?.Invoke(this, EventArgs.Empty);
            this.OnPropertyChanged(nameof(this.Value));
        }
    }

    /// <summary>
    /// Color of the value text to render, if overriding the default text color.
    /// </summary>
    public Color? ValueColor
    {
        get => this.valueLabel.Color;
        set
        {
            var color = value ?? Game1.textColor;
            if (color != this.valueLabel.Color)
            {
                this.valueLabel.Color = color;
                this.OnPropertyChanged(nameof(this.ValueColor));
            }
        }
    }

    /// <summary>
    /// Specifies how to format the <see cref="Value"/> in the label text.
    /// </summary>
    public Func<float, string> ValueFormat
    {
        get => this.valueFormat;
        set
        {
            if (value != this.valueFormat)
            {
                this.valueFormat = value;
                this.UpdateValueLabel();
                this.OnPropertyChanged(nameof(this.ValueFormat));
            }
        }
    }

    // Initialized in CreateView
    private Image backgroundImage = null!;
    private Panel sliderPanel = null!;
    private Image thumbImage = null!;
    private Label valueLabel = null!;

    // Refer to the similar implementation in Scrollbar for explanation of the drag cursor offset.
    private float? initialThumbDragCursorOffset;
    private float interval = 0.01f;
    private float max = 1;
    private float min = 0;
    private Vector2? thumbSize;
    private float value = 0;
    private Func<float, string> valueFormat = v => v.ToString();

    /// <inheritdoc />
    public override FocusSearchResult? FocusSearch(Vector2 position, Direction direction)
    {
        if (this.ContentBounds.ContainsPoint(position) && this.TryMoveValue(direction))
        {
            // Fake a focus search "into" the slider so that we keep focus on the thumb.
            return base.FocusSearch(new(0, -1), Direction.South);
        }
        return base.FocusSearch(position, direction);
    }

    /// <inheritdoc />
    protected override IView CreateView()
    {
        this.backgroundImage = new()
        {
            Layout = LayoutParameters.Fill(),
            Sprite = UiSprites.SliderBackground,
            Fit = ImageFit.Stretch,
        };
        this.thumbImage = new()
        {
            Layout = GetThumbImageLayout(this.thumbSize),
            Sprite = UiSprites.SliderButton,
            Fit = ImageFit.Stretch,
            Focusable = true,
            Draggable = true,
            ZIndex = 1,
        };
        this.thumbImage.DragStart += this.Thumb_DragStart;
        this.thumbImage.Drag += this.Thumb_Drag;
        this.thumbImage.DragEnd += this.Thumb_DragEnd;
        this.thumbImage.LeftClick += this.Thumb_LeftClick;
        this.sliderPanel = new Panel()
        {
            Layout = LayoutParameters.FixedSize(DEFAULT_TRACK_WIDTH, TRACK_HEIGHT),
            Children = [this.backgroundImage, this.thumbImage],
        };
        this.sliderPanel.LeftClick += this.Track_LeftClick;
        this.valueLabel = Label.Simple("");
        this.valueLabel.Margin = new(Left: 8);
        this.UpdateValueLabel();
        return new Lane()
        {
            Layout = LayoutParameters.FitContent(),
            VerticalContentAlignment = Alignment.Middle,
            Children = [this.sliderPanel, this.valueLabel],
        };
    }

    /// <inheritdoc />
    protected override void OnLayout()
    {
        this.UpdatePosition();
    }

    private static LayoutParameters GetThumbImageLayout(Vector2? thumbSize)
    {
        return thumbSize.HasValue
            ? LayoutParameters.FixedSize(thumbSize.Value.X, thumbSize.Value.Y)
            : LayoutParameters.FitContent();
    }

    private void SetValueFromProgress(float progress)
    {
        float exactValueAboveMin = progress * (this.Max - this.Min);
        float intervalCount = MathF.Round(exactValueAboveMin / this.Interval);
        float newValue = this.Min + intervalCount * this.Interval;
        if (newValue != this.Value)
        {
            Game1.playSound("stoneStep");
            this.Value = newValue;
        }
    }

    // Thumb-related methods are all a simplification of the Scrollbar thumb. Refer to that implementation for comments
    // and explanations.

    private void Thumb_Drag(object? sender, PointerEventArgs e)
    {
        if (!this.initialThumbDragCursorOffset.HasValue)
        {
            return;
        }

        float trackWidth = this.sliderPanel.InnerSize.X;
        float availableWidth = trackWidth - this.thumbImage.ContentSize.X;
        if (availableWidth <= 0)
        {
            return;
        }

        float targetDistance = e.Position.X - this.initialThumbDragCursorOffset.Value;
        float targetThumbStart = Math.Clamp(targetDistance, 0, availableWidth);
        float progress = targetThumbStart / availableWidth;
        this.SetValueFromProgress(progress);
    }

    private void Thumb_DragEnd(object? sender, PointerEventArgs e)
    {
        this.initialThumbDragCursorOffset = null;
    }

    private void Thumb_DragStart(object? sender, PointerEventArgs e)
    {
        float cursorOffset = e.Position.X - this.thumbImage.Margin.Left;
        this.initialThumbDragCursorOffset = cursorOffset >= 0 ? cursorOffset : null;
    }

    private void Thumb_LeftClick(object? sender, ClickEventArgs e)
    {
        // Prevent clicks on the button from being treated as clicks on the track.
        if (e.Position.X >= this.thumbImage.Margin.Left)
        {
            e.Handled = true;
        }
    }

    private void Track_LeftClick(object? sender, ClickEventArgs e)
    {
        if (!e.IsPrimaryButton())
        {
            return;
        }
        float trackWidth = this.sliderPanel.InnerSize.X;
        float thumbWidth = this.thumbImage.ContentSize.X;
        float progress = Math.Clamp((e.Position.X - thumbWidth / 2) / (trackWidth - thumbWidth), 0, 1);
        this.SetValueFromProgress(progress);
    }

    private bool TryMoveValue(Direction direction)
    {
        float interval = direction switch
        {
            Direction.East => this.Interval,
            Direction.West => -this.Interval,
            _ => 0,
        };
        if (interval == 0)
        {
            return false;
        }
        float nextValue = Math.Clamp(this.Value + interval, this.Min, this.Max);
        if (nextValue != this.Value)
        {
            Game1.playSound("stoneStep");
            this.Value = nextValue;
            return true;
        }
        return false;
    }

    private void UpdatePosition()
    {
        if (this.sliderPanel is null)
        {
            return;
        }
        if (this.Max == this.Min)
        {
            this.thumbImage.Margin = new(0);
            return;
        }
        float trackWidth = this.sliderPanel.ContentSize.X;
        float availableWidth = trackWidth - this.thumbImage.ContentSize.X;
        float progress = (this.Value - this.Min) / (this.Max - this.Min);
        this.thumbImage.Margin = new(Left: (int)Math.Round(availableWidth * progress));
    }

    private void UpdateValueLabel()
    {
        if (this.valueLabel is null)
        {
            return;
        }

        this.valueLabel.Text = this.ValueFormat(this.Value);
    }
}
