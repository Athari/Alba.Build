namespace Alba.Build.PowerShell;

public static class ErrorCode
{
    public const string Prefix = "ABPS";

    public const string ArgumentError = $"{Prefix}101";
    public const string ParseError = $"{Prefix}102";
    public const string AstParseError = $"{Prefix}103";
    public const string InternalError = $"{Prefix}104";

    public const string RuntimeError = $"{Prefix}201";
    public const string ErrorRecord = $"{Prefix}202";
    public const string NonInteractiveError = $"{Prefix}203";
    public const string InteractiveError = $"{Prefix}204";
    public const string RawOutputError = $"{Prefix}205";

    public const string WriteHostError = $"{Prefix}301";
    public const string WriteHostWarning = $"{Prefix}302";
    public const string WriteHostInfo = $"{Prefix}303";
    public const string WriteHostVerbose = $"{Prefix}304";
    public const string WriteHostDebug = $"{Prefix}305";
    public const string WriteHostMessage = $"{Prefix}306";
    public const string WriteBuildMessage = $"{Prefix}307";
    public const string WriteBuildError = $"{Prefix}308";
}