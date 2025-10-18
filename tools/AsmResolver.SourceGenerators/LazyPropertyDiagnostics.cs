using Microsoft.CodeAnalysis;

namespace AsmResolver.SourceGenerators;

public class LazyPropertyDiagnostics
{
    public static readonly DiagnosticDescriptor PropertyMustBePartial = new(
        "AR0001",
        "Lazy property needs to be marked partial",
        "Property {0} is marked with LazyAttribute but is not marked with the partial modifier",
        "LazyGenerator",
        DiagnosticSeverity.Error,
        true
    );
}
