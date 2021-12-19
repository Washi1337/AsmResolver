using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Code.Native;
using AsmResolver.PE.Code;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.VTableFixups;
using AsmResolver.PE.Exports;
using AsmResolver.PE.Platforms;
using AsmResolver.PE.Relocations;

namespace AsmResolver.DotNet.Builder.VTableFixups
{
    /// <summary>
    /// Provides a buffer for a VTable Fixups directory, which allows for registering method tokens to
    /// unmanaged exports in the PE file.
    /// </summary>
    public class VTableFixupsBuffer
    {
        private readonly Platform _targetPlatform;
        private readonly INativeSymbolsProvider _symbolsProvider;

        /// <summary>
        /// Creates a new buffer.
        /// </summary>
        /// <param name="targetPlatform">The platform to target.</param>
        /// <param name="symbolsProvider">The symbols provider that is responsible for registering unmanaged exports.</param>
        public VTableFixupsBuffer(Platform targetPlatform, INativeSymbolsProvider symbolsProvider)
        {
            _targetPlatform = targetPlatform;
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
            var vtableFixup = GetFixup(exportInfo.VTableType);
            vtableFixup.Tokens.Add(token);
            var vtableSymbol = new Symbol(vtableFixup.Tokens.GetReferenceToIndex(vtableFixup.Tokens.Count - 1));

            var thunkStub = _targetPlatform.CreateThunkStub(0x00400000, vtableSymbol);

            // Register exported symbol.
            var stubReference = thunkStub.Segment.ToReference();
            var newSymbol = exportInfo.IsByName
                ? new ExportedSymbol(stubReference, exportInfo.Name)
                : new ExportedSymbol(stubReference);
            _symbolsProvider.RegisterExportedSymbol(newSymbol, exportInfo.Ordinal);

            // Register relocations.
            for (int i = 0; i < thunkStub.Relocations.Count; i++)
                _symbolsProvider.RegisterBaseRelocation(thunkStub.Relocations[i]);
        }
    }
}
