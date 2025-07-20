using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices;

#if !NETCOREAPP3_0_OR_GREATER && !NET5_0_OR_GREATER

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class CallerArgumentExpressionAttribute : Attribute
{
    public CallerArgumentExpressionAttribute(string parameterName)
    {
        ParameterName = parameterName;
    }

    public string ParameterName { get; }
}

#endif

#if !NET5_0_OR_GREATER

internal static class IsExternalInit;

#endif

#if !NET7_0_OR_GREATER

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
internal sealed class CompilerFeatureRequiredAttribute(string featureName)
    : Attribute
{
    public const string RefStructs = nameof(RefStructs);
    public const string RequiredMembers = nameof(RequiredMembers);
    public string FeatureName { get; } = featureName;
    public bool IsOptional { get; init; }
}
#endif

#if !NET8_0_OR_GREATER

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[DebuggerNonUserCode]
[ExcludeFromCodeCoverage]
public sealed class UnsafeAccessorAttribute : Attribute
{
    public UnsafeAccessorAttribute(UnsafeAccessorKind kind) => Kind = kind;

    public UnsafeAccessorKind Kind { get; }

    public string? Name { get; set; }
}

public enum UnsafeAccessorKind
{
    Constructor,
    Method,
    StaticMethod,
    Field,
    StaticField
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
internal sealed class RequiredMemberAttribute : Attribute
{
    public RequiredMemberAttribute() { }
}

#endif