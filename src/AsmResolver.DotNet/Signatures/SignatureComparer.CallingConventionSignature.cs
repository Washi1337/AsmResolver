using System;
using System.Collections.Generic;

namespace AsmResolver.DotNet.Signatures
{
    public partial class SignatureComparer :
        IEqualityComparer<CallingConventionSignature>,
        IEqualityComparer<FieldSignature>,
        IEqualityComparer<MethodSignature>,
        IEqualityComparer<PropertySignature>,
        IEqualityComparer<LocalVariablesSignature>,
        IEqualityComparer<GenericInstanceMethodSignature>
    {
        /// <inheritdoc />
        public virtual bool Equals(CallingConventionSignature? x, CallingConventionSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
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
        public virtual int GetHashCode(CallingConventionSignature obj)
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
        public virtual bool Equals(FieldSignature? x, FieldSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return x.Attributes == y.Attributes
                   && Equals(x.FieldType, y.FieldType);
        }

        /// <inheritdoc />
        public virtual int GetHashCode(FieldSignature obj)
        {
            unchecked
            {
                int hashCode = (int) obj.Attributes;
                hashCode = (hashCode * 397) ^ GetHashCode(obj.FieldType);
                return hashCode;
            }
        }

        /// <inheritdoc />
        public virtual bool Equals(MethodSignature? x, MethodSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return x.Attributes == y.Attributes
                   && x.GenericParameterCount == y.GenericParameterCount
                   && Equals(x.ReturnType, y.ReturnType)
                   && Equals(x.ParameterTypes, y.ParameterTypes)
                   && Equals(x.SentinelParameterTypes, y.SentinelParameterTypes);
        }

        /// <inheritdoc />
        public virtual int GetHashCode(MethodSignature obj)
        {
            unchecked
            {
                int hashCode = (int) obj.Attributes;
                hashCode = (hashCode * 397) ^ obj.GenericParameterCount;
                hashCode = (hashCode * 397) ^ GetHashCode(obj.ReturnType);
                hashCode = (hashCode * 397) ^ GetHashCode(obj.ParameterTypes);
                hashCode = (hashCode * 397) ^ GetHashCode(obj.SentinelParameterTypes);
                return hashCode;
            }
        }

        /// <inheritdoc />
        public virtual bool Equals(LocalVariablesSignature? x, LocalVariablesSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return x.Attributes == y.Attributes
                   && Equals(x.VariableTypes, y.VariableTypes);
        }

        /// <inheritdoc />
        public virtual int GetHashCode(LocalVariablesSignature obj)
        {
            unchecked
            {
                int hashCode = (int) obj.Attributes;
                hashCode = (hashCode * 397) ^ GetHashCode(obj.VariableTypes);
                return hashCode;
            }
        }

        /// <inheritdoc />
        public virtual bool Equals(GenericInstanceMethodSignature? x, GenericInstanceMethodSignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return x.Attributes == y.Attributes
                   && Equals(x.TypeArguments, y.TypeArguments);
        }

        /// <inheritdoc />
        public virtual int GetHashCode(GenericInstanceMethodSignature obj)
        {
            unchecked
            {
                int hashCode = (int) obj.Attributes;
                hashCode = (hashCode * 397) ^ GetHashCode(obj.TypeArguments);
                return hashCode;
            }
        }

        /// <inheritdoc />
        public virtual bool Equals(PropertySignature? x, PropertySignature? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return x.Attributes == y.Attributes
                   && Equals(x.ReturnType, y.ReturnType)
                   && Equals(x.ParameterTypes, y.ParameterTypes);
        }

        /// <inheritdoc />
        public virtual int GetHashCode(PropertySignature obj)
        {
            unchecked
            {
                int hashCode = (int) obj.Attributes;
                hashCode = (hashCode * 397) ^ GetHashCode(obj.ReturnType);
                hashCode = (hashCode * 397) ^ GetHashCode(obj.ParameterTypes);
                return hashCode;
            }
        }
    }
}
