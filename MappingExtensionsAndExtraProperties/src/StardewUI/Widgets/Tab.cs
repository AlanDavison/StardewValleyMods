using System;
using Microsoft.Xna.Framework;
using StardewUI.Events;
using StardewUI.Graphics;
using StardewUI.Layout;
using StardewValley;

namespace StardewUI.Widgets;

/// <summary>
/// A view with tab appearance, used to navigate sections of a larger complex menu.
/// </summary>
/// <remarks>
/// Tabs activate when clicked; multiple tabs can be assigned to the same <see cref="Group"/>, each with a unique
/// <see cref="GroupKey"/>, in order to deactivate other tabs when any one tab is activated.
/// </remarks>
public class Tab : ComponentView<Panel>
{
    /// <summary>
    /// Event raised when <see cref="Active"/> becomes <c>true</c>.
    /// </summary>
    public event EventHandler? Activate;

    /// <summary>
    /// Event raised when <see cref="Active"/> becomes <c>false</c>.
    /// </summary>
    public event EventHandler? Deactivate;

    /// <summary>
    /// Whether or not the tab is considered active (selected).
    /// </summary>
    /// <remarks>
    /// Active tabs have an offset appearance, normally used to indicate their "pressed" status in relation to other,
    /// neighboring tabs. The offset can be adjusted with <see cref="ActiveOffset"/>.
    /// </remarks>
    public bool Active
    {
        get => this.isActive;
        set
        {
            if (value != this.isActive)
            {
                this.isActive = value;
                if (this.Group is not null && !string.IsNullOrEmpty(this.GroupKey))
                {
                    this.Group.Key = this.GroupKey;
                }
                if (value)
                {
                    this.Activate?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    this.Deactivate?.Invoke(this, EventArgs.Empty);
                }

                this.OnPropertyChanged(nameof(this.Active));
            }
        }
    }

    /// <summary>
    /// The drawing offset to apply when the tab is <see cref="Active"/>.
    /// </summary>
    public Vector2 ActiveOffset
    {
        get => this.activeOffset;
        set
        {
            if (value != this.activeOffset)
            {
                this.activeOffset = value;
                this.OnPropertyChanged(nameof(this.ActiveOffset));
            }
        }
    }

    /// <summary>
    /// Background image to draw behind the tab's <see cref="Content"/>, which provides the tab appearance.
    /// </summary>
    public Sprite? Background
    {
        get => this.backgroundImage.Sprite;
        set
        {
            if (value != this.backgroundImage.Sprite)
            {
                this.backgroundImage.Sprite = value;
                this.OnPropertyChanged(nameof(this.Background));
            }
        }
    }

    /// <summary>
    /// Rotation of the <see cref="Background"/>; does not apply to <see cref="Content"/>.
    /// </summary>
    public SimpleRotation? BackgroundRotation
    {
        get => this.backgroundImage.Rotation;
        set
        {
            if (this.backgroundImage.Rotation != value)
            {
                this.backgroundImage.Rotation = value;
                this.OnPropertyChanged(nameof(this.BackgroundRotation));
            }
        }
    }

    /// <summary>
    /// Content to draw inside the tab's border.
    /// </summary>
    public IView? Content
    {
        get => this.contentFrame.Content;
        set
        {
            if (this.contentFrame.Content != value)
            {
                this.contentFrame.Content = value;
                this.OnPropertyChanged(nameof(this.Content));
            }
        }
    }

    /// <summary>
    /// Margin to apply to the frame containing the <see cref="Content"/>, i.e. distance between the tab's visual border
    /// and the inner image, text, etc.
    /// </summary>
    /// <remarks>
    /// When using the default <see cref="Background"/>, this is automatically set up to match its border size. If a
    /// different background is used, the margin may need to be adjusted.
    /// </remarks>
    public Edges ContentMargin
    {
        get => this.contentFrame.Margin;
        set
        {
            if (this.contentFrame.Margin != value)
            {
                this.contentFrame.Margin = value;
                this.OnPropertyChanged(nameof(this.ContentMargin));
            }
        }
    }

    /// <summary>
    /// The selection group, if any, to which this tab belongs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Assigning multiple tabs to the same group guarantees that only one can be <see cref="Active"/> at a time. If
    /// this tab becomes active, any previously-active tab will become inactive.
    /// </para>
    /// <para>
    /// To participate in the group, a non-empty <see cref="GroupKey"/> must also be specified.
    /// </para>
    /// </remarks>
    public SelectionGroup? Group
    {
        get => this.group;
        set
        {
            if (value == this.group)
            {
                return;
            }
            if (this.group is not null)
            {
                this.group.Change -= this.Group_Change;
            }

            this.group = value;
            if (value is not null)
            {
                value.Change += this.Group_Change;
            }

            this.OnPropertyChanged(nameof(this.Group));
        }
    }

    /// <summary>
    /// The unique key per <see cref="Group"/> that identifies this tab.
    /// </summary>
    public string GroupKey
    {
        get => this.groupKey;
        set
        {
            if (value != this.groupKey)
            {
                this.groupKey = value;
                this.UpdateGroupDefault();
                this.OnPropertyChanged(nameof(this.GroupKey));
            }
        }
    }

    private readonly Image backgroundImage = new() { Layout = LayoutParameters.Fill(), Sprite = UiSprites.TabTopEmpty };
    private readonly Frame contentFrame =
        new()
        {
            Layout = LayoutParameters.Fill(),
            Margin =
                (UiSprites.TabTopEmpty.FixedEdges ?? Edges.NONE) * (UiSprites.TabTopEmpty.SliceSettings?.Scale ?? 1),
            HorizontalContentAlignment = Alignment.Middle,
            VerticalContentAlignment = Alignment.Middle,
        };

    private Vector2 activeOffset = new(0, 8);
    private bool isActive;
    private SelectionGroup? group;
    private string groupKey = "";

    /// <inheritdoc />
    public override void Draw(ISpriteBatch b)
    {
        if (this.Active)
        {
            b.Translate(this.ActiveOffset);
        }
        base.Draw(b);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        if (this.group is not null)
        {
            this.group.Change -= this.Group_Change;
            this.group = null;
        }
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public override void OnClick(ClickEventArgs e)
    {
        if (e.IsPrimaryButton())
        {
            if (!this.Active)
            {
                this.Active = true;
                Game1.playSound("smallSelect");
            }
            e.Handled = true;
        }
        base.OnClick(e);
    }

    /// <inheritdoc />
    protected override Panel CreateView()
    {
        return new() { Focusable = true, Children = [this.backgroundImage, this.contentFrame] };
    }

    private void Group_Change(object? sender, EventArgs e)
    {
        if (sender != this.Group)
        {
            return;
        }
        if (this.Group is not null && !string.IsNullOrEmpty(this.GroupKey))
        {
            this.Active = this.Group.Key == this.GroupKey;
        }
    }

    private void UpdateGroupDefault()
    {
        if (this.Group is not null && string.IsNullOrEmpty(this.Group.Key) && !string.IsNullOrEmpty(this.GroupKey))
        {
            this.Group.Key = this.GroupKey;
        }
    }
}
