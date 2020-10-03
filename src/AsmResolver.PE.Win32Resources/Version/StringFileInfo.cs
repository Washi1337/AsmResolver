using System.Collections.Generic;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Represents the organization of data in a file-version resource. It contains version information that can be
    /// displayed for a particular language and code page.
    /// </summary>
    public class StringFileInfo : VersionTableEntry
    {
        /// <summary>
        /// The name of the StringFileInfo entry.
        /// </summary>
        public const string StringFileInfoKey = "StringFileInfo";

        /// <summary>
        /// Reads a single StringFileInfo structure from the provided input stream.
        /// </summary>
        /// <param name="startOffset">The offset of the consumed header.</param>
        /// <param name="header">The header.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The read structure.</returns>
        /// <remarks>
        /// This function assumes the provided header was already consumed.
        /// </remarks>
        public static StringFileInfo FromReader(ulong startOffset, VersionTableEntryHeader header, IBinaryStreamReader reader)
        {
            var result = new StringFileInfo();

            while (reader.Offset - startOffset < header.Length)
                result.Tables.Add(StringTable.FromReader(reader));
            
            return result;
        }

        /// <inheritdoc />
        public override string Key => StringFileInfoKey;

        /// <inheritdoc />
        protected override VersionTableValueType ValueType => VersionTableValueType.String;
            
        /// <summary>
        /// Gets a collection of tables stored in this VarFileInfo structure, typically containing a list of languages
        /// that the application or DLL supports.
        /// </summary>
        public IList<StringTable> Tables
        {
            get;
        } = new List<StringTable>();

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            uint size = VersionTableEntryHeader.GetHeaderSize(Key);
            
            for (int i = 0; i < Tables.Count; i++)
            {
                size = size.Align(4);
                size += Tables[i].GetPhysicalSize();
            }

            return size;
        }
        
        /// <inheritdoc />
        protected override uint GetValueLength() => 0;

        /// <inheritdoc />
        protected override void WriteValue(IBinaryStreamWriter writer)
        {
            for (int i = 0; i < Tables.Count; i++)
                Tables[i].Write(writer);
        }
    }
}