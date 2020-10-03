namespace AsmResolver.PE.Imports.Builder
{
    /// <summary>
    /// Provides a mechanism for building an import lookup directory in a PE file.
    /// </summary>
    public class ImportDirectoryBuffer : ImportDirectoryBufferBase
    {
        private uint _entriesLength;

        /// <summary>
        /// Creates a new import lookup directory.
        /// </summary>
        /// <param name="is32Bit">Indicates the import directory should use 32-bit addresses or 64-bit addresses.</param>
        public ImportDirectoryBuffer(bool is32Bit)
            : base(new HintNameTableBuffer(), is32Bit)
        {
            ImportAddressDirectory = new ImportAddressDirectoryBuffer(HintNameTable, is32Bit);
        }

        /// <summary>
        /// Gets the import address directory that is linked to this lookup directory.
        /// </summary>
        public ImportAddressDirectoryBuffer ImportAddressDirectory
        {
            get;
        }

        /// <inheritdoc />
        public override void AddModule(IImportedModule module)
        {
            if (_entriesLength == 0)
                _entriesLength = SerializedImportedModule.ModuleImportSize;
            
            _entriesLength += SerializedImportedModule.ModuleImportSize;
            
            ImportAddressDirectory.AddModule(module);
            base.AddModule(module);
            
            HintNameTable.AddModule(module);
        }

        /// <inheritdoc />
        public override void UpdateOffsets(ulong newOffset, uint newRva)
        {
            base.UpdateOffsets(newOffset, newRva);

            newOffset += _entriesLength;
            newRva += _entriesLength;
            
            foreach (var module in Modules)
            {
                var thunkTable = GetModuleThunkTable(module);
                uint size = thunkTable.GetPhysicalSize();
                thunkTable.UpdateOffsets(newOffset, newRva);
                newOffset += size;
                newRva += size;
            }

            HintNameTable.UpdateOffsets(newOffset, newRva);
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _entriesLength + base.GetPhysicalSize() + HintNameTable.GetPhysicalSize();

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            WriteModuleImportEntries(writer);
            base.Write(writer);
            HintNameTable.Write(writer);
        }

        private void WriteModuleImportEntries(IBinaryStreamWriter writer)
        {
            foreach (var module in Modules)
                WriteModuleImportEntry(writer, module);
            WriteModuleImportEntry(writer, 0, 0, 0, 0, 0);
        }

        private void WriteModuleImportEntry(IBinaryStreamWriter writer, IImportedModule module)
        {
            WriteModuleImportEntry(writer, 
                GetModuleThunkTable(module).Rva, 
                module.TimeDateStamp, 
                module.ForwarderChain, 
                HintNameTable.GetModuleNameRva(module), 
                ImportAddressDirectory.GetModuleThunkTable(module).Rva);
        }

        private static void WriteModuleImportEntry(IBinaryStreamWriter writer, uint oft, uint timeDateStamp,
            uint forwarderChain, uint moduleNameRva, uint ft)
        {
            writer.WriteUInt32(oft);
            writer.WriteUInt32(timeDateStamp);
            writer.WriteUInt32(forwarderChain);
            writer.WriteUInt32(moduleNameRva);
            writer.WriteUInt32(ft);
        }
        
    }
}