using Microsoft.Build.Framework;

namespace Alba.Build.PowerShell;

public enum MessageCategory
{
    Message,
    Info,
    Warning,
    Error,
}

internal static class MessageCategoryExts
{
    public static LogLevel ToLogLevel(this MessageCategory @this, MessageImportance importance) =>
        (@this, importance) switch {
            (MessageCategory.Message, MessageImportance.Low) => LogLevel.MessageLow,
            (MessageCategory.Message, MessageImportance.Normal) => LogLevel.MessageNormal,
            (MessageCategory.Message, MessageImportance.High) => LogLevel.MessageHigh,
            (MessageCategory.Info, _) => LogLevel.Info,
            (MessageCategory.Warning, _) => LogLevel.Warning,
            (MessageCategory.Error, _) => LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(@this)),
        };
}