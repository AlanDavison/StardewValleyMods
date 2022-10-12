using System;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;

namespace DecidedlyShared.Input;

public record struct ButtonWatch(Buttons Button, KeyPressType Type, Action? Callback, Action<string, LogLevel>? LogCallback);
