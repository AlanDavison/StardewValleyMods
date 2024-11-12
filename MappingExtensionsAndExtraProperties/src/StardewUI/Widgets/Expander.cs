using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewUI.Events;
using StardewUI.Graphics;
using StardewUI.Layout;

namespace StardewUI.Widgets;

/// <summary>
/// A widget that can be clicked to expand/collapse with additional content.
/// </summary>
public class Expander : ComponentView
{
    /// <summary>
    /// Event that fires when the <see cref="IsExpanded"/> property is changed, either externally
    /// or by clicking on the header.
    /// </summary>
    public event EventHandler<EventArgs>? ExpandedChange;

    /// <summary>
    /// The main content, displayed when expanded.
    /// </summary>
    public IView? Content
    {
        get => this.contentFrame.Content;
        set
        {
            if (value != this.contentFrame.Content)
            {
                this.contentFrame.Content = value;
                this.UpdateContent();
                this.OnPropertyChanged(nameof(this.Content));
            }
        }
    }

    /// <summary>
    /// Sprite to show next to the header when collapsed.
    /// </summary>
    public Sprite? CollapsedSprite
    {
        get => this.collapsedSprite;
        set
        {
            if (value != this.collapsedSprite)
            {
                this.collapsedSprite = value;
                this.OnPropertyChanged(nameof(this.CollapsedSprite));
            }
        }
    }

    /// <summary>
    /// Sprite to show next to the header when expanded.
    /// </summary>
    /// <remarks>
    /// If this is <c>null</c>, and <see cref="CollapsedSprite"/> is not null, then the
    /// <see cref="CollapsedSprite"/> will be rotated clockwise on expansion.
    /// </remarks>
    public Sprite? ExpandedSprite
    {
        get => this.expandedSprite;
        set
        {
            if (value != this.expandedSprite)
            {
                this.expandedSprite = value;
                this.OnPropertyChanged(nameof(this.ExpandedSprite));
            }
        }
    }

    /// <summary>
    /// The primary content, which displays inside the menu frame and is clipped/scrollable.
    /// </summary>
    [Outlet("Header")]
    public IView? Header
    {
        get => this.headerLane.Children.ElementAtOrDefault(1);
        set
        {
            if (value != this.headerLane.Children.ElementAtOrDefault(1))
            {
                this.headerLane.Children = value is not null ? [this.indicator, value] : [this.indicator];
                this.OnPropertyChanged(nameof(this.Header));
            }
        }
    }

    /// <summary>
    /// Background sprite to display around the <see cref="Header"/> and expansion indicator.
    /// </summary>
    public Sprite? HeaderBackground
    {
        get => this.headerFrame.Background;
        set
        {
            if (value != this.headerFrame.Background)
            {
                this.headerFrame.Background = value;
                this.OnPropertyChanged(nameof(this.HeaderBackground));
            }
        }
    }

    /// <summary>
    /// Tint color for the <see cref="HeaderBackground"/>.
    /// </summary>
    public Color HeaderBackgroundTint
    {
        get => this.headerFrame.BackgroundTint;
        set
        {
            if (value != this.headerFrame.BackgroundTint)
            {
                this.headerFrame.BackgroundTint = value;
                this.OnPropertyChanged(nameof(this.HeaderBackgroundTint));
            }
        }
    }

    /// <summary>
    /// Configures the layout of the header lane that includes the indicator and
    /// <see cref="Header"/> content.
    /// </summary>
    public LayoutParameters HeaderLayout
    {
        get => this.headerFrame.Layout;
        set
        {
            if (value != this.headerFrame.Layout)
            {
                this.headerFrame.Layout = value;
                this.OnPropertyChanged(nameof(this.HeaderLayout));
            }
        }
    }

    /// <summary>
    /// Padding to apply between the header border and content (including indicator).
    /// </summary>
    public Edges HeaderPadding
    {
        get => this.headerFrame.Padding;
        set
        {
            if (value != this.headerFrame.Padding)
            {
                this.headerFrame.Padding = value;
                this.OnPropertyChanged(nameof(this.HeaderPadding));
            }
        }
    }

    /// <summary>
    /// Whether or not the view is expanded, i.e. whether or not to display the
    /// <see cref="Content"/>.
    /// </summary>
    public bool IsExpanded
    {
        get => this.isExpanded;
        set
        {
            if (value == this.isExpanded)
            {
                return;
            }

            this.isExpanded = value;
            this.UpdateContent();
            this.ExpandedChange?.Invoke(this, EventArgs.Empty);
            this.OnPropertyChanged(nameof(this.IsExpanded));
        }
    }

    /// <summary>
    /// Margin around the entire widget. Same behavior as <see cref="View.Margin"/>.
    /// </summary>
    public Edges Margin
    {
        get => this.layout.Margin;
        // "layout" is the root view so no OnPropertyChanged is required here.
        set => this.layout.Margin = value;
    }

    private Sprite? collapsedSprite = UiSprites.CaretRight;
    private Sprite? expandedSprite;
    private bool isExpanded;

    // Initialized in CreateView
    private Frame contentFrame = null!;
    private Frame headerFrame = null!;
    private Lane headerLane = null!;
    private Image indicator = null!;
    private Lane layout = null!;

    /// <inheritdoc />
    protected override IView CreateView()
    {
        this.indicator = new Image()
        {
            Name = "ExpanderIndicator",
            Layout = new() { Width = Length.Content(), Height = Length.Content() },
            Margin = new(Left: 8, Right: 16),
            HorizontalAlignment = Alignment.Middle,
            VerticalAlignment = Alignment.Middle,
        };
        this.headerLane = new Lane()
        {
            Name = "ExpanderHeaderLane",
            Layout = LayoutParameters.FitContent(),
            VerticalContentAlignment = Alignment.Middle,
            Children = [this.indicator],
        };
        this.headerFrame = new Frame()
        {
            Name = "ExpanderHeaderFrame",
            Layout = LayoutParameters.AutoRow(),
            Content = this.headerLane,
            Focusable = true,
        };
        this.headerFrame.LeftClick += this.HeaderFrame_LeftClick;
        this.contentFrame = new Frame() { Name = "ExpanderContentFrame", Layout = LayoutParameters.FitContent() };
        this.layout = new Lane()
        {
            Name = "ExpanderLayout",
            Layout = LayoutParameters.FitContent(),
            Orientation = Orientation.Vertical,
        };
        this.UpdateContent();
        return this.layout;
    }

    private void HeaderFrame_LeftClick(object? sender, ClickEventArgs e)
    {
        this.IsExpanded = !this.IsExpanded;
    }

    private void UpdateContent()
    {
        if (this.layout is null || this.indicator is null)
        {
            return;
        }

        this.indicator.Sprite = this.isExpanded && this.ExpandedSprite is not null ? this.ExpandedSprite : this.CollapsedSprite;
        this.indicator.Rotation = this.isExpanded && this.ExpandedSprite is null ? SimpleRotation.QuarterClockwise : null;
        this.layout.Children = this.isExpanded ? [this.headerFrame, this.contentFrame] : [this.headerFrame];
    }
}
