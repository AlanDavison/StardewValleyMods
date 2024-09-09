using System;

namespace DHVarietyTools.Utilities;

public class DescriptiveBool
{
    private bool state = default;
    private string message;
    private string context;
    private Exception? exception;

    public DescriptiveBool(bool state, string context = "", string message = "", Exception? exception = null)
    {
        this.state = state;
        this.context = context;
        this.message = message;
        this.exception = exception;
    }

    public Exception? Exception => this.exception;
    public string Context => this.context;

    public bool ExceptionExists()
    {
        return this.exception is not null;
    }

    public static implicit operator bool(DescriptiveBool d) => d.state;
}
