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

    public static readonly DiagnosticDescriptor TooManyLazyProperties = new(
        "AR0002",
        "Classes can define up to 32 lazy properties",
        "Lazy initialized class {0} can only define 32 lazy properties but {1} were defined",
        "LazyGenerator",
        DiagnosticSeverity.Error,
        true
    );
}
