using System;
using System.Text;
using AsmResolver.DotNet.Builder;
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
        public static DataBlobSignature FromReader(IBinaryStreamReader reader)
        {
            return new DataBlobSignature(reader.ReadToEnd());
        }

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
            var reader = new ByteArrayReader(Data);

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
        public override void Write(BlobSerializationContext context) => context.Writer.WriteBytes(Data);
    }
}