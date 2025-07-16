namespace Alba.Build.PowerShell;

public static class ErrorProvider
{
    private const string Ns = "Alba.Build.PowerShell";
    public const string Cat = $"{Ns}.{nameof(ErrorCat)}";
    public const string Code = $"{Ns}.{nameof(ErrorCode)}";
}

public static class ErrorCat
{
    public const string Abps = "ABPS";
    public const string Style = nameof(Style);
    public const string Usage = nameof(Usage);
}

public static class ErrorCode
{
    public const string Prefix = ErrorCat.Abps;

    public const string ArgumentError = $"{Prefix}101";
    public const string ParseError = $"{Prefix}102";
    public const string AstParseError = $"{Prefix}103";
    public const string InternalError = $"{Prefix}104";

    public const string RuntimeError = $"{Prefix}201";
    public const string ErrorRecord = $"{Prefix}202";

    public const string WriteHostError = $"{Prefix}301";
    public const string WriteHostWarning = $"{Prefix}302";
    public const string WriteHostDefault = $"{Prefix}303";
    public const string WriteHostVerbose = $"{Prefix}304";
    public const string WriteHostDebug = $"{Prefix}305";
}