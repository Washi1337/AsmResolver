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
        
        /// <summary>
        /// Resolves the reference to a method definition. 
        /// </summary>
        /// <returns>The resolved method definition, or <c>null</c> if the method could not be resolved.</returns>
        /// <remarks>
        /// This method can only be invoked if the reference was added to a module. 
        /// </remarks>
        MethodDefinition Resolve();
    }
}