using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures.Types;
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
        public bool Equals(TypeSignature? x, TypeSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
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
        public int GetHashCode(TypeSignature obj)
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
        public bool Equals(CorLibTypeSignature? x, CorLibTypeSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;
            return x.ElementType == y.ElementType;
        }

        /// <inheritdoc />
        public int GetHashCode(CorLibTypeSignature obj) =>
            (int) obj.ElementType << ElementTypeOffset;

        /// <inheritdoc />
        public bool Equals(SentinelTypeSignature? x, SentinelTypeSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;
            return x.ElementType == y.ElementType;
        }

        /// <inheritdoc />
        public int GetHashCode(SentinelTypeSignature obj) =>
            (int) obj.ElementType << ElementTypeOffset;

        /// <inheritdoc />
        public bool Equals(ByReferenceTypeSignature? x, ByReferenceTypeSignature? y) =>
            Equals(x as TypeSpecificationSignature, y);

        /// <inheritdoc />
        public int GetHashCode(ByReferenceTypeSignature obj) =>
            GetHashCode(obj as TypeSpecificationSignature);

        /// <inheritdoc />
        public bool Equals(PointerTypeSignature? x, PointerTypeSignature? y) =>
            Equals(x as TypeSpecificationSignature, y);

        /// <inheritdoc />
        public int GetHashCode(PointerTypeSignature obj) =>
            GetHashCode(obj as TypeSpecificationSignature);

        /// <inheritdoc />
        public bool Equals(SzArrayTypeSignature? x, SzArrayTypeSignature? y) =>
            Equals(x as TypeSpecificationSignature, y);

        /// <inheritdoc />
        public int GetHashCode(SzArrayTypeSignature obj) =>
            GetHashCode(obj as TypeSpecificationSignature);

        /// <inheritdoc />
        public bool Equals(PinnedTypeSignature? x, PinnedTypeSignature? y) =>
            Equals(x as TypeSpecificationSignature, y);

        /// <inheritdoc />
        public int GetHashCode(PinnedTypeSignature obj) =>
            GetHashCode(obj as TypeSpecificationSignature);

        /// <inheritdoc />
        public bool Equals(BoxedTypeSignature? x, BoxedTypeSignature? y) =>
            Equals(x as TypeSpecificationSignature, y);

        /// <inheritdoc />
        public int GetHashCode(BoxedTypeSignature obj) =>
            GetHashCode(obj as TypeSpecificationSignature);

        /// <inheritdoc />
        public bool Equals(TypeDefOrRefSignature? x, TypeDefOrRefSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;
            return SimpleTypeEquals(x.Type, y.Type);
        }

        /// <inheritdoc />
        public int GetHashCode(TypeDefOrRefSignature obj) => SimpleTypeHashCode(obj);

        /// <inheritdoc />
        public bool Equals(CustomModifierTypeSignature? x, CustomModifierTypeSignature? y)
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
        public int GetHashCode(CustomModifierTypeSignature obj)
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
        public bool Equals(GenericInstanceTypeSignature? x, GenericInstanceTypeSignature? y)
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
        public int GetHashCode(GenericInstanceTypeSignature obj)
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
        public bool Equals(GenericParameterSignature? x, GenericParameterSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return x.Index == y.Index
                   && x.ParameterType == y.ParameterType;
        }

        /// <inheritdoc />
        public int GetHashCode(GenericParameterSignature obj) =>
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
        public bool Equals(ArrayTypeSignature? x, ArrayTypeSignature? y)
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
        public int GetHashCode(ArrayTypeSignature obj)
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
        public bool Equals(FunctionPointerTypeSignature? x, FunctionPointerTypeSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;
            return Equals(x.Signature, y.Signature);
        }

        /// <inheritdoc />
        public int GetHashCode(FunctionPointerTypeSignature obj)
        {
            return (int) obj.ElementType << ElementTypeOffset ^ GetHashCode(obj.Signature);
        }

        /// <inheritdoc />
        public bool Equals(IList<TypeSignature>? x, IList<TypeSignature>? y)
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
        public int GetHashCode(IList<TypeSignature> obj)
        {
            int checksum = 0;
            for (int i = 0; i < obj.Count; i++)
                checksum ^= GetHashCode(obj[i]);
            return checksum;
        }

        /// <inheritdoc />
        public bool Equals(IEnumerable<TypeSignature>? x, IEnumerable<TypeSignature>? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
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
