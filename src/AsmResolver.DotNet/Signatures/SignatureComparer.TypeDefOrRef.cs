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
        public bool Equals(ITypeDescriptor x, ITypeDescriptor y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            switch (x)
            {
                case InvalidTypeDefOrRef invalidType:
                    return Equals(invalidType, y as InvalidTypeDefOrRef);

                case TypeSpecification specification:
                    return Equals(specification, y as TypeSpecification);

                case TypeSignature signature:
                    return Equals(signature, y as TypeSignature);

                default:
                    return SimpleTypeEquals(x, y);
            }
        }

        /// <inheritdoc />
        public int GetHashCode(ITypeDescriptor obj)
        {
            unchecked
            {
                int hashCode = obj.Name != null ? obj.Name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (obj.Namespace != null ? obj.Namespace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.DeclaringType != null ? GetHashCode(obj.DeclaringType) : 0);
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

            // If scope does not match, it can still be a reference to an exported type.
            return Equals(x.Resolve(), y.Resolve());
        }

        /// <inheritdoc />
        public bool Equals(ITypeDefOrRef x, ITypeDefOrRef y) => Equals((ITypeDescriptor) x, y);

        /// <inheritdoc />
        public int GetHashCode(ITypeDefOrRef obj) => GetHashCode((ITypeDescriptor) obj);

        /// <inheritdoc />
        public bool Equals(TypeDefinition x, TypeDefinition y) => Equals((ITypeDescriptor) x, y);

        /// <inheritdoc />
        public int GetHashCode(TypeDefinition obj) => GetHashCode((ITypeDescriptor) obj);

        /// <inheritdoc />
        public bool Equals(TypeReference x, TypeReference y) => Equals((ITypeDescriptor) x, y);

        /// <inheritdoc />
        public int GetHashCode(TypeReference obj) => GetHashCode((ITypeDescriptor) obj);

        /// <inheritdoc />
        public bool Equals(TypeSpecification x, TypeSpecification y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return Equals(x.Signature, y.Signature);
        }

        /// <inheritdoc />
        public int GetHashCode(TypeSpecification obj) => GetHashCode(obj.Signature);

        /// <inheritdoc />
        public bool Equals(ExportedType x, ExportedType y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return Equals((ITypeDescriptor) x, y);
        }

        /// <inheritdoc />
        public int GetHashCode(ExportedType obj) => GetHashCode((ITypeDescriptor) obj);

        /// <inheritdoc />
        public bool Equals(InvalidTypeDefOrRef x, InvalidTypeDefOrRef y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.Error == y.Error;
        }

        /// <inheritdoc />
        public int GetHashCode(InvalidTypeDefOrRef obj) => (int) obj.Error;
    }
}