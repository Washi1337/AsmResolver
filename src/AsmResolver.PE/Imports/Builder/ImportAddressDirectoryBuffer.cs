namespace AsmResolver.PE.Imports.Builder
{
    /// <summary>
    /// Provides a mechanism for building an import address directory in a PE file.
    /// </summary>
    public class ImportAddressDirectoryBuffer : ImportDirectoryBufferBase
    {
        /// <summary>
        /// Creates a new import address directory buffer, using the provided hint-name table to obtain addresses to names
        /// of an imported member.
        /// </summary>
        /// <param name="hintNameTable">The hint-name table that is used to reference names of modules or members.</param>
        /// <param name="is32Bit">Indicates the import directory should use 32-bit addresses or 64-bit addresses.</param>
        public ImportAddressDirectoryBuffer(HintNameTableBuffer hintNameTable, bool is32Bit)
            : base(hintNameTable, is32Bit)
        {
        }

        /// <inheritdoc />
        public override void UpdateOffsets(ulong newOffset, uint newRva)
        {
            base.UpdateOffsets(newOffset, newRva);
            
            foreach (var module in Modules)
            {
                var thunkTable = GetModuleThunkTable(module);
                uint size = thunkTable.GetPhysicalSize();
                thunkTable.UpdateOffsets(newOffset, newRva);
                newOffset += size;
                newRva += size;
            }
        }
        
    }
}