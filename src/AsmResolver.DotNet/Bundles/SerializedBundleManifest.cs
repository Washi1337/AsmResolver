using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.IO;

namespace AsmResolver.DotNet.Bundles
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="BundleManifest"/>  that is read from an existing file.
    /// </summary>
    public class SerializedBundleManifest : BundleManifest
    {
        private readonly uint _originalMajorVersion;
        private readonly BinaryStreamReader _fileEntriesReader;
        private readonly int _originalFileCount;

        /// <summary>
        /// Reads a bundle manifest from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        public SerializedBundleManifest(BinaryStreamReader reader)
        {
            MajorVersion = _originalMajorVersion = reader.ReadUInt32();
            MinorVersion = reader.ReadUInt32();
            _originalFileCount = reader.ReadInt32();
            BundleID = reader.ReadBinaryFormatterString();

            if (MajorVersion >= 2)
            {
                reader.Offset += 4 * sizeof(ulong);
                Flags = (BundleManifestFlags) reader.ReadUInt64();
            }

            _fileEntriesReader = reader;
        }

        /// <inheritdoc />
        protected override IList<BundleFile> GetFiles()
        {
            var reader = _fileEntriesReader;
            var result = new OwnedCollection<BundleManifest, BundleFile>(this);

            for (int i = 0; i < _originalFileCount; i++)
                result.Add(new SerializedBundleFile(ref reader, _originalMajorVersion));

            return result;
        }
    }
}
