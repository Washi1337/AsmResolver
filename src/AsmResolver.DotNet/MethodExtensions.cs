using System;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides extensions for objects that represent methods in metadata.
    /// </summary>
    public static class MethodExtensions
    {
        /// <summary>
        /// Instantiates a generic method with the provided type arguments.
        /// </summary>
        /// <param name="self">The method to instantiate.</param>
        /// <param name="arguments">The type arguments to use.</param>
        /// <returns>The instantiated method.</returns>
        /// <exception cref="ArgumentException">Occurs when <paramref name="arguments"/> has an incorrect number of elements.</exception>
        public static MethodSpecification MakeGenericInstanceMethod(this IMethodDefOrRef self, params TypeSignature[] arguments)
        {
            if (self.Signature is null)
                throw new ArgumentException($"Method does not have a signature.");

            if (self.Signature.GenericParameterCount != arguments.Length)
                throw new ArgumentException($"Expected {self.Signature.GenericParameterCount} type arguments but {arguments.Length} were given.");

            return new MethodSpecification(self, new GenericInstanceMethodSignature(arguments));
        }
    }
}
