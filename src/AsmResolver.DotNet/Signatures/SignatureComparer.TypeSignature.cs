using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Signatures
{
    public partial class SignatureComparer :
        IEqualityComparer<TypeSignature>,
        IEqualityComparer<CorLibTypeSignature>,
        IEqualityComparer<ByReferenceTypeSignature>,
        IEqualityComparer<PointerTypeSignature>,
        IEqualityComparer<SzArrayTypeSignature>,
        IEqualityComparer<PinnedTypeSignature>,
        IEqualityComparer<BoxedTypeSignature>,
        IEqualityComparer<TypeDefOrRefSignature>,
        IEqualityComparer<CustomModifierTypeSignature>,
        IEqualityComparer<GenericInstanceTypeSignature>,
        IEqualityComparer<GenericParameterSignature>,
        IEqualityComparer<ArrayTypeSignature>,
        IEqualityComparer<SentinelTypeSignature>,
        IEqualityComparer<FunctionPointerTypeSignature>,
        IEqualityComparer<IList<TypeSignature>>,
        IEqualityComparer<IEnumerable<TypeSignature>>
    {
        /// <inheritdoc />
        public virtual bool Equals(TypeSignature? x, TypeSignature? y) => Equals((ITypeDescriptor?)x, y);

        private bool SignatureEquals(TypeSignature? x, TypeSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;
            if (x.ElementType != y.ElementType)
                return false;

            switch (x.ElementType)
            {
                case ElementType.ValueType:
                case ElementType.Class:
                    return Equals(x as TypeDefOrRefSignature, y as TypeDefOrRefSignature);
                case ElementType.CModReqD:
                case ElementType.CModOpt:
                    return Equals(x as CustomModifierTypeSignature, y as CustomModifierTypeSignature);
                case ElementType.GenericInst:
                    return Equals(x as GenericInstanceTypeSignature, y as GenericInstanceTypeSignature);
                case ElementType.Var:
                case ElementType.MVar:
                    return Equals(x as GenericParameterSignature, y as GenericParameterSignature);
                case ElementType.Ptr:
                    return Equals(x as PointerTypeSignature, y as PointerTypeSignature);
                case ElementType.ByRef:
                    return Equals(x as ByReferenceTypeSignature, y as ByReferenceTypeSignature);
                case ElementType.Array:
                    return Equals(x as ArrayTypeSignature, y as ArrayTypeSignature);
                case ElementType.SzArray:
                    return Equals(x as SzArrayTypeSignature, y as SzArrayTypeSignature);
                case ElementType.Sentinel:
                    return Equals(x as SentinelTypeSignature, y as SentinelTypeSignature);
                case ElementType.Pinned:
                    return Equals(x as PinnedTypeSignature, y as PinnedTypeSignature);
                case ElementType.Boxed:
                    return Equals(x as BoxedTypeSignature, y as BoxedTypeSignature);
                case ElementType.FnPtr:
                    return Equals(x as FunctionPointerTypeSignature, y as FunctionPointerTypeSignature);
                case ElementType.Internal:
                case ElementType.Modifier:
                    throw new NotSupportedException();
                default:
                    return Equals(x as CorLibTypeSignature, y as CorLibTypeSignature);
            }
        }

        /// <inheritdoc />
        public virtual int GetHashCode(TypeSignature obj)
        {
            switch (obj.ElementType)
            {
                case ElementType.ValueType:
                case ElementType.Class:
                    return GetHashCode((TypeDefOrRefSignature) obj);
                case ElementType.CModReqD:
                case ElementType.CModOpt:
                    return GetHashCode((CustomModifierTypeSignature) obj);
                case ElementType.GenericInst:
                    return GetHashCode((GenericInstanceTypeSignature) obj);
                case ElementType.Var:
                case ElementType.MVar:
                    return GetHashCode((GenericParameterSignature) obj);
                case ElementType.Ptr:
                    return GetHashCode((PointerTypeSignature) obj);
                case ElementType.ByRef:
                    return GetHashCode((ByReferenceTypeSignature) obj);
                case ElementType.Array:
                    return GetHashCode((ArrayTypeSignature) obj);
                case ElementType.SzArray:
                    return GetHashCode((SzArrayTypeSignature) obj);
                case ElementType.Sentinel:
                    return GetHashCode((SentinelTypeSignature) obj);
                case ElementType.Pinned:
                    return GetHashCode((PinnedTypeSignature) obj);
                case ElementType.Boxed:
                    return GetHashCode((BoxedTypeSignature) obj);
                case ElementType.FnPtr:
                    return GetHashCode((FunctionPointerTypeSignature) obj);
                case ElementType.Internal:
                case ElementType.Modifier:
                    throw new NotSupportedException();
                default:
                    return GetHashCode((CorLibTypeSignature) obj);
            }
        }

        /// <inheritdoc />
        public virtual bool Equals(CorLibTypeSignature? x, CorLibTypeSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;
            return x.ElementType == y.ElementType;
        }

        /// <inheritdoc />
        public virtual int GetHashCode(CorLibTypeSignature obj) =>
            (int) obj.ElementType << ElementTypeOffset;

        /// <inheritdoc />
        public virtual bool Equals(SentinelTypeSignature? x, SentinelTypeSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;
            return x.ElementType == y.ElementType;
        }

        /// <inheritdoc />
        public virtual int GetHashCode(SentinelTypeSignature obj) =>
            (int) obj.ElementType << ElementTypeOffset;

        /// <inheritdoc />
        public virtual bool Equals(ByReferenceTypeSignature? x, ByReferenceTypeSignature? y) =>
            Equals(x as TypeSpecificationSignature, y);

        /// <inheritdoc />
        public virtual int GetHashCode(ByReferenceTypeSignature obj) =>
            GetHashCode(obj as TypeSpecificationSignature);

        /// <inheritdoc />
        public virtual bool Equals(PointerTypeSignature? x, PointerTypeSignature? y) =>
            Equals(x as TypeSpecificationSignature, y);

        /// <inheritdoc />
        public virtual int GetHashCode(PointerTypeSignature obj) =>
            GetHashCode(obj as TypeSpecificationSignature);

        /// <inheritdoc />
        public virtual bool Equals(SzArrayTypeSignature? x, SzArrayTypeSignature? y) =>
            Equals(x as TypeSpecificationSignature, y);

        /// <inheritdoc />
        public virtual int GetHashCode(SzArrayTypeSignature obj) =>
            GetHashCode(obj as TypeSpecificationSignature);

        /// <inheritdoc />
        public virtual bool Equals(PinnedTypeSignature? x, PinnedTypeSignature? y) =>
            Equals(x as TypeSpecificationSignature, y);

        /// <inheritdoc />
        public virtual int GetHashCode(PinnedTypeSignature obj) =>
            GetHashCode(obj as TypeSpecificationSignature);

        /// <inheritdoc />
        public virtual bool Equals(BoxedTypeSignature? x, BoxedTypeSignature? y) =>
            Equals(x as TypeSpecificationSignature, y);

        /// <inheritdoc />
        public virtual int GetHashCode(BoxedTypeSignature obj) =>
            GetHashCode(obj as TypeSpecificationSignature);

        /// <inheritdoc />
        public virtual bool Equals(TypeDefOrRefSignature? x, TypeDefOrRefSignature? y) => Equals(x?.Type, y?.Type);

        /// <inheritdoc />
        public virtual int GetHashCode(TypeDefOrRefSignature obj) => SimpleTypeHashCode(obj);

        /// <inheritdoc />
        public virtual bool Equals(CustomModifierTypeSignature? x, CustomModifierTypeSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return x.IsRequired == y.IsRequired
                   && Equals(x.ModifierType, y.ModifierType)
                   && Equals(x.BaseType, y.BaseType);
        }

        /// <inheritdoc />
        public virtual int GetHashCode(CustomModifierTypeSignature obj)
        {
            unchecked
            {
                int hashCode = (int) obj.ElementType << ElementTypeOffset;
                hashCode = (hashCode * 397) ^ GetHashCode(obj.ModifierType);
                hashCode = (hashCode * 397) ^ GetHashCode(obj.BaseType);
                return hashCode;
            }
        }

        /// <inheritdoc />
        public virtual bool Equals(GenericInstanceTypeSignature? x, GenericInstanceTypeSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return x.IsValueType == y.IsValueType
                   && Equals(x.GenericType, y.GenericType)
                   && Equals(x.TypeArguments, y.TypeArguments);
        }

        /// <inheritdoc />
        public virtual int GetHashCode(GenericInstanceTypeSignature obj)
        {
            unchecked
            {
                int hashCode = (int) obj.ElementType << ElementTypeOffset;
                hashCode = (hashCode * 397) ^ GetHashCode(obj.GenericType);
                hashCode = (hashCode * 397) ^ GetHashCode(obj.TypeArguments);
                return hashCode;
            }
        }

        /// <inheritdoc />
        public virtual bool Equals(GenericParameterSignature? x, GenericParameterSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return x.Index == y.Index
                   && x.ParameterType == y.ParameterType;
        }

        /// <inheritdoc />
        public virtual int GetHashCode(GenericParameterSignature obj) =>
            (int) obj.ElementType << ElementTypeOffset | obj.Index;

        private bool Equals(TypeSpecificationSignature? x, TypeSpecificationSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null || x.ElementType != y.ElementType)
                return false;
            return Equals(x.BaseType, y.BaseType);
        }

        private int GetHashCode(TypeSpecificationSignature obj) => SimpleTypeSpecHashCode(obj);

        private int SimpleTypeSpecHashCode(TypeSpecificationSignature obj)
        {
            return (int) obj.ElementType << ElementTypeOffset ^ GetHashCode(obj.BaseType);
        }

        /// <inheritdoc />
        public virtual bool Equals(ArrayTypeSignature? x, ArrayTypeSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null || x.Dimensions.Count != y.Dimensions.Count)
                return false;

            for (int i = 0; i < x.Dimensions.Count; i++)
            {
                if (x.Dimensions[i].Size != y.Dimensions[i].Size
                    || x.Dimensions[i].LowerBound != y.Dimensions[i].LowerBound)
                {
                    return false;
                }
            }

            return Equals(x.BaseType, y.BaseType);
        }

        /// <inheritdoc />
        public virtual int GetHashCode(ArrayTypeSignature obj)
        {
            unchecked
            {
                int hashCode = (int) obj.ElementType << ElementTypeOffset;
                hashCode = (hashCode * 397) ^ GetHashCode(obj.BaseType);
                for (int i = 0; i < obj.Dimensions.Count; i++)
                    hashCode = (hashCode * 397) ^ obj.Dimensions[i].GetHashCode();

                return hashCode;
            }
        }

        /// <inheritdoc />
        public virtual bool Equals(FunctionPointerTypeSignature? x, FunctionPointerTypeSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;
            return Equals(x.Signature, y.Signature);
        }

        /// <inheritdoc />
        public virtual int GetHashCode(FunctionPointerTypeSignature obj)
        {
            return (int) obj.ElementType << ElementTypeOffset ^ GetHashCode(obj.Signature);
        }

        /// <inheritdoc />
        public virtual bool Equals(IList<TypeSignature>? x, IList<TypeSignature>? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null || x.Count != y.Count)
                return false;

            for (int i = 0; i < x.Count; i++)
            {
                if (!Equals(x[i], y[i]))
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public virtual int GetHashCode(IList<TypeSignature> obj)
        {
            int checksum = 0;
            for (int i = 0; i < obj.Count; i++)
                checksum ^= GetHashCode(obj[i]);
            return checksum;
        }

        /// <inheritdoc />
        public virtual bool Equals(IEnumerable<TypeSignature>? x, IEnumerable<TypeSignature>? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return x.SequenceEqual(y, this);
        }

        /// <inheritdoc />
        public virtual int GetHashCode(IEnumerable<TypeSignature> obj)
        {
            int checksum = 0;
            foreach (var type in obj)
                checksum ^= GetHashCode(type);
            return checksum;
        }
    }
}
