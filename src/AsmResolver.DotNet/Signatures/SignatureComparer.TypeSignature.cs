using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.DotNet.Signatures
{
    public partial class SignatureComparer : 
        IEqualityComparer<TypeSignature>, 
        IEqualityComparer<CorLibTypeSignature>,
        IEqualityComparer<ByReferenceTypeSignature>,
        IEqualityComparer<PointerTypeSignature>,
        IEqualityComparer<SzArrayTypeSignature>,
        IEqualityComparer<TypeDefOrRefSignature>,
        IEqualityComparer<CustomModifierTypeSignature>,
        IEqualityComparer<GenericInstanceTypeSignature>,
        IEqualityComparer<GenericParameterSignature>,
        IEqualityComparer<IEnumerable<TypeSignature>>
    {
        /// <inheritdoc />
        public bool Equals(TypeSignature x, TypeSignature y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x switch
            {
                CorLibTypeSignature corLibType => Equals(corLibType, y as CorLibTypeSignature),
                ByReferenceTypeSignature byRefType => Equals(byRefType, y as ByReferenceTypeSignature),
                CustomModifierTypeSignature modifierType => Equals(modifierType, y as CustomModifierTypeSignature),
                PointerTypeSignature pointerType => Equals(pointerType, y as PointerTypeSignature),
                SzArrayTypeSignature szArrayType => Equals(szArrayType, y as SzArrayTypeSignature),
                TypeDefOrRefSignature typeDefOrRef => Equals(typeDefOrRef, y as TypeDefOrRefSignature),
                _ => throw new NotSupportedException()
            };
        }

        /// <inheritdoc />
        public int GetHashCode(TypeSignature obj)
        {
            return obj switch
            {
                CorLibTypeSignature corLibType => GetHashCode(corLibType),
                ByReferenceTypeSignature byRefType => GetHashCode(byRefType),
                PointerTypeSignature pointerType => GetHashCode(pointerType),
                SzArrayTypeSignature szArrayType => GetHashCode(szArrayType),
                TypeDefOrRefSignature typeDefOrRef => GetHashCode(typeDefOrRef),
                _ => throw new NotSupportedException()
            };
        }

        /// <inheritdoc />
        public bool Equals(CorLibTypeSignature x, CorLibTypeSignature y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;
            return x.ElementType == y.ElementType;
        }

        /// <inheritdoc />
        public int GetHashCode(CorLibTypeSignature obj)
        {
            return (int) obj.ElementType << ElementTypeOffset;
        }

        /// <inheritdoc />
        public bool Equals(ByReferenceTypeSignature x, ByReferenceTypeSignature y)
        {
            return Equals(x as TypeSpecificationSignature, y);
        }

        /// <inheritdoc />
        public int GetHashCode(ByReferenceTypeSignature obj)
        {
            return GetHashCode(obj as TypeSpecificationSignature);
        }

        /// <inheritdoc />
        public bool Equals(PointerTypeSignature x, PointerTypeSignature y)
        {
            return Equals(x as TypeSpecificationSignature, y);
        }

        /// <inheritdoc />
        public int GetHashCode(PointerTypeSignature obj)
        {
            return GetHashCode(obj as TypeSpecificationSignature);
        }

        /// <inheritdoc />
        public bool Equals(SzArrayTypeSignature x, SzArrayTypeSignature y)
        {
            return Equals(x as TypeSpecificationSignature, y);
        }

        /// <inheritdoc />
        public int GetHashCode(SzArrayTypeSignature obj)
        {
            return GetHashCode(obj as TypeSpecificationSignature);
        }

        /// <inheritdoc />
        public bool Equals(TypeDefOrRefSignature x, TypeDefOrRefSignature y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.Name == y.Name
                   && x.Namespace == y.Namespace
                   && Equals(x.Scope, y.Scope);
        }

        /// <inheritdoc />
        public int GetHashCode(TypeDefOrRefSignature obj)
        {
            unchecked
            {
                int hashCode = (int) obj.ElementType << ElementTypeOffset;
                hashCode = (hashCode * 397) ^ (obj.Name != null ? obj.Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Namespace != null ? obj.Namespace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Scope != null ? GetHashCode(obj.Scope) : 0);
                return hashCode;
            }
        }

        /// <inheritdoc />
        public bool Equals(CustomModifierTypeSignature x, CustomModifierTypeSignature y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.IsRequired == y.IsRequired
                   && Equals(x.ModifierType, y.ModifierType)
                   && Equals(x.BaseType, y.BaseType);
        }

        /// <inheritdoc />
        public int GetHashCode(CustomModifierTypeSignature obj)
        {
            unchecked
            {
                int hashCode = (int) obj.ElementType << ElementTypeOffset;
                hashCode = (hashCode * 397) ^ (obj.ModifierType != null ? obj.ModifierType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.BaseType != null ? obj.BaseType.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <inheritdoc />
        public bool Equals(GenericInstanceTypeSignature x, GenericInstanceTypeSignature y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.IsValueType == y.IsValueType
                   && Equals(x.GenericType, y.GenericType)
                   && Equals(x.TypeArguments, y.TypeArguments);
        }

        /// <inheritdoc />
        public int GetHashCode(GenericInstanceTypeSignature obj)
        {
            unchecked
            {
                int hashCode = (int) obj.ElementType << ElementTypeOffset;
                hashCode = (hashCode * 397) ^ (obj.GenericType != null ? obj.GenericType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ GetHashCode(obj.TypeArguments);
                return hashCode;
            }
        }
        
        /// <inheritdoc />
        public bool Equals(GenericParameterSignature x, GenericParameterSignature y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.Index == y.Index
                   && x.ParameterType == y.ParameterType;
        }

        /// <inheritdoc />
        public int GetHashCode(GenericParameterSignature obj)
        {
            return (int) obj.ElementType << ElementTypeOffset | obj.Index;
        }
        
        private bool Equals(TypeSpecificationSignature x, TypeSpecificationSignature y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null) || x.ElementType != y.ElementType)
                return false;
            return Equals(x.BaseType, y.BaseType);
        }
        
        private int GetHashCode(TypeSpecificationSignature obj)
        {
            return (int) obj.ElementType << ElementTypeOffset ^ GetHashCode(obj.BaseType);
        }

        /// <inheritdoc />
        public bool Equals(IEnumerable<TypeSignature> x, IEnumerable<TypeSignature> y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.SequenceEqual(y, this);
        }

        /// <inheritdoc />
        public int GetHashCode(IEnumerable<TypeSignature> obj)
        {
            int checksum = 0;
            foreach (var type in obj)
                checksum ^= GetHashCode(type);
            return checksum;
        }
    }
}