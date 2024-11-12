using System;
using Microsoft.Xna.Framework;
using StardewUI.Events;
using StardewUI.Graphics;
using StardewUI.Layout;
using StardewValley;

namespace StardewUI.Widgets;

/// <summary>
/// A togglable checkbox.
/// </summary>
public class CheckBox : ComponentView<Lane>
{
    /// <summary>
    /// Event raised when the checked state changes.
    /// </summary>
    public event EventHandler<EventArgs>? Change;

    /// <summary>
    /// Sprite to display when the box is checked, if not using the default.
    /// </summary>
    public Sprite? CheckedSprite
    {
        get => this.checkedSprite;
        set
        {
            if (value == this.checkedSprite)
            {
                return;
            }

            this.checkedSprite = value;
            this.UpdateCheckImage();
            this.OnPropertyChanged(nameof(this.CheckedSprite));
        }
    }

    /// <summary>
    /// Whether or not the box is checked.
    /// </summary>
    public bool IsChecked
    {
        get => this.isChecked;
        set
        {
            if (value == this.isChecked)
            {
                return;
            }

            this.isChecked = value;
            this.UpdateCheckImage();
            this.Change?.Invoke(this, EventArgs.Empty);
            this.OnPropertyChanged(nameof(this.IsChecked));
        }
    }

    /// <summary>
    /// Color with which to render any <see cref="LabelText"/>.
    /// </summary>
    public Color LabelColor
    {
        get => this.label.Color;
        set
        {
            if (value != this.label.Color)
            {
                this.label.Color = value;
                this.OnPropertyChanged(nameof(this.LabelColor));
            }
        }
    }

    /// <summary>
    /// Optional label text to be displayed to the right of the checkbox image.
    /// </summary>
    /// <remarks>
    /// The label text is clickable as part of the checkbox, but does not receive focus.
    /// </remarks>
    public string LabelText
    {
        get => this.label.Text;
        set
        {
            if (value != this.label.Text)
            {
                this.label.Text = value;
                this.View.Children = !string.IsNullOrEmpty(value) ? [this.checkImage, this.label] : [this.checkImage];
                this.OnPropertyChanged(nameof(this.LabelText));
            }
        }
    }

    /// <summary>
    /// Sprite to display when the box is unchecked, if not using the default.
    /// </summary>
    public Sprite? UncheckedSprite
    {
        get => this.uncheckedSprite;
        set
        {
            if (value == this.uncheckedSprite)
            {
                return;
            }

            this.uncheckedSprite = value;
            this.UpdateCheckImage();
            this.OnPropertyChanged(nameof(this.UncheckedSprite));
        }
    }

    private Sprite? checkedSprite;
    private bool isChecked;
    private Sprite? uncheckedSprite;

    // Initialized in CreateView
    private Image checkImage = null!;
    private Label label = null!;

    /// <inheritdoc />
    protected override Lane CreateView()
    {
        this.label = new Label() { Layout = LayoutParameters.FitContent(), Margin = new(Left: 12) };
        this.checkImage = new Image() { Layout = LayoutParameters.FitContent(), Focusable = true };
        this.UpdateCheckImage();
        var lane = new Lane()
        {
            Layout = LayoutParameters.FitContent(),
            VerticalContentAlignment = Alignment.Middle,
            Children = [this.checkImage],
        };
        lane.LeftClick += this.Lane_LeftClick;
        return lane;
    }

    private void Lane_LeftClick(object? sender, ClickEventArgs e)
    {
        Game1.playSound("drumkit6");
        this.IsChecked = !this.IsChecked;
    }

    private void UpdateCheckImage()
    {
        if (this.checkImage is null)
        {
            return;
        }

        this.checkImage.Sprite = this.IsChecked
            ? this.checkedSprite ?? UiSprites.CheckboxChecked
            : this.uncheckedSprite ?? UiSprites.CheckboxUnchecked;
    }
}
