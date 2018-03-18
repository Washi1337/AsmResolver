using System.IO;

namespace AsmResolver
{
    public class BinaryStreamWriter : IBinaryStreamWriter
    {
        private readonly BinaryWriter _writer;

        public BinaryStreamWriter(Stream stream)
        {
            Stream = stream;
            _writer = new BinaryWriter(stream);
        }

        public Stream Stream
        {
            get;
            private set;
        }

        public long Position
        {
            get { return Stream.Position; }
            set
            {
                if (Stream.Length < value)
                {
                    Stream.Position = Stream.Length;
                    this.WriteBytes(new byte[value - Stream.Length]);
                }
                Stream.Position = value;
            }
        }

        public long Length
        {
            get { return Stream.Length; }
        }

        public void WriteBytes(byte[] buffer, int count)
        {
            _writer.Write(buffer, 0, count);
        }

        public void WriteByte(byte value)
        {
            _writer.Write(value);
        }

        public void WriteUInt16(ushort value)
        {
            _writer.Write(value);
        }

        public void WriteUInt32(uint value)
        {
            _writer.Write(value);
        }

        public void WriteUInt64(ulong value)
        {
            _writer.Write(value);
        }

        public void WriteSByte(sbyte value)
        {
            _writer.Write(value);
        }

        public void WriteInt16(short value)
        {
            _writer.Write(value);
        }

        public void WriteInt32(int value)
        {
            _writer.Write(value);
        }

        public void WriteInt64(long value)
        {
            _writer.Write(value);
        }

        public void WriteSingle(float value)
        {
            _writer.Write(value);
        }

        public void WriteDouble(double value)
        {
            _writer.Write(value);
        }
    }
}
