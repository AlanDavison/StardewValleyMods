using System;

namespace DecidedlyShared.Models;

public class DescriptiveBool(bool state, string context = "", string message = "", Exception? exception = null)
{
    private bool state = default;
    private string message;
    private string context;
    private Exception? exception;

    public bool ExceptionExists()
    {
        return this.exception is not null;
    }

    public Exception? GetException()
    {
        return this.exception;
    }

    public static implicit operator bool(DescriptiveBool d) => d.state;
}
