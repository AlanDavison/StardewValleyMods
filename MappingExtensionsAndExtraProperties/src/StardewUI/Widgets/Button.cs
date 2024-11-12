using Microsoft.Xna.Framework.Graphics;
using StardewUI.Events;
using StardewUI.Graphics;
using StardewUI.Layout;
using StardewValley;

namespace StardewUI.Widgets;

/// <summary>
/// Simple button with optional hover background.
/// </summary>
public class Button : ComponentView<View>
{
    /// <summary>
    /// Content view to display inside the button frame.
    /// </summary>
    public IView? Content
    {
        get => this.contentFrame.Content;
        set
        {
            if (value != this.contentFrame.Content)
            {
                this.contentFrame.Content = value;
                this.OnPropertyChanged(nameof(this.Content));
            }
        }
    }

    /// <summary>
    /// The default background to show for the button's idle state.
    /// </summary>
    public Sprite? DefaultBackground
    {
        get => this.defaultBackgroundSprite;
        set
        {
            if (value == this.defaultBackgroundSprite)
            {
                return;
            }

            this.defaultBackgroundSprite = value;
            this.UpdateBackgroundImage();
            this.OnPropertyChanged(nameof(this.DefaultBackground));
        }
    }

    /// <summary>
    /// Font with which to render button text.
    /// </summary>
    /// <remarks>
    /// This setting only applies when the <see cref="Content"/> view is a <see cref="Label"/>, either via passing in
    /// a <see cref="Label"/> directly or by setting <see cref="Text"/>.
    /// </remarks>
    public SpriteFont Font
    {
        get => this.font;
        set
        {
            if (value == this.font)
            {
                return;
            }

            this.font = value;
            if (this.contentFrame.Content is Label label)
            {
                label.Font = this.font;
            }

            this.OnPropertyChanged(nameof(this.Font));
        }
    }

    /// <summary>
    /// Alternate background sprite when the button has cursor focus.
    /// </summary>
    public Sprite? HoverBackground
    {
        get => this.hoverBackgroundSprite;
        set
        {
            if (value == this.hoverBackgroundSprite)
            {
                return;
            }

            this.hoverBackgroundSprite = value;
            this.UpdateBackgroundImage();
            this.OnPropertyChanged(nameof(this.HoverBackground));
        }
    }

    /// <summary>
    /// Margin to add outside the button.
    /// </summary>
    public Edges Margin
    {
        get => this.View.Margin;
        set => this.View.Margin = value;
    }

    /// <summary>
    /// Whether or not to display a drop shadow for the button frame. Default <c>false</c>.
    /// </summary>
    public bool ShadowVisible
    {
        get => this.backgroundImage.ShadowAlpha > 0;
        set
        {
            if (value != this.backgroundImage.ShadowAlpha > 0)
            {
                this.backgroundImage.ShadowAlpha = value ? 0.5f : 0f;
                this.OnPropertyChanged(nameof(this.ShadowVisible));
            }
        }
    }

    /// <summary>
    /// Text to display inside the button.
    /// </summary>
    /// <remarks>
    /// If the <see cref="Content"/> is not a <see cref="Label"/> then this is always <c>null</c>, even if there is a
    /// label nested somewhere inside a different type of view. Setting this to any string value will <b>replace</b> the
    /// <see cref="Content"/> view with a <see cref="Label"/> having the specified text.
    /// </remarks>
    public string? Text
    {
        get => this.contentFrame.Content is Label label ? label.Text : null;
        set
        {
            if (this.contentFrame.Content is Label label)
            {
                if ((value ?? "") != label.Text)
                {
                    label.Text = value ?? "";
                    this.OnPropertyChanged(nameof(this.Text));
                }
            }
            else if (value is not null)
            {
                this.contentFrame.Content = Label.Simple(value, this.font);
                this.OnPropertyChanged(nameof(this.Text));
            }
        }
    }

    private Sprite? defaultBackgroundSprite;
    private SpriteFont font = Game1.smallFont;
    private Sprite? hoverBackgroundSprite;
    private bool lastHoverState;

    // Initialized in CreateView
    private Image backgroundImage = null!;
    private Frame contentFrame = null!;

    /// <inheritdoc />
    protected override View CreateView()
    {
        this.backgroundImage = new Image()
        {
            Layout = LayoutParameters.Fill(),
            Fit = ImageFit.Stretch,
            ShadowOffset = new(-4, 4),
        };
        this.UpdateBackgroundImage(false);
        this.contentFrame = new Frame() { Layout = LayoutParameters.FitContent(), Margin = new(16, 12) };
        var panel = new Panel()
        {
            Layout = new()
            {
                Width = Length.Content(),
                Height = Length.Content(),
                MinWidth = 64,
                MinHeight = 32,
            },
            HorizontalContentAlignment = Alignment.Middle,
            VerticalContentAlignment = Alignment.Middle,
            Children = [this.backgroundImage, this.contentFrame],
            Focusable = true,
        };
        panel.PointerEnter += this.Panel_PointerEnter;
        panel.PointerLeave += this.Panel_PointerLeave;
        return panel;
    }

    private void Panel_PointerEnter(object? sender, PointerEventArgs e)
    {
        this.UpdateBackgroundImage(true);
    }

    private void Panel_PointerLeave(object? sender, PointerEventArgs e)
    {
        this.UpdateBackgroundImage(false);
    }

    private void UpdateBackgroundImage(bool? hover = null)
    {
        if (hover.HasValue)
        {
            this.lastHoverState = hover.Value;
        }
        else
        {
            hover = this.lastHoverState;
        }
        if (this.backgroundImage is null)
        {
            return;
        }

        this.backgroundImage.Sprite = hover.Value
            ? this.HoverBackground ?? this.DefaultBackground ?? UiSprites.ButtonDark
            : this.DefaultBackground ?? UiSprites.ButtonDark;
    }
}
