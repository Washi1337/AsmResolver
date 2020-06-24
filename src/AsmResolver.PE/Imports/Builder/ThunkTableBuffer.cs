using System;
using System.Collections.Generic;

namespace AsmResolver.PE.Imports.Builder
{
    /// <summary>
    /// Provides a mechanism for building up a thunk table for an import directory.
    /// </summary>
    public class ThunkTableBuffer : SegmentBase
    {
        private readonly IList<ImportedSymbol> _members = new List<ImportedSymbol>();
        private readonly IDictionary<ImportedSymbol, uint> _memberOffsets = new Dictionary<ImportedSymbol, uint>();
        private readonly HintNameTableBuffer _hintNameTable;

        private uint _length;

        /// <summary>
        /// Creates a new thunk table buffer, that uses the provided hint-name table to reference names.
        /// </summary>
        /// <param name="hintNameTable">The hint-name table containing the names of each imported member</param>
        /// <param name="is32Bit">Indicates whether the thunk-table should use 32-bit addresses or 64-bit addresses.</param>
        public ThunkTableBuffer(HintNameTableBuffer hintNameTable, bool is32Bit)
        {
            _hintNameTable = hintNameTable ?? throw new ArgumentNullException(nameof(hintNameTable));
            Is32Bit = is32Bit;
            _length = ThunkSize;
        }
        
        /// <summary>
        /// Gets a value indicating whether the thunk-table should use 32-bit addresses or 64-bit addresses.
        /// </summary>
        public bool Is32Bit
        {
            get;
        }

        /// <summary>
        /// Gets the size of a single entry in the thunk table.
        /// </summary>
        public uint ThunkSize => (uint) (Is32Bit ? sizeof(uint) : sizeof(ulong));

        /// <summary>
        /// Creates a thunk for the specified member, and adds it to the table.
        /// </summary>
        /// <param name="entry">The member to add.</param>
        public void AddMember(ImportedSymbol entry)
        {
            _memberOffsets.Add(entry, _length - ThunkSize);
            _members.Add(entry);
            _length += ThunkSize;
        }

        /// <summary>
        /// Obtains the relative virtual address of the thunk associated to an imported member in the thunk table.
        /// </summary>
        /// <param name="member">The member to get the thunk entry RVA for.</param>
        /// <returns>The virtual address</returns>
        /// <remarks>
        /// This method should only be used after the thunk table has been relocated to the right location in the
        /// PE file.
        /// </remarks>
        public uint GetMemberThunkRva(ImportedSymbol member) => Rva + _memberOffsets[member];

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _length;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            foreach (var member in _members)
            {
                ulong value = member.IsImportByName
                    ? _hintNameTable.GetHintNameRva(member)
                    : member.Ordinal | (1UL << (Is32Bit ? 31 : 63));

                if (Is32Bit)
                    writer.WriteUInt32((uint) value);
                else
                    writer.WriteUInt64(value);
            }

            if (Is32Bit)
                writer.WriteUInt32(0);
            else
                writer.WriteUInt16(0);
        }
        
    }
}