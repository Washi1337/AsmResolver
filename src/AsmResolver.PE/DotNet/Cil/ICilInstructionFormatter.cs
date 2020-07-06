namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides members for formatting a CIL instruction to a human readable string.
    /// </summary>
    public interface ICilInstructionFormatter
    {
        /// <summary>
        /// Formats a single instruction to a string.
        /// </summary>
        /// <param name="instruction">The instruction to format.</param>
        /// <returns>The string representing the instruction.</returns>
        string FormatInstruction(CilInstruction instruction);
    }
}