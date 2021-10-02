using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Provides extensions to collections of <see cref="CilInstruction"/>.
    /// </summary>
    public static class CilInstructionCollectionExtensions
    {
        /// <summary>
        /// Searches for an instruction with the given offset.
        /// </summary>
        /// <param name="self">The list of instructions.</param>
        /// <param name="offset">The offset of the instruction to find.</param>
        /// <returns>The index the instruction is located at, or -1 if an instruction with the provided offset could not
        /// be found.</returns>
        /// <remarks>Requires the offsets of the instructions pre-calculated.</remarks>
        public static int GetIndexByOffset(this IList<CilInstruction> self, int offset)
        {
            int left = 0;
            int right = self.Count - 1;

            while (left <= right)
            {
                int m = (left + right) / 2;
                int currentOffset = self[m].Offset;

                if (currentOffset > offset)
                    right = m - 1;
                else if (currentOffset < offset)
                    left = m + 1;
                else
                    return m;
            }

            return -1;
        }

        /// <summary>
        /// Searches for an instruction with the given offset.
        /// </summary>
        /// <param name="self">The list of instructions.</param>
        /// <param name="offset">The offset of the instruction to find.</param>
        /// <returns>The instruction with the provided offset, or null if none could be found.</returns>
        /// <remarks>Requires the offsets of the instructions pre-calculated.</remarks>
        public static CilInstruction? GetByOffset(this IList<CilInstruction> self, int offset)
        {
            int index = GetIndexByOffset(self, offset);
            return index != -1
                ? self[index]
                : null;
        }
    }
}
