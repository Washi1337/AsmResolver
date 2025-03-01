using System.Collections.Generic;

namespace AsmResolver.DotNet.Signatures.Parsing;

internal readonly struct TypeAnnotation
{
    private readonly object? _data;

    public TypeAnnotation(TypeAnnotationType kind)
    {
        Kind = kind;
        _data = null;
    }

    public TypeAnnotation(IList<ArrayDimension> dimensions)
    {
        Kind = TypeAnnotationType.Array;
        _data = dimensions;
    }

    public TypeAnnotation(IList<ParsedTypeFullName> typeArguments)
    {
        Kind = TypeAnnotationType.GenericInstance;
        _data = typeArguments;
    }

    public TypeAnnotationType Kind { get; }

    public IList<ArrayDimension> Dimensions => (IList<ArrayDimension>) _data!;

    public IList<ParsedTypeFullName> TypeArguments => (IList<ParsedTypeFullName>) _data!;
}
