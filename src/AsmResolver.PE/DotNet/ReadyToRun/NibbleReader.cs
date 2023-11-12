using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    internal struct NibbleReader
    {
        private BinaryStreamReader _reader;
        private byte? _buffer;

        public NibbleReader(BinaryStreamReader reader)
        {
            _reader = reader;
            _buffer = null;
        }

        public BinaryStreamReader BaseReader => _reader;

        public byte ReadNibble()
        {
            if (!_buffer.HasValue)
            {
                _buffer = _reader.ReadByte();
                return (byte) (_buffer & 0xF);
            }

            byte value = (byte) ((_buffer >> 4) & 0xF);
            _buffer = null;
            return value;
        }

        public uint Read3BitEncodedUInt()
        {
            uint result = 0;
            byte nibble;

            do
            {
                nibble = ReadNibble();
                result <<= 3;
                result |= (uint) (nibble & 0b0111);
            } while ((nibble & 0b1000) != 0);

            return result;
        }
    }
}
