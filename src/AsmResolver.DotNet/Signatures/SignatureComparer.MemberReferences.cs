using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AsmResolver.DotNet.Signatures
{
    public partial class SignatureComparer :
        IEqualityComparer<MemberReference>,
        IEqualityComparer<IMethodDescriptor>,
        IEqualityComparer<IFieldDescriptor>,
        IEqualityComparer<MethodSpecification>
    {
        /// <inheritdoc />
        public bool Equals(MemberReference x, MemberReference y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;
        
            if (x.IsMethod)
                return Equals((IMethodDescriptor) x, y);
            if (y.IsField)
                return Equals((IFieldDescriptor) x, y);
            return false;
        }
        
        /// <inheritdoc />
        public int GetHashCode(MemberReference obj)
        {
            if (obj.IsMethod)
                return GetHashCode((IMethodDescriptor) obj);
            if (obj.IsField)
                return GetHashCode((IFieldDescriptor) obj);
            throw new ArgumentOutOfRangeException(nameof(obj));
        }
        
        /// <inheritdoc />
        public bool Equals(IMethodDescriptor x, IMethodDescriptor y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            if (x is MethodSpecification specification)
                return Equals(specification, y as MethodSpecification);

            return x.Name == y.Name
                   && Equals(x.DeclaringType, y.DeclaringType)
                   && Equals(x.Signature, y.Signature);
        }

        /// <inheritdoc />
        public int GetHashCode(IMethodDescriptor obj)
        {
            unchecked
            {
                int hashCode = obj.Name == null ? 0 : obj.Name.GetHashCode();
                hashCode = (hashCode * 397) ^ GetHashCode(obj.DeclaringType);
                hashCode = (hashCode * 397) ^ GetHashCode(obj.Signature);
                return hashCode;
            }
        }

        /// <inheritdoc />
        public bool Equals(IFieldDescriptor x, IFieldDescriptor y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.Name == y.Name
                   && Equals(x.DeclaringType, y.DeclaringType)
                   && Equals(x.Signature, y.Signature);
        }

        /// <inheritdoc />
        public int GetHashCode(IFieldDescriptor obj)
        {
            unchecked
            {
                int hashCode = obj.Name == null ? 0 : obj.Name.GetHashCode();
                hashCode = (hashCode * 397) ^ GetHashCode(obj.DeclaringType);
                hashCode = (hashCode * 397) ^ GetHashCode(obj.Signature);
                return hashCode;
            }
        }

        /// <inheritdoc />
        public bool Equals(MethodSpecification x, MethodSpecification y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return Equals(x.Method, y.Method)
                   && Equals(x.Signature, y.Signature);
        }

        /// <inheritdoc />
        public int GetHashCode(MethodSpecification obj)
        {
            unchecked
            {
                int hashCode = obj.Method == null ? 0 : obj.Method.GetHashCode();
                hashCode = (hashCode * 397) ^ GetHashCode(obj.Signature);
                return hashCode;
            }
        }
    }
}