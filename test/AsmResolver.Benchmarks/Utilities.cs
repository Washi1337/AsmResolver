using System.IO;
using System.IO.Compression;

namespace AsmResolver.Benchmarks
{
    internal static class Utilities
    {
        public static byte[] DecompressDeflate(byte[] compressedData)
        {
            using var decompressed = new MemoryStream();
            using (var compressed = new MemoryStream(compressedData))
            {
                using var deflate = new DeflateStream(compressed, CompressionMode.Decompress);
                deflate.CopyTo(decompressed);
            }

            return decompressed.ToArray();
        }
    }
}
