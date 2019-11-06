using System.Collections.Generic;
using System.Text;

namespace AsmResolver.PE.Imports.Builder
{
    /// <summary>
    /// Provides a mechanism for building up a table consisting of hint-name pairs.
    /// </summary>
    public class HintNameTableBuffer : SegmentBase
    {
        private readonly IList<IModuleImportEntry> _modules = new List<IModuleImportEntry>();
        private readonly IDictionary<IModuleImportEntry, uint> _moduleNameOffsets = new Dictionary<IModuleImportEntry, uint>();
        private readonly IDictionary<MemberImportEntry, uint> _hintNameOffsets = new Dictionary<MemberImportEntry, uint>();
        private uint _length;

        /// <summary>
        /// Adds the name of the module and the names of all named entries to the hint-name table.
        /// </summary>
        /// <param name="module">The module to add.</param>
        public void AddModule(IModuleImportEntry module)
        {
            _modules.Add(module);

            foreach (var entry in module.Members)
            {
                if (entry.IsImportByName)
                {
                    _hintNameOffsets[entry] = _length;
                    _length += (uint) (sizeof(ushort) + Encoding.ASCII.GetByteCount(entry.Name) + 1);

                    if (_length % 2 != 0)
                        _length++;
                }
            }

            _moduleNameOffsets[module] = _length;
            _length += (uint) Encoding.ASCII.GetByteCount(module.Name) + 1;
        }

        /// <summary>
        /// Gets the virtual address to the beginning of the name of a module.
        /// </summary>
        /// <param name="module">The module to obtain the name RVA for.</param>
        /// <returns>The virtual address.</returns>
        /// <remarks>
        /// This method should only be used after the hint-name table has been relocated to the right location in the
        /// PE file.
        /// </remarks>
        public uint GetModuleNameRva(IModuleImportEntry module) => Rva + _moduleNameOffsets[module];
        
        /// <summary>
        /// Gets the virtual address to the beginning of the hint-name pair associated to an imported member.
        /// </summary>
        /// <param name="member">The member to obtain the hint-name RVA for.</param>
        /// <returns>The virtual address.</returns>
        /// <remarks>
        /// This method should only be used after the hint-name table has been relocated to the right location in the
        /// PE file.
        /// </remarks>
        public uint GetHintNameRva(MemberImportEntry member) => Rva + _hintNameOffsets[member];

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _length;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            foreach (var module in _modules)
            {
                foreach (var member in module.Members)
                {
                    if (member.IsImportByName) 
                        WriteHintName(writer, member.Hint, member.Name);
                }

                WriteModuleName(writer, module);
            }
        }

        private static void WriteHintName(IBinaryStreamWriter writer, ushort hint, string name)
        {
            writer.WriteUInt16(hint);
            writer.WriteAsciiString(name);
            writer.WriteByte(0);
            writer.Align(2);
        }

        private static void WriteModuleName(IBinaryStreamWriter writer, IModuleImportEntry module)
        {
            writer.WriteAsciiString(module.Name);
            writer.WriteByte(0);
        }
        
    }
}