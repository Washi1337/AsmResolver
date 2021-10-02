using AsmResolver.DotNet.Code;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Provides members for reading a method body.
    /// </summary>
    public interface IMethodBodyReader
    {
        /// <summary>
        /// Reads a method body
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="owner">The owner of the method body.</param>
        /// <param name="row">The metadata row of the owner method.</param>
        /// <returns>The method, or <c>null</c> if none was provided.</returns>
        /// <remarks>
        /// Implementations should never access <see cref="MethodDefinition.MethodBody"/> or
        /// <see cref="MethodDefinition.CilMethodBody"/>, as this might result in an infinite recursive loop.
        /// </remarks>
        MethodBody? ReadMethodBody(ModuleReaderContext context, MethodDefinition owner, in MethodDefinitionRow row);
    }
}
