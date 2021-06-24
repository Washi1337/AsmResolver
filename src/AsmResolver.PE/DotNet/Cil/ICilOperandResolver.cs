using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides members for resolving raw operands of decoded CIL instructions to higher level representations.
    /// </summary>
    public interface ICilOperandResolver
    {
        /// <summary>
        /// Resolves a member operand.
        /// </summary>
        /// <param name="token">The metadata token of the referenced member.</param>
        /// <returns>The member, or <c>null</c> if the metadata token could not be resolved to a member.</returns>
        object? ResolveMember(MetadataToken token);

        /// <summary>
        /// Resolves a string operand.
        /// </summary>
        /// <param name="token">The metadata token of the referenced string/</param>
        /// <returns>The string, or <c>null</c> if the metadata token could not be resolved to a string.</returns>
        object? ResolveString(MetadataToken token);

        /// <summary>
        /// Resolves a local variable operand.
        /// </summary>
        /// <param name="index">The index of the local variable to resolve.</param>
        /// <returns>The local variable, or <c>null</c> if the index could not be resolved to a local variable.</returns>
        object? ResolveLocalVariable(int index);

        /// <summary>
        /// Resolves a parameter operand.
        /// </summary>
        /// <param name="index">The index of the parameter to resolve.</param>
        /// <returns>The parameter, or <c>null</c> if the index could not be resolved to a local variable.</returns>
        object? ResolveParameter(int index);
    }
}
