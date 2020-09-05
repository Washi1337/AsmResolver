namespace AsmResolver.PE.Win32Resources.Icon
{
    /// <summary>
    /// Represents a single icon resource entry.
    /// </summary>
    public class IconEntry : SegmentBase
    {
        /// <summary>
        /// The raw bytes of the icon.
        /// </summary>
        public byte[] RawIcon
        {
            get;
            set;
        }

        /// <summary>
        /// Reads an icon resource entry from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The parsed icon resource entry.</returns>
        public static IconEntry FromReader(IBinaryStreamReader reader)
        {
            var result = new IconEntry
            {
                Offset = reader.Offset,
                Rva = reader.Rva,
                RawIcon = new byte[reader.Length]
            };
            reader.ReadBytes(result.RawIcon, 0, (int)reader.Length);

            return result;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => (uint)RawIcon.Length;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteBytes(RawIcon, 0, RawIcon.Length);
        }
    }
}
