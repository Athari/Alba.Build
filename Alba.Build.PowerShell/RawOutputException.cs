namespace Alba.Build.PowerShell;

public class RawOutputException : NotSupportedException
{
    public RawOutputException() : base(DefaultMessage) { }
    public RawOutputException(string? message) : base(message ?? DefaultMessage) { }
    public RawOutputException(string? message, Exception? innerException) : base(message ?? DefaultMessage, innerException) { }

    private const string DefaultMessage = "Raw user interface is not supported in MSBuild tasks.";
}