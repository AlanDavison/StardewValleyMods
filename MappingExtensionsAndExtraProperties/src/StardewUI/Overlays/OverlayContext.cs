using System;
using System.Collections.Generic;

namespace StardewUI.Overlays;

/// <summary>
/// The context of an overlay, e.g. the active overlays for a particular menu or other non-overlay UI.
/// </summary>
public class OverlayContext
{
    /// <summary>
    /// Event raised when an overlay is pushed to the front.
    /// </summary>
    /// <remarks>
    /// This can either be a new overlay, or an overlay that was farther back and brought forward. After this event, the
    /// affected overlay will always be the <see cref="Front"/>.
    /// </remarks>
    public event EventHandler<EventArgs>? Pushed;

    /// <summary>
    /// The ambient context for the UI root that is currently being displayed or handling events.
    /// </summary>
    public static OverlayContext? Current
    {
        get => current;
        set => current = value;
    }

    /// <summary>
    /// Gets the overlay at the front of the stack.
    /// </summary>
    public IOverlay? Front
    {
        get => this.stack.Count > 0 ? this.stack[^1] : null;
    }

    private static OverlayContext? current;

    private readonly List<IOverlay> stack = [];

    /// <summary>
    /// Iterates the stack from the back/bottom/least-recent overlay to the front/top/most-recent.
    /// </summary>
    public IEnumerable<IOverlay> BackToFront()
    {
        return this.stack;
    }

    /// <summary>
    /// Iterates the stack from the front/top/most-recent overlay to the back/bottom/least-recent.
    /// </summary>
    public IEnumerable<IOverlay> FrontToBack()
    {
        for (int i = this.stack.Count - 1; i >= 0; i--)
        {
            yield return this.stack[i];
        }
    }

    /// <summary>
    /// Switches to a new context for a given scope.
    /// </summary>
    /// <param name="context">The new context.</param>
    /// <returns>An <see cref="IDisposable"/> which, when disposed, reverts the <see cref="Current"/> context to its
    /// value before <c>PushContext</c> was called.</returns>
    internal static IDisposable PushContext(OverlayContext context)
    {
        var result = new OverlayContextReverter(context);
        Current = context;
        return result;
    }

    /// <summary>
    /// Removes the front-most overlay.
    /// </summary>
    /// <returns>The overlay previously at the front, or <c>null</c> if no overlays were active.</returns>
    public IOverlay? Pop()
    {
        if (this.stack.Count == 0)
        {
            return null;
        }
        var overlay = this.stack[^1];
        this.stack.RemoveAt(this.stack.Count - 1);
        overlay.OnClose();
        return overlay;
    }

    /// <summary>
    /// Pushes an overlay to the front.
    /// </summary>
    /// <remarks>
    /// If the specified <paramref name="overlay"/> is already in the stack, then it will be moved from its previous
    /// position to the front.
    /// </remarks>
    /// <param name="overlay">The overlay to display on top of the current UI and any other overlays.</param>
    public void Push(IOverlay overlay)
    {
        this.stack.Remove(overlay);
        this.stack.Add(overlay);
        this.Pushed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Removes a specific overlay from the stack, regardless of its position.
    /// </summary>
    /// <remarks>
    /// This is most often invoked by an overlay needing to dismiss itself, e.g. an overlay with an "OK" or "Close"
    /// button.
    /// </remarks>
    /// <param name="overlay">The overlay to remove.</param>
    /// <returns><c>true</c> if the <paramref name="overlay"/> was removed; <c>false</c> if it was not active.</returns>
    public bool Remove(IOverlay overlay)
    {
        bool removed = this.stack.Remove(overlay);
        if (removed)
        {
            overlay.OnClose();
        }
        return removed;
    }

    class OverlayContextReverter(OverlayContext? previousContext) : IDisposable
    {
        public void Dispose()
        {
            Current = previousContext;
            GC.SuppressFinalize(this);
        }
    }
}
