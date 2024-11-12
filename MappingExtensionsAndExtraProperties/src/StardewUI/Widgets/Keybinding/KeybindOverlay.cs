using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewUI.Animation;
using StardewUI.Events;
using StardewUI.Graphics;
using StardewUI.Input;
using StardewUI.Layout;
using StardewUI.Overlays;
using StardewValley;

namespace StardewUI.Widgets.Keybinding;

/// <summary>
/// Overlay control for editing a keybinding, or list of bindings.
/// </summary>
/// <param name="spriteMap">Map of bindable buttons to sprite representations.</param>
public class KeybindOverlay(ISpriteMap<SButton>? spriteMap) : FullScreenOverlay
{
    /// <summary>
    /// Text to display on the button used to add a new binding.
    /// </summary>
    /// <remarks>
    /// If not specified, the button will use a generic "+" image instead.
    /// </remarks>
    public string AddButtonText
    {
        get => this.addButton?.Text ?? "";
        set => this.RequireView(() => this.addButton).Text = value;
    }

    /// <summary>
    /// Tooltip to display for the delete (trash can) button beside each existing binding.
    /// </summary>
    /// <remarks>
    /// If not specified, the delete buttons will have no tooltips.
    /// </remarks>
    public string DeleteButtonTooltip
    {
        get => this.deleteButtonTooltip;
        set
        {
            if (value == this.deleteButtonTooltip)
            {
                return;
            }

            this.deleteButtonTooltip = value;
            foreach (var keybindLane in this.keybindsLane?.Children ?? [])
            {
                if (keybindLane.GetChildren().LastOrDefault()?.View is Image deleteButton)
                {
                    deleteButton.Tooltip = value;
                }
            }
        }
    }

