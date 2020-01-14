using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides members for describing a method in a managed assembly. 
    /// </summary>
    public interface IMethodDescriptor : IMemberDescriptor
    {
        /// <summary>
        /// Gets the signature of the method.
        /// </summary>
        MethodSignature Signature
        {
            get;
        }
    }
}