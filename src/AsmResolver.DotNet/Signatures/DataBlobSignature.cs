using System;
// using System.Buffers.Binary;
using System.Text;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents a blob signature storing raw data.
    /// </summary>
    public class DataBlobSignature : BlobSignature
    {
        /// <summary>
        /// Reads a single data blob signature from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The blob signature.</returns>
        public static DataBlobSignature FromReader(ref BinaryStreamReader reader) => new(reader.ReadToEnd());

        /// <summary>
        /// Creates a new data blob signature.
        /// </summary>
        /// <param name="data">The raw data to store.</param>
        public DataBlobSignature(byte[] data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Gets or sets the raw data stored in the blob signature.
        /// </summary>
        public byte[] Data
        {
            get;
            set;
        }

        /// <summary>
        /// Interprets the raw data stored in the <see cref="Data"/> property as a literal.
        /// </summary>
        /// <param name="elementType">The type of the literal.</param>
        /// <returns>The deserialized literal.</returns>
        public object InterpretData(ElementType elementType)
        {
            var reader = new BinaryStreamReader(Data);

            return elementType switch
            {
                ElementType.Boolean => (object) (reader.ReadByte() != 0),
                ElementType.Char => (char) reader.ReadUInt16(),
                ElementType.I1 => reader.ReadSByte(),
                ElementType.U1 => reader.ReadByte(),
                ElementType.I2 => reader.ReadInt16(),
                ElementType.U2 => reader.ReadUInt16(),
                ElementType.I4 => reader.ReadInt32(),
                ElementType.U4 => reader.ReadUInt32(),
                ElementType.I8 => reader.ReadInt64(),
                ElementType.U8 => reader.ReadUInt64(),
                ElementType.R4 => reader.ReadSingle(),
                ElementType.R8 => reader.ReadDouble(),
                ElementType.String => Encoding.Unicode.GetString(reader.ReadToEnd()),
                _ => throw new ArgumentOutOfRangeException(nameof(elementType))
            };
        }

        /// <inheritdoc />
        public override void Write(in BlobSerializationContext context) => context.Writer.WriteBytes(Data);

        /// <summary>
        /// Create a <see cref="DataBlobSignature"/> from a value
        /// </summary>
        /// <param name="value">The value to be converted into data</param>
        /// <returns>
        /// A new <see cref="DataBlobSignature"/> with the correct <see cref="Data"/>
        /// </returns>
        public static DataBlobSignature FromValue(bool value)
        {
            byte[] bytes = new byte[1];
            bytes[0] = (byte)(value ? 1 : 0);
            return new DataBlobSignature(bytes);
        }

        /// <summary>
        /// Create a <see cref="DataBlobSignature"/> from a value
        /// </summary>
        /// <param name="value">The value to be converted into data</param>
        /// <returns>
        /// A new <see cref="DataBlobSignature"/> with the correct <see cref="Data"/>
        /// </returns>
        public static DataBlobSignature FromValue(char value)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(value & 0xFF);
            bytes[1] = (byte)((value >> 8) & 0xFF);
            return new DataBlobSignature(bytes);
        }

        /// <summary>
        /// Create a <see cref="DataBlobSignature"/> from a value
        /// </summary>
        /// <param name="value">The value to be converted into data</param>
        /// <returns>
        /// A new <see cref="DataBlobSignature"/> with the correct <see cref="Data"/>
        /// </returns>
        public static DataBlobSignature FromValue(byte value)
        {
            byte[] bytes = new byte[1];
            bytes[0] = value;
            return new DataBlobSignature(bytes);
        }

        /// <summary>
        /// Create a <see cref="DataBlobSignature"/> from a value
        /// </summary>
        /// <param name="value">The value to be converted into data</param>
        /// <returns>
        /// A new <see cref="DataBlobSignature"/> with the correct <see cref="Data"/>
        /// </returns>
        public static DataBlobSignature FromValue(sbyte value)
        {
            byte[] bytes = new byte[1];
            bytes[0] = unchecked((byte)value);
            return new DataBlobSignature(bytes);
        }

        /// <summary>
        /// Create a <see cref="DataBlobSignature"/> from a value
        /// </summary>
        /// <param name="value">The value to be converted into data</param>
        /// <returns>
        /// A new <see cref="DataBlobSignature"/> with the correct <see cref="Data"/>
        /// </returns>
        public static DataBlobSignature FromValue(ushort value)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(value & 0xFF);
            bytes[1] = (byte)((value >> 8) & 0xFF);
            return new DataBlobSignature(bytes);
        }

        /// <summary>
        /// Create a <see cref="DataBlobSignature"/> from a value
        /// </summary>
        /// <param name="value">The value to be converted into data</param>
        /// <returns>
        /// A new <see cref="DataBlobSignature"/> with the correct <see cref="Data"/>
        /// </returns>
        public static DataBlobSignature FromValue(short value)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(value & 0xFF);
            bytes[1] = (byte)((value >> 8) & 0xFF);
            return new DataBlobSignature(bytes);
        }

        /// <summary>
        /// Create a <see cref="DataBlobSignature"/> from a value
        /// </summary>
        /// <param name="value">The value to be converted into data</param>
        /// <returns>
        /// A new <see cref="DataBlobSignature"/> with the correct <see cref="Data"/>
        /// </returns>
        public static DataBlobSignature FromValue(uint value)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(value & 0xFF);
            bytes[1] = (byte)((value >> 8) & 0xFF);
            bytes[2] = (byte)((value >> 16) & 0xFF);
            bytes[3] = (byte)((value >> 24) & 0xFF);
            return new DataBlobSignature(bytes);
        }

        /// <summary>
        /// Create a <see cref="DataBlobSignature"/> from a value
        /// </summary>
        /// <param name="value">The value to be converted into data</param>
        /// <returns>
        /// A new <see cref="DataBlobSignature"/> with the correct <see cref="Data"/>
        /// </returns>
        public static DataBlobSignature FromValue(int value)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(value & 0xFF);
            bytes[1] = (byte)((value >> 8) & 0xFF);
            bytes[2] = (byte)((value >> 16) & 0xFF);
            bytes[3] = (byte)((value >> 24) & 0xFF);
            return new DataBlobSignature(bytes);
        }

        /// <summary>
        /// Create a <see cref="DataBlobSignature"/> from a value
        /// </summary>
        /// <param name="value">The value to be converted into data</param>
        /// <returns>
        /// A new <see cref="DataBlobSignature"/> with the correct <see cref="Data"/>
        /// </returns>
        public static DataBlobSignature FromValue(ulong value)
        {
            byte[] bytes = new byte[8];
            bytes[0] = (byte)(value & 0xFF);
            bytes[1] = (byte)((value >> 8) & 0xFF);
            bytes[2] = (byte)((value >> 16) & 0xFF);
            bytes[3] = (byte)((value >> 24) & 0xFF);
            bytes[4] = (byte)((value >> 32) & 0xFF);
            bytes[5] = (byte)((value >> 40) & 0xFF);
            bytes[6] = (byte)((value >> 48) & 0xFF);
            bytes[7] = (byte)((value >> 56) & 0xFF);
            return new DataBlobSignature(bytes);
        }

        /// <summary>
        /// Create a <see cref="DataBlobSignature"/> from a value
        /// </summary>
        /// <param name="value">The value to be converted into data</param>
        /// <returns>
        /// A new <see cref="DataBlobSignature"/> with the correct <see cref="Data"/>
        /// </returns>
        public static DataBlobSignature FromValue(long value)
        {
            byte[] bytes = new byte[8];
            bytes[0] = (byte)(value & 0xFF);
            bytes[1] = (byte)((value >> 8) & 0xFF);
            bytes[2] = (byte)((value >> 16) & 0xFF);
            bytes[3] = (byte)((value >> 24) & 0xFF);
            bytes[4] = (byte)((value >> 32) & 0xFF);
            bytes[5] = (byte)((value >> 40) & 0xFF);
            bytes[6] = (byte)((value >> 48) & 0xFF);
            bytes[7] = (byte)((value >> 56) & 0xFF);
            return new DataBlobSignature(bytes);
        }

        /// <summary>
        /// Create a <see cref="DataBlobSignature"/> from a value
        /// </summary>
        /// <param name="value">The value to be converted into data</param>
        /// <returns>
        /// A new <see cref="DataBlobSignature"/> with the correct <see cref="Data"/>
        /// </returns>
        public static DataBlobSignature FromValue(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return new DataBlobSignature(bytes);
        }

        /// <summary>
        /// Create a <see cref="DataBlobSignature"/> from a value
        /// </summary>
        /// <param name="value">The value to be converted into data</param>
        /// <returns>
        /// A new <see cref="DataBlobSignature"/> with the correct <see cref="Data"/>
        /// </returns>
        public static DataBlobSignature FromValue(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return new DataBlobSignature(bytes);
        }

        /// <summary>
        /// Create a <see cref="DataBlobSignature"/> from a value
        /// </summary>
        /// <param name="value">The value to be converted into data</param>
        /// <returns>
        /// A new <see cref="DataBlobSignature"/> with the correct <see cref="Data"/>
        /// </returns>
        public static DataBlobSignature FromValue(string value)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(value);
            return new DataBlobSignature(bytes);
        }
    }
}
