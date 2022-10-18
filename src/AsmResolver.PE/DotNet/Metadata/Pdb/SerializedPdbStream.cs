using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Pdb
{
    /// <summary>
    /// Provides an implementation of a PDB stream that obtains GUIDs from a readable segment in a file.
    /// </summary>
    public class SerializedPdbStream : PdbStream
    {
        private readonly BinaryStreamReader _reader;

        /// <summary>
        /// Creates a new PDB stream with the provided byte array as the raw contents of the stream.
        /// </summary>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedPdbStream(byte[] rawData)
            : this(DefaultName, new BinaryStreamReader(rawData))
        {
        }

        /// <summary>
        /// Creates a new PDB stream with the provided byte array as the raw contents of the stream.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedPdbStream(string name, byte[] rawData)
            : this(name, new BinaryStreamReader(rawData))
        {
        }

        /// <summary>
        /// Creates a new PDB stream with the provided file segment reader as the raw contents of the stream.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="reader">The raw contents of the stream.</param>
        public SerializedPdbStream(string name, in BinaryStreamReader reader)
        {
            _reader = reader;

            Name = name;
            Offset = reader.Offset;
            Rva = reader.Rva;

            var headerReader = reader.Fork();

            headerReader.ReadBytes(Id, 0, Id.Length);
            EntryPoint = headerReader.ReadUInt32();

            ulong mask = headerReader.ReadUInt64();
            for (int i = 0; i < 64; i++)
            {
                if (((mask >> i) & 1) != 0)
                    TypeSystemRowCounts[i] = headerReader.ReadUInt32();
            }
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override BinaryStreamReader CreateReader() => _reader.Fork();
    }
}
