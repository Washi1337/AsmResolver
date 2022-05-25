using System.Collections.Generic;
using AsmResolver.DotNet.Signatures.Types;

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
        public int GetHashCode(ITypeDescriptor obj)
        {
            unchecked
            {
                int hashCode = obj.Name is null ? 0 : obj.Name.GetHashCode();
                hashCode = (hashCode * 397) ^ (obj.Namespace is null ? 0 : obj.Namespace.GetHashCode());
                hashCode = (hashCode * 397) ^ (obj.DeclaringType is null ? 0 : GetHashCode(obj.DeclaringType));
                return hashCode;
            }
        }

        private bool SimpleTypeEquals(ITypeDescriptor x, ITypeDescriptor y)
        {
            // Check the basic properties first.
            if (!x.IsTypeOf(y.Namespace, y.Name))
                return false;

            if (x.DeclaringType != null && y.DeclaringType != null && !Equals(x.DeclaringType, y.DeclaringType))
                return false;

            var scopeCheck = Equals(x.Scope, y.Scope);
            // If scope matches, it is a perfect match.
            if (scopeCheck || (!scopeCheck && x.Resolve() is { } definition1 && y.Resolve() is { } definition2 && Equals(definition1.Module!.Assembly, definition2.Module!.Assembly)))
                return true;
            return false;
        }

        /// <inheritdoc />
        public bool Equals(ITypeDefOrRef? x, ITypeDefOrRef? y) => Equals(x as ITypeDescriptor, y);

        /// <inheritdoc />
        public int GetHashCode(ITypeDefOrRef obj) => GetHashCode((ITypeDescriptor) obj);

        /// <inheritdoc />
        public bool Equals(TypeDefinition? x, TypeDefinition? y) => Equals(x as ITypeDescriptor, y);

        /// <inheritdoc />
        public int GetHashCode(TypeDefinition obj) => GetHashCode((ITypeDescriptor) obj);

        /// <inheritdoc />
        public bool Equals(TypeReference? x, TypeReference? y) => Equals(x as ITypeDescriptor, y);

        /// <inheritdoc />
        public int GetHashCode(TypeReference obj) => GetHashCode((ITypeDescriptor) obj);

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
