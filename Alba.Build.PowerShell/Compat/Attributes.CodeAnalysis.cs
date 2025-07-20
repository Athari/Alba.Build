#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP3_0_OR_GREATER && !NET5_0_OR_GREATER

namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property)]
internal sealed class AllowNullAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property)]
internal sealed class DisallowNullAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue)]
internal sealed class MaybeNullAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue)]
internal sealed class NotNullAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class MaybeNullWhenAttribute(bool returnValue) : Attribute
{
    public bool ReturnValue { get; } = returnValue;
}

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class NotNullWhenAttribute(bool returnValue) : Attribute
{
    public bool ReturnValue { get; } = returnValue;
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true)]
internal sealed class NotNullIfNotNullAttribute(string parameterName) : Attribute
{
    public string ParameterName { get; } = parameterName;
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class DoesNotReturnAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class DoesNotReturnIfAttribute(bool parameterValue) : Attribute
{
    public bool ParameterValue { get; } = parameterValue;
}

#endif

#if !NET5_0_OR_GREATER

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
internal sealed class MemberNotNullAttribute : Attribute
{
    public MemberNotNullAttribute(string member) => Members = [ member ];

    public MemberNotNullAttribute(params string[] members) => Members = members;

    public string[] Members { get; }
}

#endif

#if !NET7_0_OR_GREATER

[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class ConstantExpectedAttribute : Attribute
{
    public object? Min { get; set; }
    public object? Max { get; set; }
}

[AttributeUsage(AttributeTargets.Constructor)]
internal sealed class SetsRequiredMembersAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
internal sealed class StringSyntaxAttribute(string syntax, params object?[] arguments) : Attribute
{
    public StringSyntaxAttribute(string syntax) : this(syntax, new object?[0]) { }

    public string Syntax { get; } = syntax;

    public object?[] Arguments { get; } = arguments;

    public const string CompositeFormat = nameof(CompositeFormat);
    public const string DateOnlyFormat = nameof(DateOnlyFormat);
    public const string DateTimeFormat = nameof(DateTimeFormat);
    public const string EnumFormat = nameof(EnumFormat);
    public const string GuidFormat = nameof(GuidFormat);
    public const string Json = nameof(Json);
    public const string NumericFormat = nameof(NumericFormat);
    public const string Regex = nameof(Regex);
    public const string TimeOnlyFormat = nameof(TimeOnlyFormat);
    public const string TimeSpanFormat = nameof(TimeSpanFormat);
    public const string Uri = nameof(Uri);
    public const string Xml = nameof(Xml);
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, Inherited = false)]
internal sealed class UnscopedRefAttribute : Attribute { }

#endif

#if !NET8_0_OR_GREATER

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
internal sealed class ExperimentalAttribute(string diagnosticId) : Attribute
{
    public string DiagnosticId { get; } = diagnosticId;

    public string? UrlFormat { get; set; }
}

#endif