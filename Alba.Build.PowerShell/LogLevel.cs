using System.Diagnostics.CodeAnalysis;
using Microsoft.Build.Framework;
using static Alba.Build.PowerShell.LogLevel;

namespace Alba.Build.PowerShell;

public enum LogLevel
{
    MessageLow,
    MessageNormal,
    MessageHigh,
    Warning,
    Error,
    Critical,
}

[SuppressMessage("ReSharper", "SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault", Justification = "Intentional")]
public static class LogLevelExts
{
    public static MessageImportance ToImportance(this LogLevel @this) =>
        @this switch {
            MessageLow => MessageImportance.Low,
            MessageNormal => MessageImportance.Normal,
            MessageHigh => MessageImportance.High,
            _ => throw new ArgumentOutOfRangeException(nameof(@this), @this, null),
        };

    public static string ToBuildFormat(this LogLevel @this) =>
        @this switch {
            MessageLow or MessageNormal or MessageHigh => "message",
            Warning => "warning",
            Error or Critical => "error",
            _ => throw new ArgumentOutOfRangeException(nameof(@this), @this, null),
        };
}