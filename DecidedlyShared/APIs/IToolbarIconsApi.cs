using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
// ReSharper disable InvalidXmlDocComment

namespace DecidedlyShared.APIs
{
    /// <summary>
    ///     Public api to add icons above or below the toolbar.
    /// </summary>
    public interface IToolbarIconsApi
    {
        /// <summary>(Deprecated) Event triggered when a toolbar icon is pressed.</summary>
        public event EventHandler<string> ToolbarIconPressed;

        /// <summary>Adds an icon next to the <see cref="Toolbar" />.</summary>
        /// <param name="id">A unique identifier for the icon.</param>
        /// <param name="texturePath">The path to the texture icon.</param>
        /// <param name="sourceRect">The source rectangle of the icon.</param>
        /// <param name="hoverText">Text to appear when hovering over the icon.</param>
        public void AddToolbarIcon(string id, string texturePath, Rectangle? sourceRect, string? hoverText);

        /// <summary>Removes an icon.</summary>
        /// <param name="id">A unique identifier for the icon.</param>
        public void RemoveToolbarIcon(string id);

        /// <summary>Subscribes to an event handler.</summary>
        /// <param name="handler">The event handler to subscribe.</param>
        void Subscribe(Action<IIconPressedEventArgs> handler);

        /// <summary>Unsubscribes an event handler from an event.</summary>
        /// <param name="handler">The event handler to unsubscribe.</param>
        void Unsubscribe(Action<IIconPressedEventArgs> handler);
    }

#pragma warning disable CA1711

    /// <summary>Represents the event arguments for a toolbar icon being pressed.</summary>
    public interface IIconPressedEventArgs
    {
        /// <summary>Gets the id of the icon that was pressed.</summary>
        string Id { get; }

        /// <summary>Gets the button that was pressed.</summary>
        SButton Button { get; }
    }
}
