namespace Alba.Build.PowerShell;

public static class ErrorProvider
{
    private const string Ns = "Alba.Build.PowerShell";
    public const string Cat = $"{Ns}.{nameof(ErrorCat)}";
    public const string Code = $"{Ns}.{nameof(ErrorCode)}";
}