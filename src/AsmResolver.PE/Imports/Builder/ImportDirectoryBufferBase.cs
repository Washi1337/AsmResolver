using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.IO;

namespace AsmResolver.PE.Imports.Builder
{
    /// <summary>
    /// Provides a base for the import lookup and address directory buffers.
    /// </summary>
    public abstract class ImportDirectoryBufferBase : SegmentBase, IImportAddressProvider
    {
        private readonly Dictionary<ImportedModule, ThunkTableBuffer> _lookupTables = new();
        private uint _lookupTablesLength;

        /// <summary>
        /// Initializes the new import directory buffer with a hint-name table.
        /// </summary>
        /// <param name="hintNameTable">The hint-name table that is used to reference names of modules or members.</param>
        /// <param name="is32Bit">Indicates the import directory should use 32-bit addresses or 64-bit addresses.</param>
        protected ImportDirectoryBufferBase(HintNameTableBuffer hintNameTable, bool is32Bit)
        {
            HintNameTable = hintNameTable ?? throw new ArgumentNullException(nameof(hintNameTable));
            Is32Bit = is32Bit;
        }

        /// <summary>
        /// Gets a value indicating the import directory should use 32-bit addresses or 64-bit addresses.
        /// </summary>
        public bool Is32Bit
        {
            get;
        }

        /// <summary>
        /// Gets the number of modules that were added to the import directory.
        /// </summary>
        public int Count => Modules.Count;

        /// <summary>
        /// Gets an ordered list of modules that were added to the buffer.
        /// </summary>
        protected IList<ImportedModule> Modules
        {
            get;
        } = new List<ImportedModule>();

        /// <summary>
        /// Gets the hint-name table that is used to reference names of modules or members.
        /// </summary>
        public HintNameTableBuffer HintNameTable
        {
            get;
        }

        /// <summary>
        /// Creates a thunk table for a module and its imported members, and adds it to the buffer.
        /// </summary>
        /// <param name="module">The module to add.</param>
        public virtual void AddModule(ImportedModule module)
        {
            Modules.Add(module);
            AddLookupTable(module);
        }

        /// <summary>
        /// Obtains the thunk table of a module.
        /// </summary>
        /// <param name="module">The module to get the associated thunk table for.</param>
        /// <returns>The thunk table.</returns>
        public ThunkTableBuffer GetModuleThunkTable(ImportedModule module) => _lookupTables[module];

        /// <inheritdoc />
        public uint GetThunkRva(string moduleName, string memberName)
        {
            var module = Modules.FirstOrDefault(x => x.Name == moduleName);
            if (module == null)
                throw new ArgumentException($"Module {moduleName} is not imported.", nameof(moduleName));

            var member = module.Symbols.FirstOrDefault(x => x.Name == memberName);
            if (member == null)
                throw new ArgumentException($"Member {moduleName}!{memberName} is not imported.", nameof(memberName));

            return GetModuleThunkTable(module).GetMemberThunkRva(member);
        }

        private void AddLookupTable(ImportedModule module)
        {
            var lookupTable = CreateThunkTable();
            foreach (var member in module.Symbols)
                lookupTable.AddMember(member);
            _lookupTables.Add(module, lookupTable);
            _lookupTablesLength += lookupTable.GetPhysicalSize();
        }

        /// <summary>
        /// Creates a new thunk table buffer.
        /// </summary>
        /// <returns>The buffer.</returns>
        protected virtual ThunkTableBuffer CreateThunkTable() =>
            new ThunkTableBuffer(HintNameTable, Is32Bit, false);

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _lookupTablesLength;

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            foreach (var module in Modules)
                _lookupTables[module].Write(writer);
        }
    }
}
