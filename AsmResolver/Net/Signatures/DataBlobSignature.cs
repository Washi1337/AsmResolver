using System;
using System.Text;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a chunk of raw data stored in the blob stream.
    /// </summary>
    public class DataBlobSignature : BlobSignature
    {
        /// <summary>
        /// Reads all bytes from the stream reader and puts it in a data blob signature.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read data blob.</returns>
        public static DataBlobSignature FromReader(IBinaryStreamReader reader)
        {
            return new DataBlobSignature(reader.ReadBytes((int) reader.Length));
        }

        public DataBlobSignature(byte[] data)
        {
            Data = data;
        }

        /// <summary>
        /// Gets or sets the raw data of the blob.
        /// </summary>
        public byte[] Data
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return (uint)Data.Length;
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteBytes(Data);
        }

        /// <summary>
        /// Attempts to interpret the data as a constant value.
        /// </summary>
        /// <param name="constantType">The type of the constant.</param>
        /// <returns>The constant.</returns>
        /// <exception cref="NotSupportedException">Occurs when the provided type is not supported.</exception>
        public object InterpretData(ElementType constantType)
        {
            switch (constantType)
            {
                case ElementType.Boolean:
                    return Data[0] == 1;
                case ElementType.Char:
                    return (char)BitConverter.ToUInt16(Data, 0);
                case ElementType.I1:
                    return unchecked((sbyte)Data[0]);
                case ElementType.I2:
                    return BitConverter.ToInt16(Data, 0);
                case ElementType.I4:
                    return BitConverter.ToInt32(Data, 0);
                case ElementType.I8:
                    return BitConverter.ToInt64(Data, 0);
                case ElementType.U1:
                    return Data[0];
                case ElementType.U2:
                    return BitConverter.ToUInt16(Data, 0);
                case ElementType.U4:
                    return BitConverter.ToUInt32(Data, 0);
                case ElementType.U8:
                    return BitConverter.ToUInt64(Data, 0);
                case ElementType.R4:
                    return BitConverter.ToSingle(Data, 0);
                case ElementType.R8:
                    return BitConverter.ToDouble(Data, 0);
                case ElementType.String:
                    return Encoding.Unicode.GetString(Data);
                case ElementType.Class:
                    return null;
            }
            throw new NotSupportedException("Unrecognized or unsupported constant type.");
        }

    }
}
