using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Code.Native;
using AsmResolver.PE.Code;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.VTableFixups;
using AsmResolver.PE.Exports;
using AsmResolver.PE.Relocations;

namespace AsmResolver.DotNet.Builder.VTableFixups
{
    /// <summary>
    /// Provides a buffer for a VTable Fixups directory, which allows for registering method tokens to
    /// unmanaged exports in the PE file.
    /// </summary>
    public class VTableFixupsBuffer
    {
        private readonly INativeSymbolsProvider _symbolsProvider;

        /// <summary>
        /// Creates a new buffer.
        /// </summary>
        /// <param name="symbolsProvider">The symbols provider that is responsible for registering unmanaged exports.</param>
        public VTableFixupsBuffer(INativeSymbolsProvider symbolsProvider)
        {
            _symbolsProvider = symbolsProvider;
        }

        /// <summary>
        /// The constructed VTable Fixups directory.
        /// </summary>
        public VTableFixupsDirectory Directory
        {
            get;
        } = new();

        private VTableFixup GetFixup(VTableType type)
        {
            var fixup = Directory.FirstOrDefault(f => f.Tokens.Type == type);
            if (fixup is null)
            {
                fixup = new VTableFixup(type);
                Directory.Add(fixup);
            }

            return fixup;
        }

        /// <summary>
        /// Assigns a token to an unmanaged export.
        /// </summary>
        /// <param name="exportInfo">The export to map.</param>
        /// <param name="token">The token to assign to the export.</param>
        public void MapTokenToExport(UnmanagedExportInfo exportInfo, MetadataToken token)
        {
            var stub = new CodeSegment(0x00400000, new byte[]
            {
                0xFF, 0x25, 0x00, 0x00, 0x00, 0x00
            });

            var vtableFixup = GetFixup(exportInfo.VTableType);
            vtableFixup.Tokens.Add(token);
            var tokenReference = vtableFixup.Tokens.GetReferenceToIndex(vtableFixup.Tokens.Count - 1);

            var addressFixup = new AddressFixup(2,
                AddressFixupType.Absolute32BitAddress, new Symbol(tokenReference));
            stub.AddressFixups.Add(addressFixup);

            var stubReference = stub.ToReference();

            var symbol = exportInfo.IsByName
                ? new ExportedSymbol(stubReference, exportInfo.Name)
                : new ExportedSymbol(stubReference);

            _symbolsProvider.RegisterExportedSymbol(symbol);
            if (addressFixup.Type == AddressFixupType.Absolute32BitAddress)
            {
                var relocation = new BaseRelocation(
                    RelocationType.HighLow,
                    stub.ToReference((int) addressFixup.Offset));
                _symbolsProvider.RegisterBaseRelocation(relocation);
            }
        }
    }
}
