using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures
{
    public partial class SignatureComparer : 
        IEqualityComparer<ITypeDefOrRef>,
        IEqualityComparer<TypeDefinition>,
        IEqualityComparer<TypeReference>,
        IEqualityComparer<TypeSpecification>,
        IEqualityComparer<InvalidTypeDefOrRef>
    {
        /// <inheritdoc />
        public bool Equals(ITypeDefOrRef x, ITypeDefOrRef y)
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
        public int GetHashCode(ITypeDefOrRef obj)
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
        public bool Equals(TypeDefinition x, TypeDefinition y)
        {
            return Equals((ITypeDefOrRef) x, y);
        }

        /// <inheritdoc />
        public int GetHashCode(TypeDefinition obj)
        {
            return GetHashCode((ITypeDefOrRef) obj);
        }

        /// <inheritdoc />
        public bool Equals(TypeReference x, TypeReference y)
        {
            return Equals((ITypeDefOrRef) x, y);
        }

        /// <inheritdoc />
        public int GetHashCode(TypeReference obj)
        {
            return GetHashCode((ITypeDefOrRef) obj);
        }

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
        public int GetHashCode(TypeSpecification obj)
        {
            return GetHashCode(obj.Signature);
        }

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
        public int GetHashCode(InvalidTypeDefOrRef obj)
        {
            return (int) obj.Error;
        }
    }
}