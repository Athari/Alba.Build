namespace Alba.Build.PowerShell;

public class NonInteractiveException : NotSupportedException
{
    public NonInteractiveException() : base(DefaultMessage) { }
    public NonInteractiveException(string? message) : base(message ?? DefaultMessage) { }
    public NonInteractiveException(string? message, Exception? innerException) : base(message ?? DefaultMessage, innerException) { }

    private const string DefaultMessage = "Interaction with user is not supported in MSBuild tasks.";
}