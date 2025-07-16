using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;

namespace Alba.Build.PowerShell;

internal class ScriptCommandInfo
{
    private readonly PowerShellTaskContext _ctx;

    public IList<ParameterMetadata> Parameters { get; } = [ ];

    public CmdletBindingAttribute? Cmdlet { get; }

    private ScriptCommandInfo(PowerShellTaskContext ctx) => _ctx = ctx;

    public ScriptCommandInfo(PowerShellTaskContext ctx, ScriptBlockAst ast) : this(ctx)
    {
        var cmdletAttrs = AttrInfoCollection.FromAttrAsts(this, ast.Attributes.Concat(ast.ParamBlock.Attributes));
        if (cmdletAttrs.GetAttr<CmdletCommonMetadataAttribute>() is { } cmdletAttrInfo)
            Cmdlet = cmdletAttrInfo.SetPropsFromNamedArgs(new CmdletBindingAttribute());

        var paramAsts = ast.ParamBlock.Parameters;
        foreach (var paramAst in paramAsts) {
            var paramAttrs = AttrInfoCollection.FromAttrAsts(this, paramAst.Attributes);
            var param = new ParameterMetadata(paramAst.GetName(), paramAst.StaticType);

            if (paramAttrs.GetAttr<AliasAttribute>() is { } aliasAttrInfo) {
                var aliases = aliasAttrInfo.Ast.FindAll<StringConstantExpressionAst>().Select(a => a.Value).ToArray();
                var aliasAttr = new AliasAttribute(aliases);
                param.Attributes.Add(aliasAttr);
                param.Aliases.AddRange(aliasAttr.AliasNames);
            }
            if (paramAttrs.GetAttr<ParameterAttribute>() is { } paramAttrInfo)
                param.Attributes.Add(paramAttrInfo.SetPropsFromNamedArgs(new ParameterAttribute()));
            Parameters.Add(param);
        }
    }

    public ScriptCommandInfo(PowerShellTaskContext ctx, Command command) : this(ctx)
    {
        Attempt(() => {
            using var ps = PSShell.Create(RunspaceMode.NewRunspace);
            Parameters.AddRange(ps.GetCommandInfo(command.CommandText).Parameters.Values);
        });
    }

    private void Attempt(Action action)
    {
        try {
            action();
        }
        catch (Exception e) {
            _ctx.Host.UIX.LogException(LogLevel.Error, e, withStackTrace: null,
                "Failed to parse script AST.", ErrorCat.Build, ErrorCode.AstParseError);
        }
    }

    private class AttrInfoCollection(IList<AttrInfo> list) : Collection<AttrInfo>(list)
    {
        public static AttrInfoCollection FromAttrAsts(ScriptCommandInfo info, IEnumerable<AttributeBaseAst> attrAsts) =>
            new([ ..attrAsts.OfType<AttributeAst>().Select(a => new AttrInfo(info, a)) ]);

        public AttrInfo? GetAttr<T>() =>
            this.FirstOrDefault(a => a.Type.Is<T>());
    }

    private class AttrInfo(ScriptCommandInfo info, AttributeAst ast)
    {
        public AttributeAst Ast { get; } = ast;

        [field: MaybeNull]
        public Type Type => field ??= Ast.TypeName.GetReflectionAttributeType();

        public T SetPropsFromNamedArgs<T>(T attr)
            where T : Attribute
        {
            var props = attr.GetType().GetProperties().ToDictionarySafeOIC(p => p.Name);
            foreach (var namedArgAst in Ast.NamedArguments) {
                if (props.GetValueOrDefault(namedArgAst.ArgumentName) is { } prop &&
                    (namedArgAst.ExpressionOmitted ? true : namedArgAst.SafeGetValue()) is { } value)
                    info.Attempt(() => prop.SetValue(attr, value));
            }
            return attr;
        }
    }
}