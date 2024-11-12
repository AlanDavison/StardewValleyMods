using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewUI.Events;
using StardewUI.Graphics;
using StardewUI.Layout;
using StardewUI.Overlays;
using StardewValley;

namespace StardewUI.Widgets.Keybinding;

/// <summary>
/// Editor widget for a <see cref="KeybindList"/>.
/// </summary>
/// <remarks>
/// Displays all the configured keybinds in one row, and (<see cref="EditableType"/> is not <c>null</c>) opens up a
/// <see cref="KeybindOverlay"/> to edit the keybinds when clicked.
/// </remarks>
public class KeybindListEditor : ComponentView
{
    /// <summary>
    /// The <see cref="KeybindOverlay.AddButtonText"/> to display for adding new bindings.
    /// </summary>
    /// <remarks>
    /// Changing this while the overlay is already displayed will not update the overlay.
    /// </remarks>
    public string AddButtonText
    {
        get => this.addButtonText;
        set
        {
            if (value != this.addButtonText)
            {
                this.addButtonText = value;
                this.OnPropertyChanged(nameof(this.AddButtonText));
            }
        }
    }

    /// <summary>
    /// The height for button images/sprites. Images are scaled uniformly, preserving source aspect ratio.
    /// </summary>
    public int ButtonHeight
    {
        get => this.buttonHeight;
        set
        {
            if (value == this.buttonHeight)
            {
                return;
            }

            this.buttonHeight = value;
            foreach (var keybindView in this.KeybindViews)
            {
                keybindView.ButtonHeight = value;
            }

            this.OnPropertyChanged(nameof(this.ButtonHeight));
        }
    }

    /// <summary>
    /// The <see cref="KeybindOverlay.DeleteButtonTooltip"/> to display for deleting bindings in a multiple-binding UI.
    /// </summary>
    /// <remarks>
    /// Changing this while the overlay is already displayed will not update the overlay.
    /// </remarks>
    public string DeleteButtonTooltip
    {
        get => this.deleteButtonTooltip;
        set
        {
            if (value != this.deleteButtonTooltip)
            {
                this.deleteButtonTooltip = value;
                this.OnPropertyChanged(nameof(this.DeleteButtonTooltip));
            }
        }
    }

    /// <summary>
    /// Specifies what kind of keybind the editor should allow.
    /// </summary>
    /// <remarks>
    /// The current value <see cref="KeybindList"/> is always fully displayed, even if it does not conform to the
    /// semantic type. It is up to the caller to ensure that the value initially assigned to the editor is of the
    /// correct kind. If this is <c>null</c>, the list is considered read-only.
    /// </remarks>
    public KeybindType? EditableType
    {
        get => this.editableType;
        set
        {
            if (value != this.editableType)
            {
                this.editableType = value;
                this.OnPropertyChanged(nameof(this.EditableType));
            }
        }
    }

    /// <summary>
    /// Placeholder text to display when the current keybind list is empty.
    /// </summary>
    public string EmptyText
    {
        get => this.emptyText;
        set
        {
            if (value == this.emptyText)
            {
                return;
            }

            this.emptyText = value;
            this.UpdateEmptyText(this.EmptyTextColor);
            this.OnPropertyChanged(nameof(this.EmptyText));
        }
    }

    /// <summary>
    /// Color of the displayed <see cref="EmptyText"/> when the list is empty.
    /// </summary>
    public Color EmptyTextColor
    {
        get => this.emptyTextColor;
        set
        {
            if (value == this.emptyTextColor)
            {
                return;
            }

            this.emptyTextColor = value;
            this.UpdateEmptyText(this.EmptyTextColor);
            this.OnPropertyChanged(nameof(this.EmptyTextColor));
        }
    }

    /// <inheritdoc cref="View.Focusable" />
    public bool Focusable
    {
        get => this.rootLane.Focusable;
        set => this.rootLane.Focusable = value;
    }

    /// <summary>
    /// Font used to display text in button/key placeholders.
    /// </summary>
    /// <remarks>
    /// Only applies for buttons that use a placeholder sprite (i.e. set the <c>isPlaceholder</c> output of
    /// <see cref="ISpriteMap{T}.Get(T, out bool)"/> to <c>true</c>). In these cases, the actual button text drawn
    /// inside the sprite will be drawn using the specified font.
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
            foreach (var keybindView in this.KeybindViews)
            {
                keybindView.Font = value;
            }

