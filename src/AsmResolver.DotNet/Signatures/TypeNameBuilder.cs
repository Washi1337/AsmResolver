using System;

namespace AsmResolver.DotNet.Signatures
{
    internal static class TypeNameBuilder
    {
        public static string GetAssemblyQualifiedName(TypeSignature signature)
        {
            string typeName = GetFullName(signature);
            var assembly = signature.Scope.GetAssembly();
            return $"{typeName}, {assembly.FullName}";
        }

        private static string GetFullName(TypeSignature signature)
        {
            return signature switch
            {
                TypeSpecificationSignature specification => GetFullName(specification),
                TypeDefOrRefSignature typeDefOrRef => typeDefOrRef.Type.FullName,
                CorLibTypeSignature corlibType => corlibType.FullName,
                _ => throw new NotSupportedException($"Invalid or unsupported type signature: {signature.FullName}.")
            };
        }

        private static string GetFullName(TypeSpecificationSignature specification)
        {
            string baseTypeName = GetFullName(specification.BaseType);

            return specification switch
            {
                ArrayTypeSignature arrayType => string.Format("{0}[{1}]", 
                    baseTypeName,
                    new string(',', arrayType.Dimensions.Count - 1)),
                SzArrayTypeSignature szArrayType => $"{baseTypeName}[]",
                PointerTypeSignature pointerType => $"{baseTypeName}*",
                ByReferenceTypeSignature byRefType => $"{baseTypeName}&",
                _ => throw new NotSupportedException($"Invalid or unsupported type signature: {specification}.")
            };
        }
        
    }
}