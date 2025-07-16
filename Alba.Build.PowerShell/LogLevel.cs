using System.Diagnostics.CodeAnalysis;
using Microsoft.Build.Framework;

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

public static class LogLevelExts
{
    [SuppressMessage("ReSharper", "SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault", Justification = "Intentional")]
    public static MessageImportance ToImportance(this LogLevel @this) =>
        @this switch {
            LogLevel.MessageLow => MessageImportance.Low,
            LogLevel.MessageNormal => MessageImportance.Normal,
            LogLevel.MessageHigh => MessageImportance.High,
            _ => throw new ArgumentOutOfRangeException(nameof(@this), @this, null),
        };
}