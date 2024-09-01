using System;

namespace DHVarietyTools.Utilities;

public class DescriptiveBool(bool state, string context = "", string message = "", Exception? exception = null)
{
    private bool state = default;
    private string message;
    private string context;
    private Exception? exception;

    public Exception? Exception => this.exception;
    public string Context => this.context;

    public bool ExceptionExists()
    {
        return this.exception is not null;
    }

    public static implicit operator bool(DescriptiveBool d) => d.state;
}
