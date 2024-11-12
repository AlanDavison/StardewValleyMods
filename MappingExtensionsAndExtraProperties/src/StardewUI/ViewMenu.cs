using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewUI.Events;
using StardewUI.Graphics;
using StardewUI.Input;
using StardewUI.Layout;
using StardewUI.Overlays;
using StardewValley;
using StardewValley.Menus;

namespace StardewUI;

/// <summary>
/// Generic menu implementation based on a root <see cref="IView"/>.
/// </summary>
public abstract class ViewMenu<T> : IClickableMenu, IDisposable
    where T : IView
{
    /// <summary>
    /// Event raised when the menu is closed.
    /// </summary>
    public event EventHandler<EventArgs>? Close;

    /// <summary>
    /// Amount of dimming between 0 and 1; i.e. opacity of the background underlay.
    /// </summary>
    /// <remarks>
    /// Underlay is only drawn when game options do not force clear backgrounds.
    /// </remarks>
    public float DimmingAmount { get; set; } = 0.75f;

    /// <summary>
    /// The view to display with this menu.
    /// </summary>
    public T View => this.view;

    private static readonly Edges DefaultGutter = new(100, 50);

    private readonly Edges? gutter;

    // For tracking activation paths, we not only want a weak table for the overlay itself (to prevent overlays from
    // being leaked) but also for the ViewChild path used to activate it, because these views may go out of scope while
    // the overlay is open.
    private readonly ConditionalWeakTable<IOverlay, WeakViewChild[]> overlayActivationPaths = [];
    private readonly OverlayContext overlayContext = new();
    private readonly ConditionalWeakTable<IOverlay, OverlayLayoutData> overlayCache = [];
    private readonly T view;
    private readonly bool wasHudDisplayed;

    private ViewChild[] hoverPath = [];
    private bool isRehoverScheduled;
    private int? rehoverRequestTick;

    // When clearing the activeClickableMenu, the game will call its Dispose method BEFORE actually changing the field
    // value to null or the new menu. If a Close handler then tries to open a different menu (which is really the
    // primary use case for the Close event) then this could trigger an infinite loop/stack overflow, i.e.
    // Dispose -> Close (Handler) -> set Game1.activeClickableMenu -> Dispose again
    // As a workaround, we can track when dispose has been requested and suppress duplicates.
    private bool isDisposed;

    // Whether the overlay was pushed within the last frame.
    private bool justPushedOverlay;
    private Point previousHoverPosition;
    private Point previousDragPosition;

    /// <summary>
    /// Initializes a new instance of <see cref="ViewMenu{T}"/>.
    /// </summary>
    /// <param name="gutter">Gutter edges, in which no content should be drawn. Used for overscan, or general
    /// aesthetics.</param>
    /// <param name="forceDefaultFocus">Whether to always focus (snap the cursor to) the default element, even if the
    /// menu was triggered by keyboard/mouse.</param>
    public ViewMenu(Edges? gutter = null, bool forceDefaultFocus = false)
    {
        using var _ = Diagnostics.Trace.Begin(this, "ctor");

        Game1.playSound("bigSelect");

        this.gutter = gutter;
        this.overlayContext.Pushed += this.OverlayContext_Pushed;
        this.view = this.CreateView();
        this.MeasureAndCenterView();

        if (forceDefaultFocus || Game1.options.gamepadControls)
        {
            var focusPosition = this.view.GetDefaultFocusPath().ToGlobalPositions().LastOrDefault()?.CenterPoint();
            if (focusPosition.HasValue)
            {
                Game1.setMousePosition(new Point(this.xPositionOnScreen, this.yPositionOnScreen) + focusPosition.Value, true);
            }
        }

        this.wasHudDisplayed = Game1.displayHUD;
        Game1.displayHUD = false;
    }

    /// <summary>
    /// Creates the view.
    /// </summary>
    /// <remarks>
    /// Subclasses will generally create an entire tree in this method and store references to any views that might
    /// require content updates.
    /// </remarks>
    /// <returns>The created view.</returns>
    protected abstract T CreateView();

    /// <summary>
    /// Initiates a focus search in the specified direction.
    /// </summary>
    /// <param name="directionValue">An integer value corresponding to the direction; one of 0 (up), 1 (right), 2 (down)
    /// or 3 (left).</param>
    public override void applyMovementKey(int directionValue)
    {
        using var trace = Diagnostics.Trace.Begin(this, nameof(applyMovementKey));
        using var _ = OverlayContext.PushContext(this.overlayContext);
        var direction = (Direction)directionValue;
        var mousePosition = Game1.getMousePosition(true);
        this.OnViewOrOverlay(
            (view, origin) =>
            {
                var found = view.FocusSearch(mousePosition.ToVector2() - origin, direction);
                if (found is not null)
                {
                    FinishFocusSearch(view, origin.ToPoint(), found);
                    this.RequestRehover();
                }
            }
        );
    }

    /// <summary>
    /// Returns whether or not the menu wants <b>exclusive</b> gamepad controls.
    /// </summary>
    /// <remarks>
    /// This implementation always returns <c>false</c>. Contrary to what the name in Stardew's code implies, this
    /// setting is not required for <see cref="receiveGamePadButton(Buttons)"/> to work; instead, when enabled, it
    /// suppresses the game's default mapping of button presses to clicks, and would therefore require reimplementing
    /// key-repeat and other basic behaviors. There is no reason to enable it here.
    /// </remarks>
    /// <returns>Always <c>false</c>.</returns>
    public override bool areGamePadControlsImplemented()
    {
        return false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.isDisposed = true;
        Game1.displayHUD = this.wasHudDisplayed;
        this.Close?.Invoke(this, EventArgs.Empty);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Draws the current menu content.
    /// </summary>
    /// <param name="b">The target batch.</param>
    public override void draw(SpriteBatch b)
    {
        using var trace = Diagnostics.Trace.Begin(this, nameof(draw));

        var viewportBounds = Game1.graphics.GraphicsDevice.Viewport.Bounds;
        if (!Game1.options.showClearBackgrounds)
        {
            b.Draw(Game1.fadeToBlackRect, viewportBounds, Color.Black * this.DimmingAmount);
        }

        using var _ = OverlayContext.PushContext(this.overlayContext);

        this.MeasureAndCenterView();

        var origin = new Point(this.xPositionOnScreen, this.yPositionOnScreen);
        var viewBatch = new PropagatedSpriteBatch(b, Transform.FromTranslation(origin.ToVector2()));
        this.view.Draw(viewBatch);

        foreach (var overlay in this.overlayContext.BackToFront())
        {
            b.Draw(Game1.fadeToBlackRect, viewportBounds, Color.Black * overlay.DimmingAmount);
            var overlayData = this.MeasureAndPositionOverlay(overlay);
            var overlayBatch = new PropagatedSpriteBatch(b, Transform.FromTranslation(overlayData.Position));
            overlay.View.Draw(overlayBatch);
        }

        if (this.justPushedOverlay && this.overlayContext.Front is IOverlay frontOverlay && Game1.options.gamepadControls)
        {
            var defaultFocusPosition = frontOverlay
                .View.GetDefaultFocusPath()
                .ToGlobalPositions()
                .LastOrDefault()
                ?.Center();
            if (defaultFocusPosition.HasValue)
            {
                var overlayData = this.GetOverlayLayoutData(frontOverlay);
                Game1.setMousePosition((overlayData.Position + defaultFocusPosition.Value).ToPoint(), true);
            }
        }

        this.justPushedOverlay = false;

        string? tooltip = this.FormatTooltip(this.hoverPath);
        if (!string.IsNullOrEmpty(tooltip))
        {
            drawToolTip(b, tooltip, null, null);
        }

        Game1.mouseCursorTransparency = 1.0f;
        if (!this.IsInputCaptured())
        {
            this.drawMouse(b);
        }
    }

    /// <summary>
    /// Invoked on every frame in which a mouse button is down, regardless of the state in the previous frame.
    /// </summary>
    /// <param name="x">The mouse's current X position on screen.</param>
    /// <param name="y">The mouse's current Y position on screen.</param>
    public override void leftClickHeld(int x, int y)
    {
        using var trace = Diagnostics.Trace.Begin(this, nameof(this.leftClickHeld));

        if (Game1.options.gamepadControls || this.IsInputCaptured())
        {
            // No dragging with gamepad.
            return;
        }
        var dragPosition = new Point(x, y);
        if (dragPosition == this.previousDragPosition)
        {
            return;
        }

        this.previousDragPosition = dragPosition;
        using var _ = OverlayContext.PushContext(this.overlayContext);
        this.OnViewOrOverlay((view, origin) => view.OnDrag(new(dragPosition.ToVector2() - origin)));
    }

    /// <summary>
    /// Invoked on every frame with the mouse's current coordinates.
    /// </summary>
    /// <remarks>
    /// Essentially the same as <see cref="update(GameTime)"/> but slightly more convenient for mouse hover/movement
    /// effects because of the arguments provided.
    /// </remarks>
    /// <param name="x">The mouse's current X position on screen.</param>
    /// <param name="y">The mouse's current Y position on screen.</param>
    public override void performHoverAction(int x, int y)
    {
        using var trace = Diagnostics.Trace.Begin(this, nameof(this.performHoverAction));
        bool rehover = this.isRehoverScheduled || this.rehoverRequestTick.HasValue;
        if (rehover || (this.previousHoverPosition.X != x || this.previousHoverPosition.Y != y))
        {
            using var _ = OverlayContext.PushContext(this.overlayContext);
            this.OnViewOrOverlay((view, origin) => this.PerformHoverAction(view, origin, x, y));
        }

        // We use two flags for this in order to repeat the re-hover after one frame, because (a) input events won't
        // always get handled in the ideal order to track any specific change, and (b) even if they do, when operating
        // menus from the Framework, there can often by a one-frame delay before everything gets perfectly in sync due
        // to coordination between the view model, in vs. out bindings, reactions to INPC or other change events, etc.
        //
        // A delay of exactly 1 frame isn't always going to be perfect either, but it handles the majority of cases such
        // as wheel scrolling and controller-triggered tab/page navigation.
        this.isRehoverScheduled = this.rehoverRequestTick.HasValue;
        if (this.rehoverRequestTick <= Game1.ticks)
        {
            this.rehoverRequestTick = null;
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Always a no-op for menus in StardewUI.
    /// </remarks>
    public override void populateClickableComponentList()
    {
        // The base class does a bunch of nasty reflection to populate the list, none of which is compatible with how
        // this menu works. To save time, we can simply do nothing here.
    }

    /// <summary>
    /// Invoked whenever a controller button is newly pressed.
    /// </summary>
    /// <param name="b">The button that was pressed.</param>
    public override void receiveGamePadButton(Buttons b)
    {
        using var trace = Diagnostics.Trace.Begin(this, nameof(this.receiveGamePadButton));

        // We don't actually dispatch the button to any capturing overlay, just prevent it from affecting the menu.
        //
        // This is because a capturing overlay doesn't necessarily just need to know about the button "press", it cares
        // about the entire press, hold and release cycle, and can handle these directly through InputState or SMAPI.
        if (this.IsInputCaptured())
        {
            return;
        }

        var button = b.ToSButton();
        if (UI.InputHelper.IsSuppressed(button))
        {
            return;
        }

        // When the game performs updateActiveMenu, it checks areGamePadControlsImplemented(), and if false, translates
        // those buttons into clicks.
        //
        // We still receive this event regardless of areGamePadControlsImplemented(), but letting updateActiveMenu
        // convert the A and X button presses into clicks makes receiveLeftClick and receiveRightClick fire repeatedly
        // as the button is held, which is generally the behavior that users will be accustomed to. If we override the
        // gamepad controls then we'd have to reimplement the repeating-click logic.

        using var _ = OverlayContext.PushContext(this.overlayContext);
        this.InitiateButtonPress(button);
        switch (button)
        {
            case SButton.LeftTrigger:
                this.OnTabbable(p => p.PreviousTab());
                break;
            case SButton.RightTrigger:
                this.OnTabbable(p => p.NextTab());
                break;
            case SButton.LeftShoulder:
                this.OnPageable(p => p.PreviousPage());
                break;
            case SButton.RightShoulder:
                this.OnPageable(p => p.NextPage());
                break;
        }
    }

    /// <summary>
    /// Invoked whenever a keyboard key is newly pressed.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    public override void receiveKeyPress(Keys key)
    {
        using var _ = Diagnostics.Trace.Begin(this, nameof(this.receiveKeyPress));
        var realButton = ButtonResolver.GetPressedButton(key.ToSButton());
        // See comments on receiveGamePadButton for why we don't dispatch the key itself.
        if (this.IsInputCaptured() || UI.InputHelper.IsSuppressed(realButton))
        {
            return;
        }
        var action = ButtonResolver.GetButtonAction(realButton);
        if (action == ButtonAction.Cancel && this.overlayContext.Pop() is not null)
        {
            return;
        }
        // receiveGamePadButton also initiates this, so ignore it if it appears to be from a controller source.
        if (!realButton.TryGetController(out var _) && !Game1.isAnyGamePadButtonBeingHeld())
        {
            this.InitiateButtonPress(realButton);
        }
        // The choices we have for actually "capturing" the captured input aren't awesome. Since it's a *keyboard*
        // input, we really don't want to let keyboard events through, like having the "e" key dismiss the menu while
        // trying to type in a field. On the other hand, blocking all key presses will also block gamepad buttons when
        // areGamepadControlsImplemented() is false, and it is false for the reasons documented in receiveGamePadButton.
        //
        // The best workaround seems to be to keep track of whether or not any gamepad button is being pressed, and use
        // that to allow "keyboard" input on the basis of it not being "real" keyboard input. This could run into issues
        // in some far-out scenarios like simultaneous keyboard + controller presses, but eliminates much more obvious
        // and frustrating issues like not being able to navigate or dismiss the menu with a controller after typing on
        // a regular keyboard.
        if (
            key == Keys.Escape
            || Game1.keyboardDispatcher.Subscriber is not ICaptureTarget
            || Game1.isAnyGamePadButtonBeingHeld()
        )
        {
            base.receiveKeyPress(key);
        }
    }

    /// <summary>
    /// Invoked whenever the left mouse button is newly pressed.
    /// </summary>
    /// <param name="x">The mouse's current X position on screen.</param>
    /// <param name="y">The mouse's current Y position on screen.</param>
    /// <param name="playSound">Currently not used.</param>
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        using var trace = Diagnostics.Trace.Begin(this, nameof(this.receiveLeftClick));
        if (this.IsInputCaptured())
        {
            return;
        }
        var button = ButtonResolver.GetPressedButton(SButton.MouseLeft);
        if (UI.InputHelper.IsSuppressed(button))
        {
            return;
        }
        using var _ = OverlayContext.PushContext(this.overlayContext);
        this.InitiateClick(button, new(x, y));
    }

    /// <summary>
    /// Invoked whenever the right mouse button is newly pressed.
    /// </summary>
    /// <param name="x">The mouse's current X position on screen.</param>
    /// <param name="y">The mouse's current Y position on screen.</param>
    /// <param name="playSound">Currently not used.</param>
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        using var trace = Diagnostics.Trace.Begin(this, nameof(this.receiveRightClick));
        if (this.IsInputCaptured())
        {
            return;
        }
        var button = ButtonResolver.GetPressedButton(SButton.MouseRight);
        if (UI.InputHelper.IsSuppressed(button))
        {
            return;
        }
        using var _ = OverlayContext.PushContext(this.overlayContext);
        this.InitiateClick(button, new(x, y));
    }

    /// <summary>
    /// Invoked whenever the mouse wheel is used. Only works with vertical scrolls.
    /// </summary>
    /// <param name="value">A value indicating the desired vertical scroll direction; negative values indicate "down"
    /// and positive values indicate "up".</param>
    public override void receiveScrollWheelAction(int value)
    {
        using var trace = Diagnostics.Trace.Begin(this, nameof(this.receiveScrollWheelAction));
        if (this.IsInputCaptured())
        {
            return;
        }
        using var _ = OverlayContext.PushContext(this.overlayContext);
        // IClickableMenu calls the value "direction" but it is actually a magnitude, and always in the Y direction
        // (negative is down).
        var direction = value > 0 ? Direction.North : Direction.South;
        this.InitiateWheel(direction);
    }

    /// <summary>
    /// Invoked whenever the left mouse button is just released, after being pressed/held on the last frame.
    /// </summary>
    /// <param name="x">The mouse's current X position on screen.</param>
    /// <param name="y">The mouse's current Y position on screen.</param>
    public override void releaseLeftClick(int x, int y)
    {
        using var trace = Diagnostics.Trace.Begin(this, nameof(this.releaseLeftClick));
        if (this.IsInputCaptured())
        {
            return;
        }
        using var _ = OverlayContext.PushContext(this.overlayContext);
        var mousePosition = new Point(x, y);
        this.previousDragPosition = mousePosition;
        this.OnViewOrOverlay((view, origin) => view.OnDrop(new(mousePosition.ToVector2() - origin)));
    }

    /// <summary>
    /// Activates or deactivates the menu.
    /// </summary>
    /// <param name="active">Whether the menu should be active (displayed). If this is <c>false</c>, then the menu will
    /// be closed if already open; if <c>true</c>, it will be opened if not already open.</param>
    public void SetActive(bool active)
    {
        using var _ = Diagnostics.Trace.Begin(this, nameof(this.SetActive));
        if (Game1.activeClickableMenu is TitleMenu)
        {
            if (active)
            {
                TitleMenu.subMenu = this;
            }
            else if (TitleMenu.subMenu == this)
            {
                Game1.playSound(this.closeSound);
                TitleMenu.subMenu = null;
            }
        }
        else
        {
            if (active)
            {
                Game1.activeClickableMenu = this;
            }
            else if (Game1.activeClickableMenu == this)
            {
                Game1.playSound(this.closeSound);
                Game1.activeClickableMenu = null;
            }
        }
    }

    /// <summary>
    /// Runs on every update tick.
    /// </summary>
    /// <param name="time">The current <see cref="GameTime"/> including the time elapsed since last update tick.</param>
    public override void update(GameTime time)
    {
        using var _ = Diagnostics.Trace.Begin(this, nameof(this.update));
        this.View.OnUpdate(time.ElapsedGameTime);
        foreach (var overlay in this.overlayContext.FrontToBack())
        {
            overlay.Update(time.ElapsedGameTime);
        }
    }

    /// <summary>
    /// Formats a tooltip given the sequence of views from root to the lowest-level hovered child.
    /// </summary>
    /// <remarks>
    /// The default implementation reads the string value of the <em>last</em> (lowest-level) view with a non-empty
    /// <see cref="IView.Tooltip"/>, and breaks lines longer than 640px, which is the default vanilla tooltip width.
    /// </remarks>
    /// <param name="path">Sequence of all elements, and their relative positions, that the mouse coordinates are
    /// currently within.</param>
    /// <returns>The tooltip string to display, or <c>null</c> to not show any tooltip.</returns>
    protected virtual string? FormatTooltip(IEnumerable<ViewChild> path)
    {
        string? tooltip = this.hoverPath.Select(x => x.View.Tooltip).LastOrDefault(tooltip => !string.IsNullOrEmpty(tooltip));
        return Game1.parseText(tooltip, Game1.smallFont, 640);
    }

    private static void FinishFocusSearch(IView rootView, Point origin, FocusSearchResult found)
    {
        LogFocusSearchResult(found.Target);
        ReleaseCaptureTarget();
        Game1.playSound("shiny4");
        var pathWithTarget = found.Path.Append(found.Target).ToList();
        var nextMousePosition = origin + pathWithTarget.ToGlobalPositions().Last().CenterPoint();
        if (rootView.ScrollIntoView(pathWithTarget, out var distance))
        {
            nextMousePosition -= distance.ToPoint();
        }
        Game1.setMousePosition(nextMousePosition, true);
    }

    private OverlayLayoutData GetOverlayLayoutData(IOverlay overlay)
    {
        var rootPosition = new Vector2(this.xPositionOnScreen, this.yPositionOnScreen);
        return this.overlayCache.GetValue(overlay, ov => OverlayLayoutData.FromOverlay(this.view, rootPosition, overlay));
    }

    private Vector2? GetRootViewPosition(IView view)
    {
        if (view.Equals(this.View))
        {
            return new(this.xPositionOnScreen, this.yPositionOnScreen);
        }
        foreach (var overlay in this.overlayContext.FrontToBack())
        {
            if (overlay.View == view && this.overlayCache.TryGetValue(overlay, out var layoutData))
            {
                return layoutData.Position;
            }
        }
        return null;
    }

    private void InitiateButtonPress(SButton button)
    {
        var mousePosition = Game1.getMousePosition(true);
        this.OnViewOrOverlay(
            (view, origin) =>
            {
                var localPosition = mousePosition.ToVector2() - origin;
                if (!view.ContainsPoint(localPosition))
                {
                    return;
                }
                var pathBeforeScroll = view.GetPathToPosition(localPosition).ToList();
                var args = new ButtonEventArgs(localPosition, button);
                view.OnButtonPress(args);
            }
        );
        this.RequestRehover();
    }

    private void InitiateClick(SButton button, Point screenPoint)
    {
        if (this.overlayContext.Front is IOverlay overlay)
        {
            var overlayData = this.GetOverlayLayoutData(overlay);
            var overlayLocalPosition = screenPoint.ToVector2() - overlayData.Position;
            if (!overlayData.ContainsPoint(overlayLocalPosition))
            {
                this.overlayContext.Pop();
            }
            else
            {
                var overlayArgs = new ClickEventArgs(overlayLocalPosition, button);
                overlay.View.OnClick(overlayArgs);
            }
            return;
        }
        var origin = new Point(this.xPositionOnScreen, this.yPositionOnScreen);
        var localPosition = (screenPoint - origin).ToVector2();
        if (Game1.keyboardDispatcher.Subscriber is ICaptureTarget captureTarget)
        {
            var clickPath = this.view.GetPathToPosition(localPosition);
            if (!clickPath.Select(child => child.View).Contains(captureTarget.CapturingView))
            {
                captureTarget.ReleaseCapture();
            }
        }
        var args = new ClickEventArgs(localPosition, button);
        this.view.OnClick(args);
        this.RequestRehover();
    }

    private void InitiateWheel(Direction direction)
    {
        var mousePosition = Game1.getMousePosition(true);
        this.OnViewOrOverlay(
            (view, origin) =>
            {
                var localPosition = mousePosition.ToVector2() - origin;
                var pathBeforeScroll = view.GetPathToPosition(localPosition).ToList();
                var args = new WheelEventArgs(localPosition, direction);
                view.OnWheel(args);
                if (!args.Handled)
                {
                    return;
                }
                Game1.playSound("shiny4");
                Refocus(view, origin, localPosition, pathBeforeScroll, direction);
                this.RequestRehover();
            }
        );
    }

    private bool IsInputCaptured()
    {
        return this.overlayContext.FrontToBack().Any(overlay => overlay.CapturingInput);
    }

    [Conditional("DEBUG_FOCUS_SEARCH")]
    private static void LogFocusSearchResult(ViewChild? result)
    {
        Logger.Log($"Found: {result?.View.Name} ({result?.View.GetType().Name}) at {result?.Position}", LogLevel.Info);
    }

    private void MeasureAndCenterView()
    {
        using var _ = Diagnostics.Trace.Begin(this, nameof(this.MeasureAndCenterView));
        var viewportSize = UiViewport.GetMaxSize();
        var currentGutter = this.gutter ?? DefaultGutter;
        var availableMenuSize = viewportSize.ToVector2() - currentGutter.Total;
        if (!this.view.Measure(availableMenuSize))
        {
            return;
        }

        this.RequestRehover();
        // Make gutters act as margins; otherwise centering could actually place content in the gutter.
        // For example, if there is an asymmetrical gutter with left = 100 and right = 200, and it takes up the full
        // viewport width, then it will actually occupy the horizontal region from 150 to (viewportWidth - 150), which
        // is the centered region with 300px total margin. In this case we need to push the content left by 50px, or
        // half the difference between the left and right edge.
        int gutterOffsetX = (currentGutter.Left - currentGutter.Right) / 2;
        int gutterOffsetY = (currentGutter.Top - currentGutter.Bottom) / 2;
        this.width = (int)MathF.Round(this.view.OuterSize.X);
        this.height = (int)MathF.Round(this.view.OuterSize.Y);
        this.xPositionOnScreen = viewportSize.X / 2 - this.width / 2 + gutterOffsetX;
        this.yPositionOnScreen = viewportSize.Y / 2 - this.height / 2 + gutterOffsetY;
        this.Refocus();
    }

    private OverlayLayoutData MeasureAndPositionOverlay(IOverlay overlay)
    {
        var viewportBounds = Game1.graphics.GraphicsDevice.Viewport.Bounds;
        bool isUpdateRequired = overlay.View.OuterSize == Vector2.Zero;
        if (overlay.Parent is null)
        {
            isUpdateRequired = overlay.View.Measure(viewportBounds.Size.ToVector2());
        }
        var overlayData = this.GetOverlayLayoutData(overlay);
        if (overlay.Parent is not null)
        {
            var availableOverlaySize = viewportBounds.Size.ToVector2() - overlayData.Position;
            isUpdateRequired = overlay.View.Measure(availableOverlaySize);
        }
        if (isUpdateRequired)
        {
            overlayData.Update(overlay);
            this.RequestRehover();
        }
        return overlayData;
    }

    private void OnOverlayRemoved(IOverlay overlay)
    {
        // A convenience for gamepad users is to try to move the mouse cursor back to whatever triggered the
        // overlay in the first place, e.g. the button on a drop-down list.
        // However, it's unnecessarily distracting to do it for mouse controls.
        if (Game1.options.gamepadControls)
        {
            this.RestoreFocusToOverlayActivation(overlay);
        }
    }

    private void OnPageable(Action<IPageable> action)
    {
        if (this.view is IPageable pageable)
        {
            action(pageable);
        }
    }

    private void OnTabbable(Action<ITabbable> action)
    {
        if (this.view is ITabbable tabbable)
        {
            action(tabbable);
        }
    }

    private void OnViewOrOverlay(Action<IView, Vector2> action)
    {
        if (this.overlayContext.Front is IOverlay overlay)
        {
            var overlayData = this.GetOverlayLayoutData(overlay);
            action(overlay.View, overlayData.Position);
        }
        else
        {
            var origin = new Vector2(this.xPositionOnScreen, this.yPositionOnScreen);
            action(this.view, origin);
        }
    }

    private void Overlay_Close(object? sender, EventArgs e)
    {
        if (sender is IOverlay overlay)
        {
            this.OnOverlayRemoved(overlay);
        }
    }

    private void OverlayContext_Pushed(object? sender, EventArgs e)
    {
        var overlay = this.overlayContext.Front!;
        this.overlayActivationPaths.AddOrUpdate(overlay, this.hoverPath.Select(child => child.AsWeak()).ToArray());
        overlay.Close += this.Overlay_Close;
        this.justPushedOverlay = true;
    }

    private void PerformHoverAction(IView rootView, Vector2 viewPosition, int mouseX, int mouseY)
    {
        var mousePosition = new Vector2(mouseX, mouseY);
        var localPosition = mousePosition - viewPosition;
        var previousLocalPosition = this.previousHoverPosition.ToVector2() - viewPosition;
        this.previousHoverPosition = new(mouseX, mouseY);
        this.hoverPath = rootView.GetPathToPosition(localPosition).ToArray();
        rootView.OnPointerMove(new PointerMoveEventArgs(previousLocalPosition, localPosition));
    }

    private void Refocus(Direction searchDirection = Direction.South)
    {
        if (this.hoverPath.Length == 0 || !Game1.options.gamepadControls || !Game1.options.gamepadControls)
        {
            return;
        }
        var previousLeaf = this.hoverPath.ToGlobalPositions().Last();
        this.OnViewOrOverlay(
            (view, origin) =>
            {
                var newLeaf = view.ResolveChildPath(this.hoverPath.Select(x => x.View)).ToGlobalPositions().LastOrDefault();
                if (
                    newLeaf?.View == previousLeaf.View
                    && (
                        newLeaf.Position != previousLeaf.Position
                        || newLeaf.View.OuterSize != previousLeaf.View.OuterSize
                    )
                )
                {
                    Refocus(view, origin, this.previousHoverPosition.ToVector2(), this.hoverPath, searchDirection);
                }
            }
        );
    }

    private static void Refocus(
        IView root,
        Vector2 origin,
        Vector2 previousPosition,
        IReadOnlyList<ViewChild> previousPath,
        Direction searchDirection
    )
    {
        using var _ = Diagnostics.Trace.Begin(nameof(ViewMenu<T>), nameof(Refocus));
        if (!Game1.options.gamepadControls)
        {
            return;
        }
        var pathAfterScroll = root.ResolveChildPath(previousPath.Select(child => child.View));
        var (targetView, bounds) = pathAfterScroll.Aggregate(
            (root as IView, bounds: Bounds.Empty),
            (acc, child) => (child.View, new(acc.bounds.Position + child.Position, child.View.OuterSize))
        );
        var focusedViewChild = root.GetPathToPosition(bounds.Center()).ToGlobalPositions().LastOrDefault();
        if (focusedViewChild?.View == targetView)
        {
            Game1.setMousePosition((origin + focusedViewChild.Center()).ToPoint(), true);
        }
        else
        {
            // Can happen if the target view is no longer reachable, i.e. outside the scroll bounds.
            //
            // When we try to find a new focus target, we have to accommodate for the fact that previousPosition is the
            // actual cursor position on screen before the scroll, and not the "adjusted" position of the cursor
            // relative to the new view bounds; for example, if we just scrolled up by 64 px and the view now has a
            // negative Y position (which is why we didn't find it with GetPathToPosition), then the value of
            // previousPosition will be 64px lower (still in bounds) and we have to adjust to compensate.
            var focusOffset =
                previousPath.Count > 0
                    ? bounds.Position - previousPath.ToGlobalPositions().Last().Position
                    : Vector2.Zero;
            var validResult = root.FocusSearch(previousPosition + focusOffset, searchDirection);
            if (validResult is not null)
            {
                ReleaseCaptureTarget();
                var pathWithTarget = validResult.Path.Append(validResult.Target);
                var nextMousePosition = origin + pathWithTarget.ToGlobalPositions().Last().Center();
                Game1.setMousePosition(nextMousePosition.ToPoint(), true);
            }
        }
    }

    private static void ReleaseCaptureTarget()
    {
        if (Game1.keyboardDispatcher.Subscriber is ICaptureTarget captureTarget)
        {
            captureTarget.ReleaseCapture();
        }
    }

    private void RequestRehover()
    {
        this.rehoverRequestTick = Game1.ticks;
    }

    private void RestoreFocusToOverlayActivation(IOverlay overlay)
    {
        var overlayData = this.GetOverlayLayoutData(overlay);
        if (this.overlayActivationPaths.TryGetValue(overlay, out var activationPath) && activationPath.Length > 0)
        {
            var strongActivationPath = activationPath
                .Select(x => x.TryResolve(out var viewChild) ? viewChild : null)
                .ToList();
            if (strongActivationPath.Count > 0 && strongActivationPath.All(child => child is not null))
            {
                var rootPosition = this.GetRootViewPosition(strongActivationPath[0]!.View);
                if (rootPosition is not null)
                {
                    var position = strongActivationPath!.ToGlobalPositions().Last().Center();
                    Game1.setMousePosition((rootPosition.Value + position).ToPoint(), true);
                    return;
                }
            }
        }
        var defaultFocusPosition = overlay.Parent?.GetDefaultFocusPath().ToGlobalPositions().LastOrDefault()?.Center();
        if (defaultFocusPosition.HasValue)
        {
            Game1.setMousePosition((overlayData.Position + defaultFocusPosition.Value).ToPoint(), true);
        }
    }

    class OverlayLayoutData(ViewChild root)
    {
        public Bounds ParentBounds { get; set; } = Bounds.Empty;
        public ViewChild[] ParentPath { get; set; } = [];
        public Vector2 Position { get; set; }

        // Interactable bounds are the individual bounding boxes of all top-level and floating views in the overlay.
        //
        // Union bounds are a single bounding box used to speed up checks that are completely outside any part of the
        // overlay, but a point being inside the union does not guarantee that it actually lands on a real view; that
        // is, there may be gaps between views.
        //
        // As a hybrid of speed and accuracy, we check the union bounds to exclude impossible points, then check the
        // interactable bounds to confirm a positive match.
        private Bounds[] interactableBounds = [];
        private Bounds unionBounds = Bounds.Empty;

        public static OverlayLayoutData FromOverlay(IView rootView, Vector2 rootPosition, IOverlay overlay)
        {
            using var _ = Diagnostics.Trace.Begin(nameof(OverlayLayoutData), nameof(FromOverlay));
            var data = new OverlayLayoutData(new(rootView, rootPosition));
            data.Update(overlay);
            return data;
        }

        public bool ContainsPoint(Vector2 point)
        {
            return this.unionBounds.ContainsPoint(point) && this.interactableBounds.Any(bounds => bounds.ContainsPoint(point));
        }

        public void Update(IOverlay overlay)
        {
            using var _ = Diagnostics.Trace.Begin(this, nameof(this.Update));
            var immediateParent = this.GetImmediateParent();
            if (overlay.Parent != immediateParent?.View)
            {
                this.ParentPath =
                    (
                        overlay.Parent is not null
                            ? root.View.GetPathToView(overlay.Parent)?.ToGlobalPositions().ToArray()
                            : null
                    ) ?? [];
            }
            immediateParent = this.GetImmediateParent();
            this.ParentBounds = immediateParent is not null
                ? immediateParent.GetContentBounds().Offset(root.Position)
                : GetUiViewportBounds();
            float x = ResolveAlignments(
                overlay.HorizontalParentAlignment, this.ParentBounds.Left, this.ParentBounds.Right,
                overlay.HorizontalAlignment,
                overlay.View.OuterSize.X
            );
            float y = ResolveAlignments(
                overlay.VerticalParentAlignment, this.ParentBounds.Top, this.ParentBounds.Bottom,
                overlay.VerticalAlignment,
                overlay.View.OuterSize.Y
            );
            this.Position = new Vector2(x, y);

            this.interactableBounds = overlay.View.FloatingBounds.Prepend(overlay.View.ActualBounds).ToArray();
            this.unionBounds = this.interactableBounds.Aggregate(Bounds.Empty, (acc, bounds) => acc.Union(bounds));
        }

        private ViewChild? GetImmediateParent() => this.ParentPath.Length > 0 ? this.ParentPath[^1] : null;

        private static Bounds GetUiViewportBounds()
        {
            var deviceViewport = Game1.graphics.GraphicsDevice.Viewport;
            var uiViewport = Game1.uiViewport;
            int viewportWidth = Math.Min(deviceViewport.Width, uiViewport.Width);
            int viewportHeight = Math.Min(deviceViewport.Height, uiViewport.Height);
            return new(new(0, 0), new(viewportWidth, viewportHeight));
        }

        private static float ResolveAlignments(
            Alignment parentAlignment,
            float parentStart,
            float parentEnd,
            Alignment childAlignment,
            float childLength
        )
        {
            float anchor = parentAlignment switch
            {
                Alignment.Start => parentStart,
                Alignment.Middle => (parentEnd - parentStart) / 2,
                Alignment.End => parentEnd,
                _ => throw new ArgumentException(
                    $"Invalid parent alignment: {parentAlignment}",
                    nameof(parentAlignment)
                ),
            };
            return childAlignment switch
            {
                Alignment.Start => anchor,
                Alignment.Middle => anchor - (childLength / 2),
                Alignment.End => anchor - childLength,
                _ => throw new ArgumentException($"Invalid child alignment: {childAlignment}", nameof(childAlignment)),
            };
        }
    }
}
