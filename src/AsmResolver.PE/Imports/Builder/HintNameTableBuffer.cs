using System.Collections.Generic;
using System.Text;

namespace AsmResolver.PE.Imports.Builder
{
    /// <summary>
    /// Provides a mechanism for building up a table consisting of hint-name pairs.
    /// </summary>
    public class HintNameTableBuffer : SegmentBase
    {
        private readonly IList<IImportedModule> _modules = new List<IImportedModule>();
        private readonly IDictionary<IImportedModule, uint> _moduleNameOffsets = new Dictionary<IImportedModule, uint>();
        private readonly IDictionary<ImportedSymbol, uint> _hintNameOffsets = new Dictionary<ImportedSymbol, uint>();
        private uint _length;

        /// <inheritdoc />
        public override void UpdateOffsets(ulong newOffset, uint newRva)
        {
            base.UpdateOffsets(newOffset, newRva);

            ulong currentOffset = newOffset;
            foreach (var module in _modules)
            {
                foreach (var entry in module.Symbols)
                {
                    if (entry.IsImportByName)
                    {
                        _hintNameOffsets[entry] = (uint) (currentOffset - newOffset);
                        currentOffset += (uint) (sizeof(ushort) + Encoding.ASCII.GetByteCount(entry.Name) + 1);
                        currentOffset = currentOffset.Align(2);
                    }
                }
                _moduleNameOffsets[module] = (uint) (currentOffset - newOffset);
                currentOffset += (uint) Encoding.ASCII.GetByteCount(module.Name) + 1;
            }

            _length = (uint) (currentOffset - newOffset);
        }

        /// <summary>
        /// Adds the name of the module and the names of all named entries to the hint-name table.
        /// </summary>
        /// <param name="module">The module to add.</param>
        public void AddModule(IImportedModule module)
        {
            _modules.Add(module);
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
        public uint GetModuleNameRva(IImportedModule module) => Rva + _moduleNameOffsets[module];
        
        /// <summary>
        /// Gets the virtual address to the beginning of the hint-name pair associated to an imported member.
        /// </summary>
        /// <param name="member">The member to obtain the hint-name RVA for.</param>
        /// <returns>The virtual address.</returns>
        /// <remarks>
        /// This method should only be used after the hint-name table has been relocated to the right location in the
        /// PE file.
        /// </remarks>
        public uint GetHintNameRva(ImportedSymbol member) => Rva + _hintNameOffsets[member];

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _length;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            foreach (var module in _modules)
            {
                foreach (var member in module.Symbols)
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

        private static void WriteModuleName(IBinaryStreamWriter writer, IImportedModule module)
        {
            writer.WriteAsciiString(module.Name);
            writer.WriteByte(0);
        }
        
    }
}