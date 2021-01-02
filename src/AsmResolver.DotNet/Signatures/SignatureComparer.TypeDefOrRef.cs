using System.Collections.Generic;

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
                case TypeSpecification typeSpec:
                    return Equals(typeSpec, y as TypeSpecification);
                default:
                    return x.Name == y.Name
                           && x.Namespace == y.Namespace
                           && Equals(x.Scope, y.Scope);
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