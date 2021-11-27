using AsmResolver.IO;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Tls
{
    /// <summary>
    /// Provides an implementation of a TLS directory that was read from an existing PE file.
    /// </summary>
    public class SerializedTlsDirectory : TlsDirectory
    {
        private readonly PEReaderContext _context;
        private readonly ulong _templateStart;
        private readonly ulong _templateEnd;
        private readonly ulong _addressOfCallbacks;

        /// <summary>
        /// Reads a single TLS directory from an input stream.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="reader">The input stream.</param>
        public SerializedTlsDirectory(PEReaderContext context, ref BinaryStreamReader reader)
        {
            _context = context;

            ulong imageBase = context.File.OptionalHeader.ImageBase;
            bool is32Bit = context.File.OptionalHeader.Magic == OptionalHeaderMagic.Pe32;

            _templateStart = reader.ReadNativeInt(is32Bit);
            _templateEnd = reader.ReadNativeInt(is32Bit);
            Index = context.File.GetReferenceToRva((uint)(reader.ReadNativeInt(is32Bit) - imageBase));
            _addressOfCallbacks = reader.ReadNativeInt(is32Bit);
            SizeOfZeroFill = reader.ReadUInt32();
            Characteristics = (TlsCharacteristics) reader.ReadUInt32();

            ImageBase = imageBase;
            Is32Bit = is32Bit;
        }

        /// <inheritdoc />
        protected override IReadableSegment? GetTemplateData()
        {
            if (_templateEnd < _templateStart)
            {
                return _context.BadImageAndReturn<IReadableSegment>(
                    "End address of TLS template data is smaller than the start address.");
            }

            ulong imageBase = _context.File.OptionalHeader.ImageBase;
            if (!_context.File.TryCreateReaderAtRva((uint) (_templateStart - imageBase), out var reader))
            {
                return _context.BadImageAndReturn<IReadableSegment>(
                    $"TLS template data start address 0x{_templateStart:X8} is invalid.");
            }

            uint length = (uint) (_templateEnd - _templateStart);
            if (!reader.CanRead(length))
            {
                return _context.BadImageAndReturn<IReadableSegment>(
                    $"TLS template data end address 0x{_templateEnd:X8} is invalid.");
            }

            return reader.ReadSegment(length);
        }

        /// <inheritdoc />
        protected override TlsCallbackCollection GetCallbackFunctions()
        {
            var result = new TlsCallbackCollection(this);

            var file = _context.File;
            var optionalHeader = file.OptionalHeader;
            ulong imageBase = optionalHeader.ImageBase;
            bool is32Bit = optionalHeader.Magic == OptionalHeaderMagic.Pe32;

            if (!file.TryCreateReaderAtRva((uint) (_addressOfCallbacks - imageBase), out var reader))
            {
                _context.BadImage($"TLS callback function table start address 0x{_addressOfCallbacks:X8} is invalid.");
                return result;
            }

            while (true)
            {
                if (!reader.CanRead((uint) (is32Bit ? sizeof(uint) : sizeof(ulong))))
                {
                    _context.BadImage($"TLS callback function table does not end with a zero entry.");
                    break;
                }

                ulong address = reader.ReadNativeInt(is32Bit);
                if (address == 0)
                    break;

                result.Add(file.GetReferenceToRva((uint) (address - imageBase)));
            }

            return result;
        }
    }
}