            this.OnPropertyChanged(nameof(this.Font));
        }
    }

    /// <summary>
    /// The current keybinds to display in the list.
    /// </summary>
    /// <remarks>
    /// Changing these while the overlay is open may not update the overlay.
    /// </remarks>
    public KeybindList KeybindList
    {
        get => this.keybindList;
        set
        {
            if (value == this.keybindList)
            {
                return;
            }

            this.keybindList = value;
            this.UpdateAll();
            this.OnPropertyChanged(nameof(this.KeybindList));
        }
    }

    /// <summary>
    /// Map of bindable buttons to sprite representations.
    /// </summary>
    public ISpriteMap<SButton>? SpriteMap
    {
        get => this.spriteMap;
        set
        {
            if (value == this.spriteMap)
            {
                return;
            }

            this.spriteMap = value;
            foreach (var keybindView in this.KeybindViews)
            {
                keybindView.SpriteMap = value;
            }

            this.OnPropertyChanged(nameof(this.SpriteMap));
        }
    }

    private IEnumerable<KeybindView> KeybindViews => this.rootLane.Children.OfType<Frame>().Select(frame => frame.Content).OfType<KeybindView>();

    private readonly Lane rootLane = new();

    private string addButtonText = "";
    private int buttonHeight = KeybindView.DEFAULT_BUTTON_HEIGHT;
    private string deleteButtonTooltip = "";
    private KeybindType? editableType;
    private string emptyText = "";
    private Color emptyTextColor = Game1.textColor;
    private SpriteFont font = Game1.smallFont;
    private KeybindList keybindList = new();
    private ISpriteMap<SButton>? spriteMap;

    /// <inheritdoc />
    protected override IView CreateView()
    {
        this.rootLane.LeftClick += this.RootLane_LeftClick;
        this.rootLane.RightClick += this.RootLane_RightClick;
        this.rootLane.PointerEnter += this.RootLane_PointerEnter;
        this.rootLane.PointerLeave += this.RootLane_PointerLeave;
        return this.rootLane;
    }

    private void Overlay_Close(object? sender, EventArgs e)
    {
        if (sender is not KeybindOverlay overlay)
        {
            return;
        }

        this.KeybindList = overlay.KeybindList;
        // We generally won't receive the PointerLeave event after a full-screen overlay was open.
        // Known issue due to dependency on recursive PointerMove, hard to resolve.
        this.UpdateTint(Color.White);
        this.UpdateEmptyText(this.EmptyTextColor);
    }

    private void RootLane_LeftClick(object? sender, ClickEventArgs e)
    {
        if (this.EditableType is null)
        {
            return;
        }
        Game1.playSound("bigSelect");
        var overlay = new KeybindOverlay(this.spriteMap)
        {
            AddButtonText = this.AddButtonText,
            DeleteButtonTooltip = this.DeleteButtonTooltip,
            KeybindList = this.KeybindList,
            KeybindType = this.EditableType.Value,
        };
        overlay.Close += this.Overlay_Close;
        Overlay.Push(overlay);
        if (this.EditableType != KeybindType.MultipleKeybinds || !this.KeybindList.IsBound)
        {
            UI.InputHelper.Suppress(e.Button);
            overlay.StartCapturing();
        }
    }

    private void RootLane_PointerEnter(object? sender, PointerEventArgs e)
    {
        if (this.EditableType is null)
        {
            return;
        }

        this.UpdateTint(Color.Orange);
        this.UpdateEmptyText(Color.SaddleBrown);
    }

    private void RootLane_PointerLeave(object? sender, PointerEventArgs e)
    {
        this.UpdateTint(Color.White);
        this.UpdateEmptyText(this.EmptyTextColor);
    }

    private void RootLane_RightClick(object? sender, ClickEventArgs e)
    {
        if (this.EditableType is null)
        {
            return;
        }
        Game1.playSound("drumkit5");
        this.KeybindList = new();
    }

    private void UpdateAll()
    {
        var keybindViews = this.keybindList
            .Keybinds.Where(kb => kb.IsBound)
            .Select(
                (kb, index) =>
                    new Frame()
                    {
                        Layout = LayoutParameters.FitContent(),
                        Margin = index > 0 ? new Edges(Left: 16) : Edges.NONE,
                        Background = UiSprites.MenuSlotTransparent,
                        Padding = UiSprites.MenuSlotTransparent.FixedEdges! + new Edges(4),
                        Content = new KeybindView
                        {
                            ButtonHeight = this.buttonHeight,
                            Font = this.font,
                            Keybind = kb,
                            SpriteMap = this.spriteMap,
                        },
                    }
            )
            .Cast<IView>()
            .ToList();
        this.rootLane.Children = keybindViews.Count > 0 ? keybindViews : [Label.Simple(this.EmptyText)];
    }

    private void UpdateEmptyText(Color color)
    {
        if (this.rootLane.Children.Count == 1 && this.rootLane.Children[0] is Label label)
        {
            label.Text = this.EmptyText;
            label.Color = color;
        }
    }

    private void UpdateTint(Color tintColor)
    {
        foreach (var keybindView in this.KeybindViews)
        {
            keybindView.TintColor = tintColor;
        }
    }
}
