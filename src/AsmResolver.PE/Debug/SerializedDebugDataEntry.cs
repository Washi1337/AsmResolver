using System;
using AsmResolver.IO;

namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Provides an implementation of a debug data entry that was stored in a PE file.
    /// </summary>
    public class SerializedDebugDataEntry : DebugDataEntry
    {
        private readonly PEReaderContext _context;
        private readonly DebugDataType _type;
        private readonly uint _sizeOfData;
        private readonly uint _addressOfRawData;
        private readonly uint _pointerToRawData;

        /// <summary>
        /// Reads a single debug data entry from an input stream.
        /// </summary>
        /// <param name="context">The reading context.</param>
        /// <param name="directoryReader">The input stream.</param>
        public SerializedDebugDataEntry(
            PEReaderContext context,
            ref BinaryStreamReader directoryReader)
        {
            if (!directoryReader.IsValid)
                throw new ArgumentNullException(nameof(directoryReader));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Offset = directoryReader.Offset;
            Rva = directoryReader.Rva;

            Characteristics = directoryReader.ReadUInt32();
            TimeDateStamp = directoryReader.ReadUInt32();
            MajorVersion = directoryReader.ReadUInt16();
            MinorVersion = directoryReader.ReadUInt16();
            _type = (DebugDataType) directoryReader.ReadUInt32();
            _sizeOfData = directoryReader.ReadUInt32();
            _addressOfRawData = directoryReader.ReadUInt32();
            _pointerToRawData = directoryReader.ReadUInt32();
        }

        /// <inheritdoc />
        protected override IDebugDataSegment GetContents()
        {
            if (_sizeOfData == 0)
                return null;

            var reference = _context.File.GetReferenceToRva(_addressOfRawData);
            if (reference is null || !reference.CanRead)
            {
                _context.BadImage("Debug data entry contains an invalid RVA.");
                return null;
            }

            var reader = reference.CreateReader();
            if (_sizeOfData > reader.Length)
            {
                _context.BadImage("Debug data entry contains a too large size.");
                return null;
            }

            reader.ChangeSize(_sizeOfData);
            return _context.Parameters.DebugDataReader.ReadDebugData(_context, _type, ref reader);
        }
    }
}
