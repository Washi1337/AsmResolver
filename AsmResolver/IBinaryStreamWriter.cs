using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net;

namespace AsmResolver
{
    public interface IBinaryStreamWriter
    {
        long Position
        {
            get;
            set;
        }

        long Length
        {
            get;
        }

        void WriteBytes(byte[] buffer, int count);
        void WriteByte(byte value);
        void WriteUInt16(ushort value);
        void WriteUInt32(uint value);
        void WriteUInt64(ulong value);
        void WriteSByte(sbyte value);
        void WriteInt16(short value);
        void WriteInt32(int value);
        void WriteInt64(long value);
        void WriteSingle(float value);
        void WriteDouble(double value);

    }

    public static class OutputStreamExtensions
    {
        public static void WriteBytes(this IBinaryStreamWriter writer, byte[] buffer)
        {
            writer.WriteBytes(buffer, buffer.Length);
        }

        public static void WriteZeroes(this IBinaryStreamWriter writer, int count)
        {
            writer.WriteBytes(new byte[count]);
        }

        public static void WriteAsciiString(this IBinaryStreamWriter writer, string value)
        {
            writer.WriteBytes(Encoding.ASCII.GetBytes(value));
        }

        public static void WriteSerString(this IBinaryStreamWriter writer, string value)
        {
            if (value == null)
            {
                writer.WriteByte(0xFF);
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(value);
            writer.WriteCompressedUInt32((uint)bytes.Length);
            writer.WriteBytes(bytes);
        }

        public static uint GetCompressedSize(this uint value)
        {
            if (value < 0x80)
                return 1;
            if (value < 0x4000)
                return 2;
            return 4;
        }

        public static uint GetCompressedSize(this int value)
        {
            return ((uint)value).GetCompressedSize();
        }

        public static uint GetSerStringSize(this string value)
        {
            var byteCount = (uint)Encoding.UTF8.GetByteCount(value);
            return byteCount.GetCompressedSize() + byteCount;
        }

        public static void WriteCompressedUInt32(this IBinaryStreamWriter writer, uint value)
        {
            if (value < 0x80)
                writer.WriteByte((byte)value);
            else if (value < 0x4000)
            {
                writer.WriteByte((byte)(0x80 | value >> 8));
                writer.WriteByte((byte)(value & 0xFF));
            }
            else
            {
                writer.WriteByte((byte)(0x80 | 0x40 | value >> 0x18));
                writer.WriteByte((byte)(value >> 0x10 & 0xFF));
                writer.WriteByte((byte)(value >> 0x08 & 0xFF));
                writer.WriteByte((byte)(value & 0xFF));
            }
        }

        public static void WriteIndex(this IBinaryStreamWriter writer, IndexSize indexSize, uint value)
        {
            if (indexSize == IndexSize.Short)
                writer.WriteUInt16((ushort)value);
            else
                writer.WriteUInt32(value);
        }
    }
}
