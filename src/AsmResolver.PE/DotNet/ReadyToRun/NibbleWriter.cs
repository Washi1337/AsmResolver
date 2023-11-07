using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    internal struct NibbleWriter
    {
        private readonly IBinaryStreamWriter _writer;
        private byte? _buffer;

        public NibbleWriter(IBinaryStreamWriter writer)
        {
            _writer = writer;
            _buffer = null;
        }

        public void WriteNibble(byte nibble)
        {
            nibble &= 0xF;

            if (_buffer.HasValue)
            {
                _writer.WriteByte((byte) (_buffer.Value | nibble << 4));
                _buffer = null;
            }
            else
            {
                _buffer = nibble;
            }
        }

        public void Write3BitEncodedUInt(uint value)
        {
            int i = 0;
            while ((value >> i) > 7)
                i += 3;

            while (i > 0)
            {
                WriteNibble((byte)(((value >> i) & 0x7) | 0x8));
                i -= 3;
            }

            WriteNibble((byte)(value & 0x7));
        }

        public void Flush()
        {
            if (_buffer is not null)
            {
                _writer.WriteByte(_buffer.Value);
                _buffer = null;
            }
        }
    }
}
