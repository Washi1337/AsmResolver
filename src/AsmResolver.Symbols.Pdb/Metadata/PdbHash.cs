namespace AsmResolver.Symbols.Pdb.Metadata;

/// <summary>
/// Provides methods for computing hash codes for a PDB hash table.
/// </summary>
public static class PdbHash
{
    /// <summary>
    /// Computes the V1 hash code for a UTF-8 string.
    /// </summary>
    /// <param name="value">The string to compute the hash for.</param>
    /// <returns>The hash code.</returns>
    /// <remarks>
    /// See PDB/include/misc.h for reference implementation.
    /// </remarks>
    public static unsafe uint ComputeV1(Utf8String value)
    {
        uint result = 0;

        uint count = (uint) value.ByteCount;

        fixed (byte* ptr = value.GetBytesUnsafe())
        {
            byte* p = ptr;

            while (count >= 4)
            {
                result ^= *(uint*) p;
                count -= 4;
                p += 4;
            }

            if (count >= 2)
            {
                result ^= *(ushort*) p;
                count -= 2;
                p += 2;
            }

            if (count == 1)
                result ^= *p;
        }

        result |= 0x20202020;
        result ^= result >> 11;

        return result ^ (result >> 16);
    }
}
