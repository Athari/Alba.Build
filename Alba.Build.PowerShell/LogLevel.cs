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
    Info,
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

    public static MessageCategory ToMessageCategory(this LogLevel @this) =>
        @this switch {
            >= MessageLow and <= MessageHigh => MessageCategory.Message,
            Info => MessageCategory.Info,
            Warning => MessageCategory.Warning,
            Error => MessageCategory.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(@this), @this, null),
        };
}