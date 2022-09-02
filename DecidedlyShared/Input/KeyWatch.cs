using System;
using Microsoft.Xna.Framework.Input;

namespace DecidedlyShared.Input;

public record struct KeyWatch(Keys Key, KeyPressType Type, Action? Callback, Action<string>? LogCallback);
