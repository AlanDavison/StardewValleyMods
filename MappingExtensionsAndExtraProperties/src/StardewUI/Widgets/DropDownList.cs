using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewUI.Events;
using StardewUI.Graphics;
using StardewUI.Layout;
using StardewUI.Overlays;
using StardewValley;

namespace StardewUI.Widgets;

/// <summary>
/// Button/text field with a drop-down menu.
/// </summary>
/// <typeparam name="T">The type of list item that can be chosen.</typeparam>
public class DropDownList<T> : ComponentView
    where T : notnull
{
    /// <summary>
    /// Event raised when an item in the list is selected.
    /// </summary>
    public event EventHandler<EventArgs>? Select;

    /// <summary>
    /// Specifies how to format the <see cref="SelectedOption"/> in the label text.
    /// </summary>
    public Func<T, string>? OptionFormat
    {
        get => this.optionFormat;
        set
        {
            if (value != this.optionFormat)
            {
                this.optionFormat = value ?? defaultOptionFormat;
                this.UpdateOptions();
                this.UpdateSelectedOption();
                this.OnPropertyChanged(nameof(this.OptionFormat));
            }
        }
    }

    /// <summary>
    /// Minimum width for the text area of an option.
    /// </summary>
    /// <remarks>
    /// If this is <c>0</c>, or if all options are larger, then the dropdown will expand horizontally.
    /// </remarks>
    public float OptionMinWidth
    {
        get => this.selectionFrame.Layout.MinWidth ?? 0;
        set
        {
            if (value != (this.selectionFrame.Layout.MinWidth ?? 0))
            {
                this.selectionFrame.Layout = new()
                {
                    Width = Length.Content(),
                    Height = Length.Content(),
                    MinWidth = value,
                };
                this.OnPropertyChanged(nameof(this.OptionMinWidth));
            }
        }
    }

    /// <summary>
    /// The options available to select.
    /// </summary>
    public IReadOnlyList<T> Options
    {
        get => this.options;
        set
        {
            if (this.options.SetItems(value))
            {
                this.OnPropertyChanged(nameof(this.Options));
            }
        }
    }

    /// <summary>
    /// Index of the currently-selected option in the <see cref="Options"/>, or <c>-1</c> if none selected.
    /// </summary>
    public int SelectedIndex
    {
        get => this.selectedIndex;
        set
        {
            int validIndex = this.options.Count > 0 ? Math.Clamp(value, -1, this.options.Count - 1) : -1;
            if (validIndex == this.selectedIndex)
            {
                return;
            }

            this.selectedIndex = validIndex;
            this.UpdateSelectedOption();
            this.Select?.Invoke(this, EventArgs.Empty);
            this.OnPropertyChanged(nameof(this.SelectedIndex));
            this.OnPropertyChanged(nameof(this.SelectedOption));
            this.OnPropertyChanged(nameof(this.SelectedOptionText));
        }
    }

    /// <summary>
    /// The option that is currently selected, or <c>null</c> if there is no selection.
    /// </summary>
    public T? SelectedOption
    {
        get => this.SelectedIndex >= 0 && this.SelectedIndex < this.options.Count ? this.options[this.SelectedIndex] : default;
        set => this.SelectedIndex = value is not null ? this.options.IndexOf(value) : -1;
    }

    /// <summary>
    /// The text of the currently-selected option.
    /// </summary>
    public string SelectedOptionText
    {
        get => this.selectedOptionLabel.Text;
    }

    private static readonly Func<T, string> defaultOptionFormat = v => v.ToString() ?? "";

    private readonly DirtyTrackingList<T> options = [];

    private IOverlay? overlay;
    private Func<T, string> optionFormat = defaultOptionFormat;
    private int selectedIndex = -1;

    // Initialized in CreateView
    private Lane optionsLane = null!;
    private DropDownOverlayView overlayView = null!;
    private Label selectedOptionLabel = null!;
    private Frame selectionFrame = null!;

    /// <inheritdoc />
    public override bool Measure(Vector2 availableSize)
    {
        if (this.options.IsDirty)
        {
            this.UpdateOptions();
            this.options.ResetDirty();
        }
        bool wasDirty = base.Measure(availableSize);
        if (wasDirty)
        {
            this.overlayView.Layout = new LayoutParameters()
            {
                // Subtract padding from width.
                Width = Length.Px(this.selectionFrame.OuterSize.X - 4),
                Height = Length.Content(),
            };
        }
        return wasDirty;
    }

    /// <inheritdoc />
    protected override IView CreateView()
    {
        // Overlay
        this.optionsLane = new Lane() { Layout = LayoutParameters.AutoRow(), Orientation = Orientation.Vertical };
        this.UpdateOptions();
        this.overlayView = new(this);

        // Always visible
        this.selectedOptionLabel = Label.Simple("");
        this.selectionFrame = new Frame()
        {
            Layout = LayoutParameters.FitContent(),
            Padding = new(8, 4),
            Background = UiSprites.DropDownBackground,
            Content = this.selectedOptionLabel,
        };
        var button = new Image()
        {
            Layout = new() { Width = Length.Content(), Height = Length.Stretch() },
            Sprite = UiSprites.DropDownButton,
            Focusable = true,
        };
        var lane = new Lane() { Layout = LayoutParameters.FitContent(), Children = [this.selectionFrame, button] };
        lane.LeftClick += this.Lane_LeftClick;
        return lane;
    }

    private void Lane_LeftClick(object? sender, ClickEventArgs e)
    {
        if (!e.IsPrimaryButton())
        {
            return;
        }

        this.ToggleOverlay();
    }

    private void OptionView_LeftClick(object? sender, ClickEventArgs e)
    {
        if (!e.IsPrimaryButton() || sender is not DropDownOptionView optionView)
        {
            return;
        }
        Game1.playSound("drumkit6");
        this.RemoveOverlay();
        this.SelectedOption = optionView.Value;
    }

    private void OptionView_Select(object? sender, EventArgs e)
    {
        this.SetSelectedOptionView(view => view == sender);
    }

    private bool RemoveOverlay()
    {
        if (this.overlay is null)
        {
            return false;
        }
        Overlay.Remove(this.overlay);
        this.overlay = null;
        return true;
    }

    private void SetSelectedOptionView(Predicate<DropDownOptionView> predicate)
    {
        foreach (var optionView in this.optionsLane.Children.OfType<DropDownOptionView>())
        {
            optionView.IsSelected = predicate(optionView);
        }
    }

    private void ToggleOverlay()
    {
        if (this.RemoveOverlay())
        {
            return;
        }
        var selectedOption = this.SelectedOption;
        this.SetSelectedOptionView(view => Equals(view.Value, selectedOption));
        Game1.playSound("shwip");
        this.overlay = new Overlay(this.overlayView,
            this,
            horizontalAlignment: Alignment.Start,
            horizontalParentAlignment: Alignment.Start,
            verticalAlignment: Alignment.Start,
            verticalParentAlignment: Alignment.End
        ).OnClose(() => this.overlay = null);
        Overlay.Push(this.overlay);
    }

    private void UpdateOptions()
    {
        if (this.optionsLane is null)
        {
            return;
        }

        this.optionsLane.Children = this.options
            .Select(
                (option, i) =>
                {
                    var optionView = new DropDownOptionView(option, this.optionFormat(option))
                    {
                        IsSelected = this.SelectedIndex == i,
                    };
                    optionView.LeftClick += this.OptionView_LeftClick;
                    optionView.Select += this.OptionView_Select;
                    return optionView as IView;
                }
            )
            .ToList();
    }

    private void UpdateSelectedOption()
    {
        if (this.selectedOptionLabel is null)
        {
            return;
        }

        this.selectedOptionLabel.Text = this.SelectedOption is not null ? this.optionFormat(this.SelectedOption) : "";
    }

    class DropDownOptionView(T value, string text) : ComponentView<Frame>
    {
        public event EventHandler<EventArgs>? Select;

        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (this.isSelected == value)
                {
                    return;
                }

                this.isSelected = value;
                this.View.BackgroundTint = this.GetBackgroundTint();
            }
        }

        public T Value { get; } = value;

        private bool isSelected;

        protected override Frame CreateView()
        {
            var label = Label.Simple(text);
            var frame = new Frame()
            {
                Layout = LayoutParameters.AutoRow(),
                Padding = new(4),
                Background = new(Game1.staminaRect),
                BackgroundTint = this.GetBackgroundTint(),
                Content = label,
                Focusable = true,
            };
            frame.PointerEnter += this.Frame_PointerEnter;
            return frame;
        }

        private void Frame_PointerEnter(object? sender, PointerEventArgs e)
        {
            this.IsSelected = true;
            this.Select?.Invoke(this, EventArgs.Empty);
        }

        private Color GetBackgroundTint()
        {
            return this.isSelected ? new(Color.White, 0.35f) : Color.Transparent;
        }
    }

    class DropDownOverlayView(DropDownList<T> owner) : ComponentView
    {
        public override ViewChild? GetDefaultFocusChild()
        {
            return owner
                .optionsLane.GetChildren()
                .Where(child => child.View is DropDownOptionView optionView && optionView.IsSelected)
                .FirstOrDefault();
        }

        protected override IView CreateView()
        {
            return new Frame()
            {
                Layout = LayoutParameters.AutoRow(),
                Background = UiSprites.DropDownBackground,
                Padding = new(4),
                Content = owner.optionsLane,
            };
        }
    }
}
