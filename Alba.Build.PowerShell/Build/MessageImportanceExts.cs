using Microsoft.Build.Framework;

namespace Alba.Build.PowerShell;

public static class MessageImportanceExts
{
    public static LogLevel ToLogLevel(this MessageImportance @this) =>
        @this switch {
            MessageImportance.High => LogLevel.MessageHigh,
            MessageImportance.Normal => LogLevel.MessageNormal,
            MessageImportance.Low => LogLevel.MessageLow,
            _ => throw new ArgumentOutOfRangeException(nameof(@this), @this, null),
        };
}