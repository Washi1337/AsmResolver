using AsmResolver.IO;

namespace AsmResolver.DotNet.Bundles
{
    public class SerializedBundleFile : BundleFile
    {
        private readonly BinaryStreamReader _contentsReader;

        public SerializedBundleFile(ref BinaryStreamReader reader, uint bundleVersionFormat)
        {
            ulong offset = reader.ReadUInt64();
            ulong size = reader.ReadUInt64();

            if (bundleVersionFormat >= 6)
            {
                ulong compressedSize = reader.ReadUInt64();
                if (compressedSize != 0)
                {
                    size = compressedSize;
                    IsCompressed = true;
                }
            }

            Type = (BundleFileType) reader.ReadByte();
            RelativePath = reader.ReadBinaryFormatterString();

            _contentsReader = reader.ForkAbsolute(offset, (uint) size);
        }

        protected override ISegment GetContents() => _contentsReader.ReadSegment(_contentsReader.Length);
    }
}
