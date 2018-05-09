using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public interface IMemberReference : IFullNameProvider, IHasCustomAttribute
    {
        string Name
        {
            get;
            set;
        }
        
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