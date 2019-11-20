using System;

namespace AsmResolver.DotNet.Blob
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
            MemberReturnType = memberReturnType ?? throw new ArgumentNullException(nameof(memberReturnType));
        }

        /// <summary>
        /// Gets the type of the object this member returns or contains.
        /// </summary>
        protected TypeSignature MemberReturnType
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString() => (HasThis ? "instance " : string.Empty) + MemberReturnType.FullName;
    }
}