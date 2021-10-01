using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides members for serializing high level representations of an operand in a CIL instruction.
    /// </summary>
    public interface ICilOperandBuilder
    {
        /// <summary>
        /// Transforms the provided variable operand into its variable index.
        /// </summary>
        /// <param name="operand">The variable operand.</param>
        /// <returns>The variable index.</returns>
        int GetVariableIndex(object? operand);

        /// <summary>
        /// Transforms the provided argument operand into its argument index.
        /// </summary>
        /// <param name="operand">The argument operand.</param>
        /// <returns>The argument index.</returns>
        int GetArgumentIndex(object? operand);

        /// <summary>
        /// Transforms the provided string operand into a string token.
        /// </summary>
        /// <param name="operand">The string operand.</param>
        /// <returns>The string token.</returns>
        uint GetStringToken(object? operand);

        /// <summary>
        /// Transforms the provided member into a metadata token that references the member.
        /// </summary>
        /// <param name="operand">The member.</param>
        /// <returns>The metadata token.</returns>
        MetadataToken GetMemberToken(object? operand);
    }
}