    /// <summary>
    /// The current keybinds to display in the list.
    /// </summary>
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
            this.UpdateKeybinds();
        }
    }

    /// <summary>
    /// Specifies what kind of keybind is being edited.
    /// </summary>
    /// <remarks>
    /// This determines the behavior of the capturing as well as what happens after capture:
    /// <list type="bullet">
    /// <item><see cref="KeybindType.MultipleKeybinds"/> displays the list of existing keybinds (if any), adds the
    /// captured keybind when all buttons/keys are released, and allows adding more;</item>
    /// <item><see cref="KeybindType.SingleKeybind"/> does not display the list or separator, and when all buttons/keys
    /// are released, updates its <see cref="KeybindList"/> to have that single keybind and closes the overlay.</item>
    /// <item><see cref="KeybindType.SingleButton"/> is similar to <see cref="KeybindType.SingleKeybind"/> but records
    /// the keybind and closes the overlay as soon as a single button is pressed.</item>
    /// </list>
    /// Typically when using single-bind or single-button modes, the caller should <see cref="StartCapturing"/> upon
    /// creation of the overlay in order to minimize redundant clicks.
    /// </remarks>
    public KeybindType KeybindType
    {
        get => this.keybindType;
        set
        {
            if (value == this.keybindType)
            {
                return;
            }

            this.keybindType = value;
            this.UpdateKeybindType();
        }
    }

    private static readonly HashSet<SButton> bannedButtons =
    [
        SButton.Escape,
        SButton.LeftThumbstickUp,
        SButton.LeftThumbstickDown,
        SButton.LeftThumbstickLeft,
        SButton.LeftThumbstickRight,
        SButton.RightThumbstickUp,
        SButton.RightThumbstickDown,
        SButton.RightThumbstickLeft,
        SButton.RightThumbstickRight,
    ];

    private readonly HashSet<SButton> capturedButtons = [];

    private readonly Image horizontalDivider =
        new()
        {
            Layout = LayoutParameters.AutoRow(),
            Margin = new(0, 16),
            Fit = ImageFit.Stretch,
            Sprite = UiSprites.GenericHorizontalDivider,
        };

    private KeybindList keybindList = new();
    private string deleteButtonTooltip = "";
    private KeybindType keybindType = KeybindType.MultipleKeybinds;

    // Initialized in CreateView
    private Button addButton = null!;
    private Animator<Frame, Color> capturingAnimator = null!;
    private KeybindView currentKeybindView = null!;
    private Frame keybindEntryHighlighter = null!;
    private Lane keybindsLane = null!;
    private Lane layoutLane = null!;

    /// <summary>
    /// Starts capturing a new keybind.
    /// </summary>
    /// <remarks>
    /// This makes the capture area start flashing and hides the "Add" button; any buttons pressed in the capturing
    /// state are recorded and combined into a single keybind after the capture ends, when all buttons are released.
    /// </remarks>
    public void StartCapturing()
    {
        if (this.CapturingInput)
        {
            return;
        }

        this.addButton.Visibility = Visibility.Hidden;
        this.capturedButtons.Clear();
        this.currentKeybindView.Keybind = new();
        var endColor = Color.DarkOrange;
        var startColor = AlphaLerp(Color.Transparent, endColor, 0.3f);
        this.capturingAnimator.Start(startColor, endColor, TimeSpan.FromSeconds(1));
        this.CapturingInput = true;
    }

    /// <inheritdoc />
    public override void Update(TimeSpan elapsed)
    {
        base.Update(elapsed);
        if (!this.CapturingInput)
        {
            return;
        }
        var cancellationButtons = ButtonResolver.GetActionButtons(ButtonAction.Cancel);
        foreach (var button in cancellationButtons)
        {
            if (UI.InputHelper.IsDown(button))
            {
                this.StopCapturing();
                UI.InputHelper.Suppress(button);
                return;
            }
        }
        var pressedButtons = GetPressedButtons().Where(IsBindable).ToList();
        bool anyPressed = false;
        bool anyChanged = false;
        foreach (var button in pressedButtons)
        {
            anyChanged |= this.capturedButtons.Add(button);
            anyPressed = true;
        }
        if (this.KeybindType == KeybindType.SingleButton && anyPressed)
        {
            anyChanged |= this.capturedButtons.RemoveWhere(button =>
                    !UI.InputHelper.IsDown(button)
                    // Some buttons, like triggers, can behave erratically due to discrepancies between SDV's definition
                    // of "down" vs. SMAPI's, so we add this extra condition to prevent removal of any buttons that SDV
                    // says are still down, even if SMAPI thinks they're not.
                    && !pressedButtons.Contains(button)
                ) > 0;
        }
        if (anyChanged)
        {
            this.currentKeybindView.Keybind = this.KeybindType == KeybindType.SingleButton ? new(this.capturedButtons.First()) : new([.. this.capturedButtons]);
        }
        if (this.capturedButtons.Count > 0 && !anyPressed)
        {
            var capturedKeybind = new Keybind([.. this.capturedButtons]);
            if (capturedKeybind.IsBound)
            {
                this.KeybindList = this.KeybindType == KeybindType.MultipleKeybinds
                        ? new([.. this.KeybindList.Keybinds, capturedKeybind])
                        : new(capturedKeybind);
            }

            this.StopCapturing();
        }
    }

    /// <inheritdoc />
    protected override IView CreateView()
    {
        this.currentKeybindView = new() { SpriteMap = spriteMap };
        this.addButton = this.CreateAddButton();
        var currentKeybindLane = new Lane()
        {
            Layout = new()
            {
                Width = Length.Stretch(),
                Height = Length.Content(),
                MinHeight = 64,
            },
            VerticalContentAlignment = Alignment.Middle,
            Children = [this.currentKeybindView, this.addButton],
        };
        this.keybindEntryHighlighter = new Frame()
        {
            Layout = LayoutParameters.AutoRow(),
            Background = new(Game1.staminaRect),
            BackgroundTint = Color.Transparent,
            Content = currentKeybindLane,
        };
        this.capturingAnimator = new(this.keybindEntryHighlighter,
            frame => frame.BackgroundTint,
            AlphaLerp,
            (frame, color) => frame.BackgroundTint = color
        )
        {
            AutoReverse = true,
            Loop = true,
        };
        this.keybindsLane = new() { Layout = LayoutParameters.AutoRow(), Orientation = Orientation.Vertical };
        this.UpdateKeybinds();
        this.layoutLane = new Lane() { Layout = LayoutParameters.AutoRow(), Orientation = Orientation.Vertical };
        this.UpdateKeybindType();
        return new Frame()
        {
            Layout = new() { Width = Length.Px(640), Height = Length.Content() },
            Border = UiSprites.ControlBorder,
            Padding = UiSprites.ControlBorder.FixedEdges! + new Edges(8),
            Content = this.layoutLane,
        };
    }

    private void AddButton_LeftClick(object? sender, ClickEventArgs e)
    {
        if (!e.IsPrimaryButton())
        {
            return;
        }
        Game1.playSound("drumkit6");
        UI.InputHelper.Suppress(e.Button);
        this.StartCapturing();
    }

    private void AddKeybindRow(Keybind keybind)
    {
        var keybindView = new KeybindView
        {
            Layout = LayoutParameters.AutoRow(),
            Margin = this.keybindsLane.Children.Count > 0 ? new(Top: 16) : Edges.NONE,
            Keybind = keybind,
            SpriteMap = spriteMap,
        };
        var deleteButton = new Image()
        {
            Layout = new() { Width = Length.Content(), Height = Length.Px(40) },
            Sprite = UiSprites.SmallTrashCan,
            ShadowAlpha = 0.35f,
            ShadowOffset = new(-3, 4),
            Tooltip = this.DeleteButtonTooltip,
            Focusable = true,
            Tags = Tags.Create(keybind),
        };
        deleteButton.LeftClick += this.DeleteButton_LeftClick;
        var row = new Lane()
        {
            Layout = LayoutParameters.AutoRow(),
            VerticalContentAlignment = Alignment.Middle,
            Children = [keybindView, new Spacer() { Layout = LayoutParameters.AutoRow() }, deleteButton],
        };
        this.keybindsLane.Children.Add(row);
    }

    private static Color AlphaLerp(Color color1, Color color2, float amount)
    {
        amount = MathHelper.Clamp(amount, 0f, 1f);
        float alpha = MathHelper.Lerp(color1.A, color2.A, amount) / 255f;
        return new((int)(color2.R * alpha), (int)(color2.G * alpha), (int)(color2.B * alpha), (int)(alpha * 255));
    }

    private Button CreateAddButton()
    {
        var button = new Button() { Layout = LayoutParameters.FitContent() };
        if (!string.IsNullOrEmpty(this.AddButtonText))
        {
            button.Text = this.AddButtonText;
        }
        else
        {
            button.Content = new Image()
            {
                Layout = LayoutParameters.FixedSize(20, 20),
                Margin = new(0, 4),
                Sprite = UiSprites.SmallGreenPlus,
                Tint = new(0.2f, 0.5f, 1f),
            };
        }
        button.LeftClick += this.AddButton_LeftClick;
        return button;
    }

    private void DeleteButton_LeftClick(object? sender, ClickEventArgs e)
    {
        if (sender is not IView view)
        {
            return;
        }
        Game1.playSound("trashcan");
        var keybind = view.Tags.Get<Keybind>();
        this.KeybindList = new(this.KeybindList.Keybinds.Where(kb => kb != keybind).ToArray());
    }

    private static IEnumerable<SButton> GetPressedButtons()
    {
        foreach (var key in Game1.input.GetKeyboardState().GetPressedKeys())
        {
            yield return key.ToSButton();
        }
        if (Game1.options.gamepadControls)
        {
            var gamepadState = Game1.input.GetGamePadState();
            var heldButtons = Utility.getHeldButtons(gamepadState).GetEnumerator();
            while (heldButtons.MoveNext())
            {
                yield return heldButtons.Current.ToSButton();
            }
            if (gamepadState.Buttons.LeftStick == ButtonState.Pressed)
            {
                yield return SButton.LeftStick;
            }
            if (gamepadState.Buttons.RightStick == ButtonState.Pressed)
            {
                yield return SButton.RightStick;
            }
        }
    }

    private static bool IsBindable(SButton button)
    {
        return !bannedButtons.Contains(button);
    }

    private void StopCapturing()
    {
        Game1.playSound("drumkit6");
        this.CapturingInput = false;
        this.capturingAnimator.Stop();
        this.keybindEntryHighlighter.BackgroundTint = Color.Transparent;
        this.addButton.Visibility = Visibility.Visible;
        this.capturedButtons.Clear();
        this.currentKeybindView.Keybind = new();
        if (this.KeybindType != KeybindType.MultipleKeybinds)
        {
            Overlay.Remove(this);
        }
    }

    private void UpdateKeybinds()
    {
        if (this.keybindsLane is null)
        {
            return;
        }

        this.keybindsLane.Children.Clear();
        foreach (var keybind in this.KeybindList.Keybinds)
        {
            if (keybind.IsBound)
            {
                this.AddKeybindRow(keybind);
            }
        }
    }

    private void UpdateKeybindType()
    {
        if (this.layoutLane is null)
        {
            return;
        }

        this.layoutLane.Children = this.KeybindType == KeybindType.MultipleKeybinds
                ? [this.keybindEntryHighlighter, this.horizontalDivider, this.keybindsLane]
                : [this.keybindEntryHighlighter];
    }
}
