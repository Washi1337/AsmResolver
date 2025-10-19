using Microsoft.CodeAnalysis;

namespace AsmResolver.SourceGenerators;

public static class LazyPropertyDiagnostics
{
    public static readonly DiagnosticDescriptor LazyPropertyMustBePartial = new(
        "AR0001",
        "Lazy property requires a non-abstract and partial property",
        "Property {0} is marked as lazily initialized but is not a non-abstract partial property",
        "LazyGenerator",
        DiagnosticSeverity.Error,
        true
    );
}
