using System.Collections.Generic;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Represents the organization of data in a file-version resource. It contains version information not dependent
    /// on a particular language and code page combination.
    /// </summary>
    public class VarFileInfo : VersionTableEntry
    {
        /// <summary>
        /// The name of the VarFileInfo entry.
        /// </summary>
        public const string VarFileInfoKey = "VarFileInfo";

        /// <summary>
        /// Reads a single VarFileInfo structure from the provided input stream.
        /// </summary>
        /// <param name="startOffset">The offset of the consumed header.</param>
        /// <param name="header">The header.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The read structure.</returns>
        /// <remarks>
        /// This function assumes the provided header was already consumed.
        /// </remarks>
        public static VarFileInfo FromReader(ulong startOffset, VersionTableEntryHeader header, IBinaryStreamReader reader)
        {
            var result = new VarFileInfo();

            while (reader.Offset - startOffset < header.Length)
                result.Tables.Add(VarTable.FromReader(reader));
            
            return result;
        }

        /// <inheritdoc />
        public override string Key => VarFileInfoKey;

        /// <inheritdoc />
        protected override VersionTableValueType ValueType => VersionTableValueType.String;
            
        /// <summary>
        /// Gets a collection of tables stored in this VarFileInfo structure, typically containing a list of languages
        /// that the application or DLL supports.
        /// </summary>
        public IList<VarTable> Tables
        {
            get;
        } = new List<VarTable>();

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