using System;
using Microsoft.Xna.Framework.Input;

namespace DecidedlyShared.Input;

public record struct ButtonWatch(Buttons Button, KeyPressType Type, Action? Callback, Action<string>? LogCallback);
