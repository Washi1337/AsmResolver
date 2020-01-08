using System;
using System.Collections.Generic;

namespace AsmResolver.DotNet.Blob
{
    public partial class SignatureComparer : 
        IEqualityComparer<CallingConventionSignature>,
        IEqualityComparer<FieldSignature>,
        IEqualityComparer<MethodSignature>,
        IEqualityComparer<LocalVariablesSignature>
    {
        /// <inheritdoc />
        public bool Equals(CallingConventionSignature x, CallingConventionSignature y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x switch
            {
                LocalVariablesSignature localVarSig => Equals(localVarSig, y as LocalVariablesSignature),
                FieldSignature fieldSig => Equals(fieldSig, y as FieldSignature),
                MethodSignature methodSig => Equals(methodSig, y as MethodSignature),
                PropertySignature propertySig => Equals(propertySig, y as PropertySignature),
                _ => false
            };
        }

        /// <inheritdoc />
        public int GetHashCode(CallingConventionSignature obj)
        {
            return obj switch
            {
                LocalVariablesSignature localVarSig => GetHashCode(localVarSig),
                FieldSignature fieldSig => GetHashCode(fieldSig),
                MethodSignature methodSig => GetHashCode(methodSig),
                PropertySignature propertySig => GetHashCode(propertySig),
                _ => throw new ArgumentOutOfRangeException(nameof(obj))
            };
        }

        /// <inheritdoc />
        public bool Equals(FieldSignature x, FieldSignature y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.Attributes == y.Attributes
                   && Equals(x.FieldType, y.FieldType);
        }

        /// <inheritdoc />
        public int GetHashCode(FieldSignature obj)
        {
            unchecked
            {
                int hashCode = (int) obj.Attributes;
                hashCode = (hashCode * 397) ^ (obj.FieldType != null ? obj.FieldType.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <inheritdoc />
        public bool Equals(MethodSignature x, MethodSignature y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.Attributes == y.Attributes
                   && x.GenericParameterCount == y.GenericParameterCount
                   && Equals(x.ReturnType, y.ReturnType)
                   && Equals(x.ParameterTypes, y.ParameterTypes)
                   && Equals(x.SentinelParameterTypes, y.SentinelParameterTypes);
        }

        /// <inheritdoc />
        public int GetHashCode(MethodSignature obj)
        {
            unchecked
            {
                int hashCode = (int) obj.Attributes;
                hashCode = (hashCode * 397) ^ obj.GenericParameterCount;
                hashCode = (hashCode * 397) ^ (obj.ReturnType != null ? obj.ReturnType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ GetHashCode(obj.ParameterTypes);
                hashCode = (hashCode * 397) ^ GetHashCode(obj.SentinelParameterTypes);
                return hashCode;
            }
        }

        /// <inheritdoc />
        public bool Equals(LocalVariablesSignature x, LocalVariablesSignature y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.Attributes == y.Attributes
                   && Equals(x.VariableTypes, y.VariableTypes);
        }

        /// <inheritdoc />
        public int GetHashCode(LocalVariablesSignature obj)
        {
            unchecked
            {
                int hashCode = (int) obj.Attributes;
                hashCode = (hashCode * 397) ^ GetHashCode(obj.VariableTypes);
                return hashCode;
            }
        }
    }
}