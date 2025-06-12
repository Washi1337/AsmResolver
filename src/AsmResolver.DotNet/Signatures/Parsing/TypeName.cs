using System.Collections.Generic;

namespace AsmResolver.DotNet.Signatures.Parsing;

internal readonly struct TypeName(string? ns, IList<string> names)
{
    public string? Namespace { get; } = ns;

    public IList<string> Names { get; } = names;

    public ITypeDefOrRef ToTypeDefOrRef(ModuleDefinition contextModule, IResolutionScope? scope)
    {
        // Short circuit corlib types to avoid allocations.
        if (Names.Count == 1 && contextModule.CorLibTypeFactory.FromName(Namespace, Names[0]) is {} corlibType)
            return corlibType.Type;

        var type = new TypeReference(contextModule, scope, Namespace, Names[0]);

        // If the scope is null, it means it was omitted from the fully qualified type name.
        // In this case, the CLR first looks into the current assembly, and then into corlib.
        if (scope is null)
        {
            // First look into the current module.
            type.Scope = contextModule;
            var definition = type.Resolve();
            if (definition is null)
            {
                // If that fails, try corlib.
                // However, we would prefer to use the implementation corlib for the runtime targeted, not the one it was compiled against.
                if (contextModule.OriginalTargetRuntime.GetAssumedImplCorLib() is { } implCorLib)
                {
                    type.Scope = implCorLib;
                    definition = type.Resolve();
                }

                if (definition is null)
                {
                    // fall back to the CorLibScope
                    type.Scope = contextModule.CorLibTypeFactory.CorLibScope;
                    definition = type.Resolve();
                    if (definition is null)
                    {
                        // All lookups failed, leave no scope.
                        type.Scope = null;
                    }
                }
            }
        }

        // Walk over nested type names.
        for (int i = 1; i < Names.Count; i++)
            type = new TypeReference(contextModule, type, null, Names[i]);

        return type;
    }
}
