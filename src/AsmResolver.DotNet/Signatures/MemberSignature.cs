using System;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides a base for all member signatures.
    /// </summary>
    public abstract class MemberSignature : CallingConventionSignature
    {
        /// <summary>
        /// Initializes a new member signature.
        /// </summary>
        /// <param name="attributes">The attributes of the signature.</param>
        /// <param name="memberReturnType">The type of the object this member returns or contains.</param>
        protected MemberSignature(CallingConventionAttributes attributes, TypeSignature memberReturnType) 
            : base(attributes)
        {
            MemberReturnType = memberReturnType;
        }

        /// <summary>
        /// Gets the type of the object this member returns or contains.
        /// </summary>
        protected TypeSignature MemberReturnType
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0}{1}",
                HasThis ? "instance " : string.Empty,
                MemberReturnType?.FullName ?? TypeSignature.NullTypeToString);
        }
    }
}