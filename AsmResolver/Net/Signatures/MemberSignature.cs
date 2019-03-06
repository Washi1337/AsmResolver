namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Provides a base for all member signatures.
    /// </summary>
    public abstract class MemberSignature : CallingConventionSignature, IHasTypeSignature
    {
        /// <summary>
        /// The type of the object this member returns or contains.
        /// </summary>
        protected abstract TypeSignature TypeSignature
        {
            get;
        }

        /// <inheritdoc />
        TypeSignature IHasTypeSignature.TypeSignature => TypeSignature;

        public override string ToString()
        {
            return (HasThis ? "instance " : string.Empty) + TypeSignature.FullName;
        }
    }
}
