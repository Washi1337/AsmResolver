using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.DotNet.Signatures.Parsing;

internal sealed class ParsedTypeFullName(TypeName name)
{
    public TypeName Name { get; } = name;

    public IList<TypeAnnotation> Annotations { get; } = new List<TypeAnnotation>();

    public IResolutionScope? Scope { get; set; }

    public TypeSignature ToTypeSignature(ModuleDefinition contextModule)
    {
        var baseTypeDefOrRef = Name.ToTypeDefOrRef(contextModule, Scope);

        // The first annotation may be a generic instantiation, of which the root typesig is represented by
        // GenericInstanceTypeSignature.
        int startIndex = 0;
        TypeSignature signature;
        if (Annotations.Count > 0 && Annotations[0] is {Kind: TypeAnnotationType.GenericInstance} first)
        {
            var arguments = new TypeSignature[first.TypeArguments.Count];
            for (int i = 0; i < first.TypeArguments.Count; i++)
                arguments[i] = first.TypeArguments[i].ToTypeSignature(contextModule);

            signature = baseTypeDefOrRef.MakeGenericInstanceType(arguments);
            startIndex++;
        }
        else
        {
            signature = baseTypeDefOrRef.ToTypeSignature();
        }

        // Go over all type annotations and annotate the type signature accordingly.
        for (int i = startIndex; i < Annotations.Count; i++)
        {
            signature = Annotations[i].Kind switch
            {
                TypeAnnotationType.ByReference => signature.MakeByReferenceType(),
                TypeAnnotationType.Pointer => signature.MakePointerType(),
                TypeAnnotationType.SzArray => signature.MakeSzArrayType(),
                TypeAnnotationType.Array => signature.MakeArrayType(Annotations[i].Dimensions.ToArray()),
                TypeAnnotationType.GenericInstance => throw new FormatException("Cannot instantiate a non-generic type."),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return signature;
    }
}
