using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using JetBrains.Annotations;

namespace Alba.Build.PowerShell;

#pragma warning disable CS8763 // A method marked [DoesNotReturn] should not return.

internal static class ExceptionExts
{
    [DoesNotReturn, ContractAnnotation("=> halt")]
    public static void Rethrow(this Exception @this) =>
        ExceptionDispatchInfo.Capture(@this).Throw();
}