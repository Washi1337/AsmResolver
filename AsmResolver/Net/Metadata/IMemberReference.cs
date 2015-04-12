using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public interface IMemberReference : IFullNameProvider, IHasCustomAttribute
    {
        ITypeDefOrRef DeclaringType
        {
            get;
        }
    }

    public interface ICallableMemberReference : IMemberReference, IResolvable
    {
        CallingConventionSignature Signature
        {
            get;
        }
    }
}