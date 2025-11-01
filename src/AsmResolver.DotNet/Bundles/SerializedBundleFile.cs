using AsmResolver.IO;

namespace AsmResolver.DotNet.Bundles
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="BundleFile"/>  that is read from an existing file.
    /// </summary>
    public class SerializedBundleFile : BundleFile
    {
        private readonly BinaryStreamReader _contentsReader;

        /// <summary>
        /// Reads a bundle file entry from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="bundleVersionFormat">The file format version of the bundle.</param>
        public SerializedBundleFile(ref BinaryStreamReader reader, uint bundleVersionFormat)
            : base(string.Empty)
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

        /// <inheritdoc />
        protected override ISegment GetContents() => _contentsReader.Fork().ReadSegment(_contentsReader.Length);
    }
}
