using AsmResolver.Collections;
using AsmResolver.IO;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides a lazy-initialized implementation of the <see cref="ImportSection"/> that is read from a file.
    /// </summary>
    public class SerializedImportSection : ImportSection
    {
        private readonly PEReaderContext _context;
        private readonly DataDirectory _section;
        private readonly byte _originalEntrySize;
        private readonly uint _signatures;

        /// <summary>
        /// Reads a single import section from the provided input stream.
        /// </summary>
        /// <param name="context">The context in which the reader is situated in.</param>
        /// <param name="reader">The input stream.</param>
        public SerializedImportSection(PEReaderContext context, ref BinaryStreamReader reader)
        {
            _context = context;
            _section = DataDirectory.FromReader(ref reader);

            Attributes = (ImportSectionAttributes) reader.ReadUInt16();
            Type = (ImportSectionType) reader.ReadByte();
            EntrySize = _originalEntrySize = reader.ReadByte();
            _signatures = reader.ReadUInt32();
            AuxiliaryData = context.File.GetReferenceToRva(reader.ReadUInt32());
        }

        /// <inheritdoc />
        protected override ReferenceTable GetSlots()
        {
            var result = base.GetSlots();

            if (!_section.IsPresentInPE)
                return result;

            if (!_context.File.TryCreateDataDirectoryReader(_section, out var slotsReader))
            {
                _context.BadImage("Invalid RVA and/or size for import section.");
                return result;
            }

            if (_originalEntrySize is not (4 or 8))
            {
                _context.BadImage($"Invalid entry size of {_originalEntrySize} for import section.");
                return result;
            }

            while (slotsReader.CanRead(_originalEntrySize))
            {
                ulong address = slotsReader.ReadNativeInt(_originalEntrySize == 4);
                var reference = address != 0
                    ? _context.File.GetReferenceToRva((uint) (address - _context.File.OptionalHeader.ImageBase))
                    : SegmentReference.Null;

                result.Add(reference);
            }

            return result;
        }

        /// <inheritdoc />
        protected override ReferenceTable GetSignatures()
        {
            var result = base.GetSignatures();

            if (_signatures == 0)
                return result;

            if (!_context.File.TryCreateReaderAtRva(_signatures, out var reader))
            {
                _context.BadImage($"Invalid signatures RVA {_signatures:X8} for import section.");
                return result;
            }

            for (int i = 0; i < _section.Size / _originalEntrySize; i++)
                result.Add(_context.File.GetReferenceToRva(reader.ReadUInt32()));

            return result;
        }
    }
}
