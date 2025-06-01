using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures
{
    public partial class SignatureComparer :
        IEqualityComparer<ITypeDescriptor>,
        IEqualityComparer<ITypeDefOrRef>,
        IEqualityComparer<TypeDefinition>,
        IEqualityComparer<TypeReference>,
        IEqualityComparer<TypeSpecification>,
        IEqualityComparer<ExportedType>,
        IEqualityComparer<InvalidTypeDefOrRef>
    {
        /// <inheritdoc />
        public bool Equals(ITypeDescriptor? x, ITypeDescriptor? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return x switch
            {
                InvalidTypeDefOrRef invalidType => Equals(invalidType, y as InvalidTypeDefOrRef),
                TypeSpecification specification => Equals(specification, y as TypeSpecification),
                TypeSignature signature => Equals(signature, y as TypeSignature),
                _ => SimpleTypeEquals(x, y)
            };
        }

        /// <inheritdoc />
        public int GetHashCode(ITypeDescriptor obj) => obj switch
        {
            InvalidTypeDefOrRef invalidType => GetHashCode(invalidType),
            ITypeDefOrRef typeDefOrRef => GetHashCode(typeDefOrRef),
            TypeSignature signature => GetHashCode(signature),
            _ => SimpleTypeHashCode(obj)
        };

        private int SimpleTypeHashCode(ITypeDescriptor obj)
        {
            unchecked
            {
                int hashCode = obj.Name?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (obj.Namespace?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (obj.DeclaringType is null ? 0 : GetHashCode(obj.DeclaringType));
                return hashCode;
            }
        }

        private bool SimpleTypeEquals(ITypeDescriptor x, ITypeDescriptor y)
        {
            // Check the basic properties first.
            if (!x.IsTypeOf(y.Namespace, y.Name))
                return false;

            // If scope matches, it is a perfect match.
            if (Equals(x.Scope, y.Scope))
                return true;

            // It can still be an exported type, we need to resolve the type then and check if the definitions match.
            return x.Resolve() is { } definition1
                   && y.Resolve() is { } definition2
                   && Equals(definition1.Module!.Assembly, definition2.Module!.Assembly)
                   && Equals(definition1.DeclaringType, definition2.DeclaringType);
        }

        /// <inheritdoc />
        public bool Equals(ITypeDefOrRef? x, ITypeDefOrRef? y) => Equals(x as ITypeDescriptor, y);

        /// <inheritdoc />
        public int GetHashCode(ITypeDefOrRef obj) => obj.MetadataToken.Table == TableIndex.TypeSpec
            ? GetHashCode((TypeSpecification) obj)
            : SimpleTypeHashCode(obj);

        /// <inheritdoc />
        public bool Equals(TypeDefinition? x, TypeDefinition? y) => Equals(x as ITypeDescriptor, y);

        /// <inheritdoc />
        public int GetHashCode(TypeDefinition obj) => SimpleTypeHashCode(obj);

        /// <inheritdoc />
        public bool Equals(TypeReference? x, TypeReference? y) => Equals(x as ITypeDescriptor, y);

        /// <inheritdoc />
        public int GetHashCode(TypeReference obj) => SimpleTypeHashCode(obj);

        /// <inheritdoc />
        public bool Equals(TypeSpecification? x, TypeSpecification? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return Equals(x.Signature, y.Signature);
        }

        /// <inheritdoc />
        public int GetHashCode(TypeSpecification obj) => obj.Signature is not null ? GetHashCode(obj.Signature) : 0;

        /// <inheritdoc />
        public bool Equals(ExportedType? x, ExportedType? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return Equals((ITypeDescriptor) x, y);
        }

        /// <inheritdoc />
        public int GetHashCode(ExportedType obj) => GetHashCode((ITypeDescriptor) obj);

        /// <inheritdoc />
        public bool Equals(InvalidTypeDefOrRef? x, InvalidTypeDefOrRef? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return x.Error == y.Error;
        }

        /// <inheritdoc />
        public int GetHashCode(InvalidTypeDefOrRef obj) => (int) obj.Error;
    }
}
