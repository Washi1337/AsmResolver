using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using AsmResolver.Net;

namespace AsmResolver
{
    public interface IBinaryStreamReader
    {
        long StartPosition
        {
            get;
        }

        long Position
        {
            get;
            set;
        }

        long Length
        {
            get;
        }

        IBinaryStreamReader CreateSubReader(long address, int size);
        byte[] ReadBytesUntil(byte value);
        byte[] ReadBytes(int count);
        byte ReadByte();
        ushort ReadUInt16();
        uint ReadUInt32();
        ulong ReadUInt64();
        sbyte ReadSByte();
        short ReadInt16();
        int ReadInt32();
        long ReadInt64();
        float ReadSingle();
        double ReadDouble();
        
    }

    public static class BinaryStreamReaderExtensions
    {
        public static IBinaryStreamReader CreateSubReader(this IBinaryStreamReader reader, long address)
        {
            return reader.CreateSubReader(address, (int)(reader.Length - (address - reader.StartPosition)));
        }

        public static string ReadAsciiString(this IBinaryStreamReader reader)
        {
            return Encoding.ASCII.GetString(reader.ReadBytesUntil(0));
        }

        public static string ReadAlignedAsciiString(this IBinaryStreamReader reader, int align)
        {
            var position = reader.Position;
            var value = reader.ReadAsciiString();
            do
            {
                reader.Position++;
            } while ((reader.Position - position) % align != 0);
            return value;
        }

        public static string ReadSerString(this IBinaryStreamReader reader)
        {
            if (reader.ReadByte() == 0xFF)
                return null;
            reader.Position--;
            var length = reader.ReadCompressedUInt32();
            return Encoding.UTF8.GetString(reader.ReadBytes((int)length));
        }

        public static uint ReadCompressedUInt32(this IBinaryStreamReader reader)
        {
            var firstByte = reader.ReadByte();

            if ((firstByte & 0x80) == 0)
                return firstByte;

            if ((firstByte & 0x40) == 0)
                return (uint)(((firstByte & 0x7F) << 8) | reader.ReadByte());

            return (uint)(((firstByte & 0x3F) << 0x18) |
                          (reader.ReadByte() << 0x10) |
                          (reader.ReadByte() << 0x08) |
                          reader.ReadByte());
        }

        public static int ReadCompressedInt32(this IBinaryStreamReader reader)
        {
            unchecked
            {
                var value = (int)ReadCompressedUInt32(reader);
                return (((value & 1) != 0) ? -(value >> 1) : (value >> 1));
            }
        }

        public static uint ReadIndex(this IBinaryStreamReader reader, IndexSize size)
        {
            switch (size)
            {
                case IndexSize.Short:
                    return reader.ReadUInt16();
                case IndexSize.Long:
                    return reader.ReadUInt32();
                default:
                    throw new ArgumentOutOfRangeException("size");
            }
        }

        public static void Align(this IBinaryStreamReader reader, int align)
        {
            align--;
            reader.ReadBytes((((int)reader.Position + align) & ~align) - (int)reader.Position);
        }
    }
}
