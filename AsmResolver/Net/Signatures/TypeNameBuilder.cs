using System;

namespace AsmResolver.Net.Signatures
{
    public static class TypeNameBuilder
    {
        public static string GetAssemblyQualifiedName(TypeSignature signature)
        {
            string typeName = GetFullName(signature);
            var assembly = signature.ResolutionScope.GetAssembly();
            return typeName + ", " + assembly.FullName;
        }

        private static string GetFullName(TypeSignature signature)
        {
            var specification = signature as TypeSpecificationSignature;
            if (specification != null)
                return GetFullName(specification);

            var typeDefOrRef = signature as TypeDefOrRefSignature;
            if (typeDefOrRef != null)
                return typeDefOrRef.Type.FullName;

            var corlibType = signature as MsCorLibTypeSignature;
            if (corlibType != null)
                return corlibType.FullName;

            throw new NotSupportedException("Invalid or unsupported type signature: " + signature.FullName + ".");
        }
        

        private static string GetFullName(TypeSpecificationSignature specification)
        {
            string baseTypeName = GetFullName(specification.BaseType);

            var arrayType = specification as ArrayTypeSignature;
            if (arrayType != null)
                return baseTypeName + '[' + new string(',', arrayType.Dimensions.Count - 1) + ']';

            var szArrayType = specification as SzArrayTypeSignature;
            if (szArrayType != null)
                return baseTypeName + "[]";

            var pointerType = specification as PointerTypeSignature;
            if (pointerType != null)
                return baseTypeName + "*";

            var byRefType = specification as ByReferenceTypeSignature;
            if (byRefType != null)
                return baseTypeName + "&";

            throw new NotSupportedException("Invalid or unsupported type signature: " + specification.FullName + ".");
        }
        
    }
}
