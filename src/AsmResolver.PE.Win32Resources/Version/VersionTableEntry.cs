using AsmResolver.IO;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Provides a base for all version tables stored in a native version resource file.
    /// </summary>
    public abstract class VersionTableEntry : SegmentBase
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        public abstract string Key
        {
            get;
        }

        /// <summary>
        /// Gets the value type of the table.
        /// </summary>
        protected abstract VersionTableValueType ValueType
        {
            get;
        }

        /// <summary>
        /// Creates a new header for the table entry.
        /// </summary>
        /// <returns></returns>
        protected VersionTableEntryHeader CreateHeader() =>
            new((ushort) GetPhysicalSize(), (ushort) GetValueLength(), ValueType, Key);

        /// <summary>
        /// Gets the length of the raw value.
        /// </summary>
        /// <returns>The raw value.</returns>
        protected abstract uint GetValueLength();

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            var header = CreateHeader();
            header.Write(writer);
            writer.Align(4);
            WriteValue(writer);
        }

        /// <summary>
        /// Writes the value field of the version structure.
        /// </summary>
        /// <param name="writer">The output stream.</param>
        protected abstract void WriteValue(BinaryStreamWriter writer);
    }
}
