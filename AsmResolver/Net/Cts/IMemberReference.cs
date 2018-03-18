using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
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