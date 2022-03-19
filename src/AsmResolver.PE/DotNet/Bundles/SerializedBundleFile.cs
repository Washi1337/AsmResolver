using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Bundles
{
    public class SerializedBundleFile : BundleFile
    {
        public SerializedBundleFile(ref BinaryStreamReader reader, uint bundleVersionFormat)
        {
            ulong offset = reader.ReadUInt64();
            ulong size = reader.ReadUInt64();

            if (bundleVersionFormat >= 6)
            {
                ulong compressedSize = reader.ReadUInt64();
                IsCompressed = compressedSize != 0;
            }

            Type = (BundleFileType) reader.ReadByte();
            RelativePath = reader.ReadBinaryFormatterString();
        }
    }
}
