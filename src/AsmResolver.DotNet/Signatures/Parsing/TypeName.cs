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
                type.Scope = contextModule.CorLibTypeFactory.CorLibScope;
                definition = type.Resolve();

                // If both lookups fail, revert to the normal module as scope as a fallback.
                if (definition is null)
                    type.Scope = contextModule;
            }
        }

        // Walk over nested type names.
        for (int i = 1; i < Names.Count; i++)
            type = new TypeReference(type, null, Names[i]);

        return type;
    }
}
