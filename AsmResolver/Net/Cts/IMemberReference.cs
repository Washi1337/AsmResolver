using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a single reference to a member defined in a .NET assembly.
    /// </summary>
    public interface IMemberReference : IFullNameProvider, IHasCustomAttribute
    {
        /// <summary>
        /// Gets or sets the name of the member.
        /// </summary>
        new string Name
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets the type that declares the member (if available).
        /// </summary>
        ITypeDefOrRef DeclaringType
        {
            get;
        }
    }

    /// <summary>
    /// Represents a single reference to a member that contains a blob signature.
    /// </summary>
    public interface ICallableMemberReference : IMemberReference, IResolvable
    {
        /// <summary>
        /// Gets the signature associated to the reference.
        /// </summary>
        CallingConventionSignature Signature
        {
            get;
        }
    }
}