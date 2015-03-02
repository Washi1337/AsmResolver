using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
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
        public static bool CanRead(this IBinaryStreamReader reader, int size)
        {
            return (reader.Position - reader.StartPosition) + size <= reader.Length;
        }

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
            uint length;
            if (!reader.TryReadCompressedUInt32(out length))
                return null;
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

        public static bool TryReadCompressedUInt32(this IBinaryStreamReader reader, out uint value)
        {
            value = 0;
            if (!reader.CanRead(sizeof(byte)))
                return false;

            var firstByte = reader.ReadByte();
            reader.Position--;

            if (((firstByte & 0x80) == 0 && reader.CanRead(sizeof(byte))) ||
                ((firstByte & 0x40) == 0 && reader.CanRead(sizeof(ushort))) ||
                (reader.CanRead(sizeof(uint))))
            {
                value = ReadCompressedUInt32(reader);
                return true;
            }

            return false;
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
