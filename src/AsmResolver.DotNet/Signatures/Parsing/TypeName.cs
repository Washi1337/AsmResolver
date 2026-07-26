using System.Collections.Generic;

namespace AsmResolver.DotNet.Signatures.Parsing;

internal class TypeName(TypeName? declaringType, string? ns, string name)
{
    public TypeName? DeclaringType { get; } = declaringType;

    public string? Namespace { get; } = ns;

    public string Name { get; } = name;

    public ITypeDefOrRef ToTypeDefOrRef(ModuleDefinition contextModule, IResolutionScope? scope)
    {
        // Short circuit corlib types to avoid allocations.
        if (DeclaringType is null && contextModule.CorLibTypeFactory.FromName(Namespace, Name) is { } corlibType)
            return corlibType.Type;

        scope = DeclaringType?.ToTypeDefOrRef(contextModule, scope) as IResolutionScope ?? scope;
        var type = new TypeReference(contextModule, scope, Namespace, Name);

        // If the scope is null, it means it was omitted from the fully qualified type name.
        // In this case, the CLR first looks into the current assembly, and then into corlib.
        if (scope is null)
        {
            // First look into the current module.
            type.Scope = contextModule;
            if (!type.TryResolve(contextModule.RuntimeContext, out var definition))
            {
                // If that fails, try corlib.
                // However, we would prefer to use the implementation corlib for the runtime targeted, not the one it was compiled against.
                if (contextModule.RuntimeContext?.RuntimeCorLib is {} runtimeCorLib)
                {
                    type.Scope = new AssemblyReference(runtimeCorLib);
                    type.TryResolve(contextModule.RuntimeContext, out definition);
                }

                if (definition is null)
                    type.Scope = null;
            }
        }

        return type;
    }
}
